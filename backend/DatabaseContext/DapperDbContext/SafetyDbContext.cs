using Npgsql;
using System.Data;

namespace DashboardApi.DatabaseContext.DapperDbContext
{
    public class SafetyDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public SafetyDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = Environment.GetEnvironmentVariable("SAFETYCONNECTIONSTRING");
        }

        public IDbConnection CreateConnection()
          => new NpgsqlConnection(_connectionString);

          
    }
}
