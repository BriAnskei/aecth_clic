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
                .Where(p => p.Status == "pending") // ✅ fixed
                .Include(p => p.PatientProcedure!)
                    .ThenInclude(pp => pp.User)
                .Include(p => p.PatientProcedure!)
                    .ThenInclude(pp => pp.Patient)
                .Include(p => p.PatientMedicines)
                    .ThenInclude(pm => pm.Medicine)
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


        public async Task<Prescription> UpdateAsync(
     int patientProcedureId,
     List<PrescriptionMedicineDto> medicines)
        {
            if (patientProcedureId <= 0)
                throw new ArgumentException("Invalid PatientProcedureId.");

            if (medicines == null || medicines.Count == 0)
                throw new ArgumentException("At least one medicine must be provided.");

            foreach (var m in medicines)
            {
                if (m.MedicineId <= 0)
                    throw new ArgumentException("Invalid MedicineId.");

                if (m.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero.");
            }

            using var db = CreateTenantDb();
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var prescription = await db.Set<Prescription>()
                    .Include(p => p.PatientMedicines)
                    .FirstOrDefaultAsync(p => p.PatientProcedureId == patientProcedureId);

                if (prescription == null)
                    throw new Exception("Prescription not found.");

                // 1️⃣ Delete existing medicines
                if (prescription.PatientMedicines.Any())
                {
                    db.Set<PatientMedicine>().RemoveRange(prescription.PatientMedicines);
                }

                // 2️⃣ Add new medicines
                var newMedicines = medicines.Select(m => new PatientMedicine
                {
                    PrescriptionId = prescription.Id,
                    MedicineId = m.MedicineId,
                    Quantity = m.Quantity
                }).ToList();

                await db.Set<PatientMedicine>().AddRangeAsync(newMedicines);

                await db.SaveChangesAsync();

                await transaction.CommitAsync();

                return prescription;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<Prescription> MarkCompletedAsync(int patientProcedureId)
        {
            if (patientProcedureId <= 0)
                throw new ArgumentException("Invalid PatientProcedureId.");

            using var db = CreateTenantDb();
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var prescription = await db.Set<Prescription>()
                    .Include(p => p.PatientMedicines)
                        .ThenInclude(pm => pm.Medicine)
                    .FirstOrDefaultAsync(p => p.PatientProcedureId == patientProcedureId);

                if (prescription == null)
                    throw new Exception("Prescription not found.");

                if (prescription.Status.Equals("completed", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Prescription is already completed.");

                // Decrement stock
                foreach (var pm in prescription.PatientMedicines)
                {
                    var medicine = pm.Medicine
                        ?? throw new Exception($"Medicine not found for PatientMedicineId {pm.Id}");

                    if (medicine.Stock < pm.Quantity)
                        throw new Exception($"Not enough stock for medicine '{medicine.Name}'.");

                    medicine.Stock -= pm.Quantity;
                }

                // Update prescription status
                prescription.Status = "completed";

                await db.SaveChangesAsync();

                await transaction.CommitAsync();

                return prescription;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}