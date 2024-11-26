using Npgsql;
using System.Data;

namespace MediHub.Web.DatabaseContext.DapperDbContext
{
    public class MediHubDapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public MediHubDapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = Environment.GetEnvironmentVariable("MediHubConnectionString");
        }

        public IDbConnection CreateConnection()
           => new NpgsqlConnection(_connectionString);
    }
}
