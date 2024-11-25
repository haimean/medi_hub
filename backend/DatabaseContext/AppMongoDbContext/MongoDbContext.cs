using DashboardApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DashboardApi.DatabaseContext.AppMongoDbContext
{
    public class MongoDbContext
    {
        public IMongoDatabase AppMongoDbContext { get; set; }
        public IMongoCollection<BimElementEntity> BimDataCollection { get; set; }

        public MongoDbContext(IOptions<MongoDbSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionUri);
            AppMongoDbContext = client.GetDatabase(mongoSettings.Value.DatabaseName);

            BimDataCollection = AppMongoDbContext.GetCollection<BimElementEntity>(nameof(BimElementEntity));
        }
    }
}
