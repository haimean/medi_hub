using System.Data;
using Npgsql;

namespace DashboardApi.DatabaseContext.DapperDbContext
{
   public class AppMainDapperContext
    {
      private readonly IConfiguration _configuration;
      private readonly string _connectionString;

      public AppMainDapperContext(IConfiguration configuration)
      {
         _configuration = configuration;
         _connectionString = Environment.GetEnvironmentVariable("APPMAINCONNECTIONSTRING");
      }

      public IDbConnection CreateConnection()
         => new NpgsqlConnection(_connectionString);
   }
}
