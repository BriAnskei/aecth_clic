using aesth_clic.Session;
using aesth_clic.Tenant.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Services
{
    public sealed class MenuService : TenantServiceBase
    {
        // -------------------------
        // Helpers
        // -------------------------

        private void EnsureDoctor()
        {
            var user = AppSession.Instance.CurrentUser
                       ?? throw new Exception("No active user session.");

            if (user.Role.ToLower() != "doctor")
                throw new UnauthorizedAccessException("Only doctors can modify the service menu.");
        }

        private int GetCurrentUserId()
        {
            return AppSession.Instance.CurrentUser?.Id
                   ?? throw new Exception("No active user session.");
        }

        // -------------------------
        // CREATE
        // -------------------------
        public async Task<ServiceMenu> CreateAsync(string name, double price)
        {
            EnsureDoctor();

            using var db = CreateTenantDb();

            var service = new ServiceMenu
            {
                Name = name,
                Price = price,
                AddedBy = GetCurrentUserId()
            };

            db.ServiceMenu.Add(service);
            await db.SaveChangesAsync();

            return service;
        }

        // -------------------------
        // READ (All)
        // -------------------------
        public async Task<List<ServiceMenu>> GetAllAsync()
        {
            using var db = CreateTenantDb();

            return await db.ServiceMenu
                .Include(s => s.User)
                .ToListAsync();
        }

        // -------------------------
        // READ (By Id)
        // -------------------------
        public async Task<ServiceMenu?> GetByIdAsync(int id)
        {
            using var db = CreateTenantDb();

            return await db.ServiceMenu
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // -------------------------
        // UPDATE
        // -------------------------
        public async Task<bool> UpdateAsync(int id, string name, double price)
        {
            EnsureDoctor();

            using var db = CreateTenantDb();

            var service = await db.ServiceMenu.FindAsync(id);
            if (service == null)
                return false;

            // Optional: ensure doctor only edits his own services
            if (service.AddedBy != GetCurrentUserId())
                throw new UnauthorizedAccessException("You can only edit your own services.");

            service.Name = name;
            service.Price = price;

            await db.SaveChangesAsync();
            return true;
        }

        // -------------------------
        // DELETE
        // -------------------------
        public async Task<bool> DeleteAsync(int id)
        {
            EnsureDoctor();

            using var db = CreateTenantDb();

            var service = await db.ServiceMenu.FindAsync(id);
            if (service == null)
                return false;

            // Optional: ensure doctor only deletes his own services
            if (service.AddedBy != GetCurrentUserId())
                throw new UnauthorizedAccessException("You can only delete your own services.");

            db.ServiceMenu.Remove(service);
            await db.SaveChangesAsync();

            return true;
        }
    }
}