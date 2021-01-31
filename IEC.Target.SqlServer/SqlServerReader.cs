using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using IEC.Common.Read;

namespace IEC.Target.SqlServer
{
    public class SqlServerReader : IReader
    {
        private readonly string _connectionString;
        private readonly FullTableName _tableName;
        private readonly TargetFileNameController _targetController;

        public SqlServerReader(
            string connectionString,
            FullTableName tableName,
            TargetFileNameController targetController
            )
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (targetController == null)
            {
                throw new ArgumentNullException(nameof(targetController));
            }

            _connectionString = connectionString;
            _tableName = tableName;
            _targetController = targetController;
        }

        /// <inheritdoc />
        public List<StoredFrame> ReadBetween(
            DateTime inclusiveFrom,
            DateTime exclusiveTo
            )
        {
            var frames = ReadFramesFromDb(inclusiveFrom, exclusiveTo);

            var frameGroups = (
                from frame in frames
                group frame by frame.FilePath into frameGroup
                select frameGroup
                ).ToList();

            foreach (var frameGroup in frameGroups)
            {
                lock(TargetFileLocker.GetFileLocker(frameGroup.Key))
                {
                    using (var fs = File.OpenRead(frameGroup.Key))
                    {
                        foreach (var frame in frameGroup.OrderBy(f => f.Offset))
                        {
                            fs.Position = frame.Offset;

                            var compressedBody = new byte[frame.Length];
                            fs.Read(compressedBody, 0, compressedBody.Length);

                            using (var targetStream = new MemoryStream(compressedBody))
                            {
                                using (var decompressionStream = new GZipStream(targetStream, CompressionMode.Decompress))
                                {
                                    using (var sr = new StreamReader(decompressionStream, Encoding.UTF8))
                                    {
                                        var body = sr.ReadToEnd();
                                        frame.AppendBody(body);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return frames;
        }

        private List<StoredFrame> ReadFramesFromDb(
            DateTime inclusiveFrom,
            DateTime exclusiveTo
            )
        {
            var frames = new List<StoredFrame>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new SqlCommand($@"
select
    insert_date, 
    file_name, 
    offset, 
    length
from {_tableName.CombinedName}
where
    insert_date >= @since
    and insert_date < @to
",
                        connection,
                        transaction
                        ))
                    {
                        command.Parameters.Add("since", SqlDbType.DateTime);
                        command.Parameters.Add("to", SqlDbType.DateTime);
                        command.Prepare();

                        command.Parameters["since"].Value = inclusiveFrom;
                        command.Parameters["to"].Value = exclusiveTo;

                        using (var r = command.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                var insertDate = (DateTime) r["insert_date"];
                                var fileName = (string) r["file_name"];
                                var offset = (long) r["offset"];
                                var length = (long) r["length"];

                                var frame = new StoredFrame(
                                    insertDate,
                                    Path.Combine(_targetController.TargetFolderPath, fileName),
                                    offset,
                                    length
                                    );
                                frames.Add(frame);
                            }
                        }
                    }
                }
            }

            return frames;
        }
    }
}
