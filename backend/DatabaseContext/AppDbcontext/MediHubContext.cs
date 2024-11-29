using MediHub.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace MediHub.Web.DatabaseContext.AppDbcontext
{
    public class MediHubContext : DbContext
    {
        #region DB SET
        public DbSet<DeviceEntity> DeviceEntity { get; set; }
        public DbSet<UserEntity> UserEntity { get; set; }
        #endregion

        #region Config context
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
        #endregion
    }
}
