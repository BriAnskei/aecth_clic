using aesth_clic.Session;
using aesth_clic.Tenant.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Services
{
    public sealed class PatientService : TenantServiceBase
    {
        // -------------------------
        // CREATE
        // -------------------------
        public async Task<Patient> CreateAsync(Patient patient)
        {
            using var db = CreateTenantDb();

            patient.Validate();
            patient.CreatedAt = DateTime.UtcNow;

            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            return patient;
        }

        // -------------------------
        // READ (ALL)
        // -------------------------
        public async Task<List<Patient>> GetAllAsync()
        {
            using var db = CreateTenantDb();

            return await db.Patients.ToListAsync();
        }

        // -------------------------
        // READ (BY ID)
        // -------------------------
        public async Task<Patient?> GetByIdAsync(int id)
        {
            using var db = CreateTenantDb();

            return await db.Patients
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // -------------------------
        // UPDATE
        // -------------------------
        public async Task<bool> UpdateAsync(Patient updatedPatient)
        {
            using var db = CreateTenantDb();

            var patient = await db.Patients.FindAsync(updatedPatient.Id);

            if (patient == null)
                return false;

            updatedPatient.Validate();

            patient.FullName = updatedPatient.FullName;
            patient.Gender = updatedPatient.Gender;
            patient.Age = updatedPatient.Age;
            patient.Email = updatedPatient.Email;
            patient.Address = updatedPatient.Address;
            patient.PhoneNumber = updatedPatient.PhoneNumber;

            await db.SaveChangesAsync();

            return true;
        }

        // -------------------------
        // DELETE
        // -------------------------
        public async Task<bool> DeleteAsync(int id)
        {
            using var db = CreateTenantDb();

            var patient = await db.Patients.FindAsync(id);

            if (patient == null)
                return false;

            db.Patients.Remove(patient);
            await db.SaveChangesAsync();

            return true;
        }
    }
}