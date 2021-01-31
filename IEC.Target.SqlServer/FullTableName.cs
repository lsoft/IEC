using System;

namespace IEC.Target.SqlServer
{
    public class FullTableName
    {
        public string TableName
        {
            get;
        }

        public string SchemaName
        {
            get;
        }

        public string CombinedName
        {
            get
            {
                return $"{SchemaName}.{TableName}";
            }
        }

        public string GuardedCombinedName
        {
            get
            {
                return $"{SchemaName}_{TableName}";
            }
        }

        public FullTableName(
            string tableName,
            string schemaName = "dbo"
            )
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (schemaName == null)
            {
                throw new ArgumentNullException(nameof(schemaName));
            }

            if (tableName.Contains("."))
            {
                throw new ArgumentException("table name contains incorrect symbol");
            }

            if (schemaName.Contains("."))
            {
                throw new ArgumentException("schema name contains incorrect symbol");
            }

            TableName = tableName.TrimStart('[').TrimEnd(']');
            SchemaName = schemaName.TrimStart('[').TrimEnd(']');
        }
    }
}
