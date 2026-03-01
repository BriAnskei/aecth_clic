using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using aesth_clic.Context;

namespace aesth_clic.Context
{
    // This is only for EF CLI
    public class TenantDbDesignTimeFactory : IDesignTimeDbContextFactory<TenantDbContext>
    {
        public TenantDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

            // Use a default database for design-time migrations
            string connectionString = "Server=localhost\\SQLEXPRESS;Database=Tenant_DesignTime;Trusted_Connection=True;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString);

            return new TenantDbContext(optionsBuilder.Options);
        }
    }
}
