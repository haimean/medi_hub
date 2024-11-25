using Npgsql;
using System.Data;

namespace DashboardApi.DatabaseContext.DapperDbContext
{
    public class QaQcDapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public QaQcDapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = Environment.GetEnvironmentVariable("QAQCCONNECTIONSTRING");
        }

        public IDbConnection CreateConnection()
           => new NpgsqlConnection(_connectionString);
    }
}
