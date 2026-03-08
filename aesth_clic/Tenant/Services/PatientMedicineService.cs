using aesth_clic.Session;
using aesth_clic.Tenant.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace aesth_clic.Tenant.Services
{
    public sealed class PatientMedicineService : TenantServiceBase
    {
        // -------------------------
        // FETCH ALL BY PrescriptionId
        // -------------------------
        public async Task<List<PatientMedicine>> GetAllByPrescriptionIdAsync(int prescriptionId)
        {
            if (prescriptionId <= 0)
                throw new ArgumentException("Invalid PrescriptionId.");

            using var db = CreateTenantDb();

            return await db.Set<PatientMedicine>()
                .Include(pm => pm.Medicine)
                .Where(pm => pm.PrescriptionId == prescriptionId)
                .AsNoTracking()
                .ToListAsync();
        }

        // -------------------------
        // UPDATE quantity of a PatientMedicine
        // -------------------------
        public async Task<bool> UpdateAsync(int id, int quantity)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid PatientMedicine id.");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            using var db = CreateTenantDb();

            var patientMedicine = await db.Set<PatientMedicine>().FindAsync(id);

            if (patientMedicine == null)
                return false;

            patientMedicine.Quantity = quantity;

            await db.SaveChangesAsync();
            return true;
        }

        // -------------------------
        // DELETE a PatientMedicine
        // -------------------------
        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid PatientMedicine id.");

            using var db = CreateTenantDb();

            var patientMedicine = await db.Set<PatientMedicine>().FindAsync(id);

            if (patientMedicine == null)
                return false;

            db.Set<PatientMedicine>().Remove(patientMedicine);
            await db.SaveChangesAsync();

            return true;
        }
    }
}