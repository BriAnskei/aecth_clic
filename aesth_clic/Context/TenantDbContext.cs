using aesth_clic.Tenant.Model;
using Microsoft.EntityFrameworkCore;

namespace aesth_clic.Context
{
    public class TenantDbContext : DbContext
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<AccountStatus> AccountsStatus { get; set; }
        public DbSet<ServiceMenu> ServiceMenu { get; set; }
        public DbSet<TncTenant> TncTenants { get; set; }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<PatientProcedure> PatientProcedures { get; set; }


        public DbSet<Medicine> Medicines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccountStatus>()
                .HasOne(a => a.User)
                .WithOne(u => u.AccountStatus)
                .HasForeignKey<AccountStatus>(a => a.AccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);


            // service -> docter
            modelBuilder.Entity<ServiceMenu>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.AddedBy);


            // patient procedure
            modelBuilder.Entity<PatientProcedure>()
           .HasOne(p => p.User)
           .WithMany()
           .HasForeignKey(p => p.AssignedDoctorId)
           .IsRequired(false) // optional relationship
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientProcedure>()
                .HasOne(p => p.ServiceMenu)
                .WithMany()
                .HasForeignKey(p => p.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientProcedure>()
                .HasOne(p => p.Patient)
                .WithMany()
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}