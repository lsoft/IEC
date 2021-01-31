using System.Data.SqlClient;

namespace IEC.Target.SqlServer.Helper
{
    public static class DatabaseHelper
    {
        public static void SafelyDispose(
            this SqlCommand? command
            )
        {
            if (command is null)
            {
                return;
            }

            try
            {
                command.Dispose();
            }
            catch
            {
                //nothing to do
            }
        }

        public static void SafelyRollback(
            this SqlTransaction? transaction
            )
        {
            if (transaction is null)
            {
                return;
            }

            try
            {
                transaction.Rollback();
            }
            catch
            {
                //nothing to do
            }
        }

        public static void SafelyCommit(
            this SqlTransaction? transaction
            )
        {
            if (transaction is null)
            {
                return;
            }

            try
            {
                transaction.Commit();
            }
            catch
            {
                //nothing to do
            }
        }

        public static void SafelyDispose(
            this SqlTransaction? transaction
            )
        {
            if (transaction is null)
            {
                return;
            }

            try
            {
                transaction.Dispose();
            }
            catch
            {
                //nothing to do
            }
        }

        public static void SafelyClose(
            this SqlConnection? connection
            )
        {
            if (connection is null)
            {
                return;
            }

            try
            {
                connection.Close();
            }
            catch
            {
                //nothing to do
            }
        }

        public static void SafelyDispose(
            this SqlConnection? connection
            )
        {
            if (connection is null)
            {
                return;
            }

            try
            {
                connection.Dispose();
            }
            catch
            {
                //nothing to do
            }
        }

    }
}
