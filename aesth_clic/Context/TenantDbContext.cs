using aesth_clic.Tenant.Model;
using Microsoft.EntityFrameworkCore;

namespace aesth_clic.Context
{
    public class TenantDbContext : DbContext
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<AccountStatus> AccountsStatus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccountStatus>()
                .HasOne(a => a.User)
                .WithOne(u => u.AccountStatus)
                .HasForeignKey<AccountStatus>(a => a.AccountId)
                .IsRequired(false)  // <-- optional
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}