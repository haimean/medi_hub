using System.Data;
using Npgsql;

namespace DashboardApi.DatabaseContext.DapperDbContext
{
    public class WorkerDapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _connectionStringQaQc;

        public WorkerDapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = Environment.GetEnvironmentVariable("WORKERCONNECTIONSTRING");
        }

        public IDbConnection CreateConnection()
           => new NpgsqlConnection(_connectionString);
    }
}
