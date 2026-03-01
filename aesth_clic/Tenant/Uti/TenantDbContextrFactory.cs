using aesth_clic.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Uti
{
    public class TenantDbContextFactory
    {
        public TenantDbContext Create(string databaseName)
        {
            var options = new DbContextOptionsBuilder<TenantDbContext>();

            string connection =
       $"Server=localhost\\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;";
            options.UseSqlServer(connection);

            return new TenantDbContext(options.Options);
        }
    }
}
