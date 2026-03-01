using aesth_clic.Tenant.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Context
{
    public class TenantDbContext(DbContextOptions<TenantDbContext> options) : DbContext(options)
    {

        public DbSet<User> Users { get; set; }
    }
}
