using Npgsql;
using System.Data;

namespace DashboardApi.DatabaseContext.DapperDbContext
{
    public class MaintenanceDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public MaintenanceDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = Environment.GetEnvironmentVariable("MAINTENANCE_CONNECTIONSTRING");
        }

        public IDbConnection CreateConnection()
          => new NpgsqlConnection(_connectionString);
    }
}
