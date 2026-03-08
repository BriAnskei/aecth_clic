using aesth_clic.Context;
using aesth_clic.Session;
using aesth_clic.Tenant.DTO;
using aesth_clic.Tenant.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Services
{
    public sealed class PrescriptionService : TenantServiceBase
    {
        public async Task<Prescription> CreateAsync(
     TenantDbContext db,
     int patientProcedureId,
     List<PrescriptionMedicineDto> medicines)
        {
            if (patientProcedureId <= 0)
                throw new ArgumentException("PatientProcedureId is required.");

            if (medicines == null || medicines.Count == 0)
                throw new ArgumentException("At least one medicine must be provided.");

            foreach (var m in medicines)
            {
                if (m.MedicineId <= 0) throw new ArgumentException("Invalid MedicineId.");
                if (m.Quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
            }

            var prescription = new Prescription
            {
                PatientProcedureId = patientProcedureId,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            db.Set<Prescription>().Add(prescription);
            await db.SaveChangesAsync();

            foreach (var m in medicines)
            {
                var patientMedicine = new PatientMedicine
                {
                    PrescriptionId = prescription.Id,
                    MedicineId = m.MedicineId,
                    Quantity = m.Quantity
                };

                db.Set<PatientMedicine>().Add(patientMedicine);
            }

            await db.SaveChangesAsync();

            return prescription;
        }


        public async Task<List<Prescription>> GetAllAsync()
        {
            using var db = CreateTenantDb();

            return await db.Set<Prescription>()
                .Where(p => p.Status.Equals("pending", StringComparison.OrdinalIgnoreCase)) // only pending
                .Include(p => p.PatientProcedure!)
                    .ThenInclude(pp => pp.User)          // assigned doctor
                .Include(p => p.PatientProcedure!)
                    .ThenInclude(pp => pp.Patient)       // patient
                .Include(p => p.PatientMedicines)        // medicines
                    .ThenInclude(pm => pm.Medicine)      // actual medicine details
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<Prescription?> GetByProcedureIdAsync(int patientProcedureId)
        {
            if (patientProcedureId <= 0)
                throw new ArgumentException("Invalid PatientProcedureId.");

            using var db = CreateTenantDb();

            // Fetch the prescription for this procedure, including all related data
            var prescription = await db.Set<Prescription>()
                .Where(p => p.PatientProcedureId == patientProcedureId)  // only for this procedure
                .Include(p => p.PatientProcedure!)                        // include procedure
                    .ThenInclude(pp => pp.User)                           // assigned doctor
                .Include(p => p.PatientProcedure!)
                    .ThenInclude(pp => pp.Patient)                        // patient
                .Include(p => p.PatientMedicines)                        // all patient medicines
                    .ThenInclude(pm => pm.Medicine)                      // include actual medicine details
                .AsNoTracking()
                .FirstOrDefaultAsync();                                   // get single prescription

            return prescription;
        }


    }
}