using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using IEC.Common.Other;
using IEC.Common.Publish.Saver;
using IEC.Target.SqlServer.Helper;

namespace IEC.Target.SqlServer
{
    public class SqlServerCollectionSaver : CollectionSaver
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;
        private readonly TargetFileNameController _targetController;
        private readonly SqlCommand _command;

        private long _cleanup = 0L;

        /// <inheritdoc />
        public SqlServerCollectionSaver(
            SqlConnection connection,
            SqlTransaction transaction,
            FullTableName tableName,
            TargetFileNameController targetController
            )
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (targetController == null)
            {
                throw new ArgumentNullException(nameof(targetController));
            }

            _connection = connection;
            _transaction = transaction;
            _targetController = targetController;

            _command = new SqlCommand($@"
insert into {tableName.CombinedName}
    ( insert_date, file_name, offset, length )
values
    ( GETDATE(), @fileName, @offset, @length )
",
                connection,
                transaction
                );
            _command.Parameters.Add("fileName", SqlDbType.VarChar, 261);
            _command.Parameters.Add("offset", SqlDbType.BigInt);
            _command.Parameters.Add("length", SqlDbType.BigInt);
            _command.Prepare();

        }

        /// <inheritdoc />
        protected override void LogCompleteMessage(
            string message
            )
        {
            //zip message...
            byte[] zippedMessage;
            using (var targetStream = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                {
                    using (var sw = new StreamWriter(compressionStream, Encoding.UTF8))
                    {
                        sw.Write(message);
                    }
                }

                zippedMessage = targetStream.ToArray();
            }

            //determine target file...
            var (targetFilePath, targetFolderPath, targetFileName) = _targetController.GetTargetFile();

            //append data
            var offset = 0L;
            lock (TargetFileLocker.GetFileLocker(targetFilePath))
            {
                using (var fs = new FileStream(targetFilePath, FileMode.Append, FileAccess.Write))
                {
                    offset = fs.Position;
                    fs.Write(zippedMessage, 0, zippedMessage.Length);
                    fs.Flush();
                }

                //we should still hold the file lock here for the case of revert
                //otherwise different thread (different SqlServerCollectionSaver)
                //can append their data between append ours and revert ours
                //in that case we will truncate both records and DB-metadata will be completely invalid

                //save metadata to DB
                try
                {
                    _command.Parameters["fileName"].Value = targetFileName.GetFromLast(261);
                    _command.Parameters["offset"].Value = offset;
                    _command.Parameters["length"].Value = zippedMessage.Length;
                    _command.ExecuteNonQuery();
                }
                catch (Exception firstException)
                {
                    //shit happens...

                    //try to revert the file to its original state
                    try
                    {
                        using (var fs = new FileStream(targetFilePath, FileMode.Open, FileAccess.ReadWrite))
                        {
                            fs.SetLength(offset);
                        }
                    }
                    catch (Exception secondException)
                    {
                        //reverting fails, this file is unrecoverable
                        //force to switch the file!
                        _targetController.SwitchTargetFile();

                        throw new AggregateException(firstException, secondException);
                    }

                    throw;
                }
            }

            //success

        }

        /// <inheritdoc />
        public override void Commit()
        {
            if (Interlocked.Exchange(ref _cleanup, 1L) != 0L)
            {
                return;
            }

            _command.SafelyDispose();
            _transaction.SafelyCommit();
            _transaction.SafelyDispose();
            _connection.SafelyClose();
            _connection.SafelyDispose();
        }

        /// <inheritdoc />
        protected override void DoDispose()
        {
            if (Interlocked.Exchange(ref _cleanup, 1L) != 0L)
            {
                return;
            }

            _command.SafelyDispose();
            _transaction.SafelyRollback();
            _transaction.SafelyDispose();
            _connection.SafelyClose();
            _connection.SafelyDispose();
        }
    }
}
