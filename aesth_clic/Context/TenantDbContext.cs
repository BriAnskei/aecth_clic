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

        public DbSet<Prescription> Prescriptions { get; set; }

        public DbSet<PatientProcedure> PatientProcedures { get; set; }


        public DbSet<Medicine> Medicines { get; set; }

        public DbSet<PatientMedicine> PatientMedicines { get; set; }
        public DbSet<ProcedurePayment> ProcedurePayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccountStatus>()
    .HasOne(a => a.User)
    .WithOne(u => u.AccountStatus)
    .HasForeignKey<AccountStatus>(a => a.AccountId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<AccountStatus>()
    .HasIndex(a => a.AccountId)
    .IsUnique();


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

            modelBuilder.Entity<PatientProcedure>()
     .HasOne(p => p.Prescription)
     .WithOne(pr => pr.PatientProcedure)
     .HasForeignKey<Prescription>(pr => pr.PatientProcedureId)
     .IsRequired()   // cannot be optional
     .OnDelete(DeleteBehavior.Cascade);


            //// prescription
            //modelBuilder.Entity<Prescription>()
            //.HasOne(p => p.PatientProcedure)
            //.WithMany()
            //.HasForeignKey(p => p.PatientProcedureId)
            //.OnDelete(DeleteBehavior.Cascade);


            // patient medicine
            modelBuilder.Entity<PatientMedicine>()
             .HasOne(pm => pm.Medicine)
             .WithMany()
             .HasForeignKey(pm => pm.MedicineId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientMedicine>()
             .HasOne(pm => pm.Prescription)
             .WithMany(p => p.PatientMedicines)
             .HasForeignKey(pm => pm.PrescriptionId)
             .OnDelete(DeleteBehavior.Cascade);




            // prrocedure payments
            modelBuilder.Entity<ProcedurePayment>()
                .HasOne(pp => pp.PatientProcedure)
                .WithMany() // no navigation property
                .HasForeignKey(pp => pp.PatientProcedureId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProcedurePayment>()
                .HasOne(pp => pp.ServiceMenu)
                .WithMany() // no navigation property
                .HasForeignKey(pp => pp.ServiceMenuId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}