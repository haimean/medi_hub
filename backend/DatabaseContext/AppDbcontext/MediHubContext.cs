using Microsoft.EntityFrameworkCore;

namespace QAQCApi.DatabaseContext.AppDbcontext
{
    public class MediHubContext : DbContext
    {
        // public DbSet<BimElementEntity> BimElementEntities { get; set; }

        public MediHubContext(DbContextOptions options)
           : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);

            /*builder.Entity<CTA>(x =>
            {
                x.HasOne(x => x.EmailInfo)
                .WithMany()
                .HasForeignKey(x => x.EmailInfoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
            });*/
        }
    }
}
