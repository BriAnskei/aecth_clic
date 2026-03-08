using aesth_clic.Session;
using aesth_clic.Tenant.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Services
{
    public sealed class MedicineService : TenantServiceBase
    {
        // -------------------------
        // CREATE
        // -------------------------
        public async Task<Medicine> CreateAsync(
            string name,
            int stock,
            string unit,
            DateTime expiryDate)
        {
            using var db = CreateTenantDb();

            var medicine = new Medicine
            {
                Name = name,
                Stock = stock,
                Unit = unit,
                ExpiryDate = expiryDate,
                LastStockIn = DateTime.UtcNow
            };

            db.Set<Medicine>().Add(medicine);
            await db.SaveChangesAsync();

            return medicine;
        }

        // -------------------------
        // READ ALL
        // -------------------------
        public async Task<List<Medicine>> GetAllAsync()
        {
            using var db = CreateTenantDb();

            return await db.Set<Medicine>()
                .AsNoTracking()
                .ToListAsync();
        }

        // -------------------------
        // READ BY ID
        // -------------------------
        public async Task<Medicine?> GetByIdAsync(int id)
        {
            using var db = CreateTenantDb();

            return await db.Set<Medicine>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // -------------------------
        // UPDATE
        // -------------------------
        public async Task<bool> UpdateAsync(
            int id,
            string name,
            int stock,
            string unit,
            DateTime expiryDate)
        {
            using var db = CreateTenantDb();

            var medicine = await db.Set<Medicine>().FindAsync(id);

            if (medicine == null)
                return false;

            medicine.Name = name;
            medicine.Stock = stock;
            medicine.Unit = unit;
            medicine.ExpiryDate = expiryDate;

            await db.SaveChangesAsync();
            return true;
        }


        // -------------------------
        // RESTOCK
        // 
        public async Task<bool> RestockAsync(int id, int amount)
        {
            using var db = CreateTenantDb();

            var medicine = await db.Set<Medicine>().FindAsync(id);

            if (medicine == null)
                return false;

            medicine.Stock += amount;
            medicine.LastStockIn = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return true;
        }

        // -------------------------
        // DELETE
        // -------------------------
        public async Task<bool> DeleteAsync(int id)
        {
            using var db = CreateTenantDb();

            var medicine = await db.Set<Medicine>().FindAsync(id);

            if (medicine == null)
                return false;

            db.Set<Medicine>().Remove(medicine);
            await db.SaveChangesAsync();

            return true;
        }
    }
}