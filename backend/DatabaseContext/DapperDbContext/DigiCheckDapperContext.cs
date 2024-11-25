using Npgsql;
using System.Data;

namespace DashboardApi.DatabaseContext.DapperDbContext
{
    public class DigiCheckDapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DigiCheckDapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = Environment.GetEnvironmentVariable("DIGICONNECTIONSTRING");
        }

        public IDbConnection CreateConnection()
           => new NpgsqlConnection(_connectionString);
    }
}
