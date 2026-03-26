
using Microsoft.Data.SqlClient;

namespace backend.DataAccess
{
    public class BaseDataAccess
    {
        private readonly string _connectionString;
        public BaseDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        protected SqlConnection GetSqlConnection()
        {
            return new SqlConnection(_connectionString);
        }

        protected SqlCommand GetSqlCommand(string sql, SqlConnection connection, SqlTransaction? transaction = null)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = transaction;
            return cmd;
        }

        protected (SqlConnection, SqlTransaction) CreateConnectionAndTransaction()
        {
            var cxn = GetSqlConnection();
            cxn.Open();
            var tx = cxn.BeginTransaction();
            return (cxn, tx);
        }
    }
}