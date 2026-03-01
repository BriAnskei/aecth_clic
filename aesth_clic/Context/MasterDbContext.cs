using aesth_clic.Master.Model;
using aesth_clic.Models; // wherever Client and Subscription are located

using Microsoft.EntityFrameworkCore;

namespace aesth_clic.Context
{
    public class MasterDbContext(DbContextOptions<MasterDbContext> options) : DbContext(options)
    {
        public DbSet<Client> Clients { get; set; }
   
    }
}