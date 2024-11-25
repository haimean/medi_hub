using DashboardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DashboardApi.DatabaseContext.AppDbcontext
{
    public class WorkerDbContext : DbContext
    {
        public DbSet<BimElementEntity> BimElementEntities { get; set; }
        public WorkerDbContext(DbContextOptions options)
           : base(options)
        {
        }

    }
}
