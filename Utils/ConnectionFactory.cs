using Npgsql;
using System.Data;

namespace smrp.Utils
{
    public interface IConnectionFactory
    {
        IDbConnection CreateDefaultConnection();
        IDbConnection CreateRsConnection();
    }

    public class ConnectionFactory : IConnectionFactory
    {
        private readonly IConfiguration configuration;

        public ConnectionFactory(IConfiguration config)
        {
            configuration = config;
        }

        public IDbConnection CreateDefaultConnection()
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            return new NpgsqlConnection(connectionString);
        }

        public IDbConnection CreateRsConnection()
        {
            var connectionString = configuration.GetConnectionString("RsConnection");
            return new NpgsqlConnection(connectionString);
        }
    }
}
