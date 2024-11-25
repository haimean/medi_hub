using Npgsql;
using System.Data;

namespace DashboardApi.DatabaseContext.DapperDbContext
{
    public class ResourceCommonDapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _connectionStringQaQc;

        public ResourceCommonDapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = Environment.GetEnvironmentVariable("RESOURCECOMMONCONNECTIONSTRING");
        }

        public IDbConnection CreateConnection()
           => new NpgsqlConnection(_connectionString);
    }
}
