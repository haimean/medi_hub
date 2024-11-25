using System.Data;
using Npgsql;

namespace DashboardApi.DatabaseContext.DapperDbContext
{
   public class JotDapperContext
   {
      private readonly IConfiguration _configuration;
      private readonly string _connectionString;

      public JotDapperContext(IConfiguration configuration)
      {
         _configuration = configuration;
         _connectionString = Environment.GetEnvironmentVariable("JOTCONNECTIONSTRING");
      }

      public IDbConnection CreateConnection()
         => new NpgsqlConnection(_connectionString);
   }
}
