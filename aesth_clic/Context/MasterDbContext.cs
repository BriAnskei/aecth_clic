using aesth_clic.Master.Model;


using Microsoft.EntityFrameworkCore;

namespace aesth_clic.Context
{
    public class MasterDbContext(DbContextOptions<MasterDbContext> options) : DbContext(options)
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Admin> Admins { get; set; }
   
    }
}