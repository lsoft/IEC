using System;
using System.Data.SqlClient;
using IEC.Common.Publish.Saver;
using IEC.Target.SqlServer.Helper;

namespace IEC.Target.SqlServer
{
    public class SqlServerCollectionSaverFactory : ICollectionSaverFactory
    {
        private readonly string _connectionString;
        private readonly FullTableName _tableName;
        private readonly TargetFileNameController _targetController;

        public SqlServerCollectionSaverFactory(
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

        public void PrepareDatabase()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command0 = new SqlCommand($@"
select
	1
from sys.tables t
join sys.schemas  s on s.schema_id = t.schema_id
where
	t.[name] = '{_tableName.TableName}'
	and s.[name] = '{_tableName.SchemaName}'
	and [type] = 'U'
",
                    connection
                    ))
                {

                    var ro = command0.ExecuteScalar();

                    if (ro is not null)
                    {
                        return;
                    }
                }

                using (var command1 = new SqlCommand($@"
CREATE TABLE {_tableName.CombinedName}
(
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[insert_date] [datetime] NOT NULL,
	[file_name] [varchar](261) NOT NULL,
	[offset] [bigint] NOT NULL,
	[length] [bigint] NOT NULL,
CONSTRAINT [PK_{_tableName.GuardedCombinedName}] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
",
                    connection
                    ))
                {
                    command1.ExecuteNonQuery();
                }


                using (var command2 = new SqlCommand($@"
CREATE NONCLUSTERED INDEX [IX_{_tableName.GuardedCombinedName}_insert_date] ON {_tableName.CombinedName}
(
	[insert_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
",
                    connection
                    ))
                {
                    command2.ExecuteNonQuery();
                }
            }
        }

        /// <inheritdoc />
        public ICollectionSaver CreateCollectionSaver()
        {
            if (_targetController.Cleanup(out var border))
            {
                //something were deleted from the file system
                //so we need to delete obsolete records from the DB

                CleanupDatabase(border);
            }

            SqlConnection? connection = null;
            SqlTransaction? transaction = null;
            try
            {
                connection = new SqlConnection(_connectionString);
                connection.Open();

                transaction = connection.BeginTransaction();

                return new SqlServerCollectionSaver(
                    connection,
                    transaction,
                    _tableName,
                    _targetController
                    );
            }
            catch
            {
                transaction?.SafelyDispose();
                connection?.SafelyDispose();

                throw;
            }
        }

        private void CleanupDatabase(DateTime border)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command0 = new SqlCommand($@"
delete from {_tableName.CombinedName}
where
    insert_date < '{border:yyyyMMdd}'
",
                        connection,
                        transaction
                        ))
                    {

                        command0.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
