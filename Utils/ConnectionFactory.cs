using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace smrp.Utils
{
    public class DefaultConnection
    {
        private readonly string connectionString;

        public DefaultConnection(IConfiguration config)
        {
            connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public IDbConnection CreateConnection()
        {
            var con = new NpgsqlConnection(connectionString);
            con.Open();
            return con;
        }
    }

    public class RsConnection
    {
        private readonly string connectionString;

        public RsConnection(IConfiguration config)
        {
            connectionString = config.GetConnectionString("RsConnection") ?? "";
        }

        public IDbConnection CreateConnection()
        {
            var con = new OracleConnection(connectionString);
            con.Open();
            return con;
        }
    }
}
