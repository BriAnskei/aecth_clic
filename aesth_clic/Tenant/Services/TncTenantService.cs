using aesth_clic.Master.Services;
using aesth_clic.Session;
using aesth_clic.Tenant.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Services
{
    internal sealed class TncTenantService(TncMasterService masterTncService) : TenantServiceBase
    {
        private readonly TncMasterService _masterTncService =
            masterTncService ?? throw new ArgumentNullException(nameof(masterTncService));

        // ──────────────────────────────────────────────
        // GET ALL TENANT TNCs
        // ──────────────────────────────────────────────
        public async Task<List<TncTenant>> GetAllTncsAsync()
        {
            try
            {
                using var db = CreateTenantDb();
                return await db.TncTenants
                    .OrderBy(t => t.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAllTncsAsync failed: {ex}");
                return new List<TncTenant>();
            }
        }

        // ──────────────────────────────────────────────
        // GET TNC BY ID
        // ──────────────────────────────────────────────
        public async Task<TncTenant?> GetTncByIdAsync(int id)
        {
            try
            {
                using var db = CreateTenantDb();
                return await db.TncTenants
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetTncByIdAsync failed for Id {id}: {ex}");
                return null;
            }
        }

        // ──────────────────────────────────────────────
        // CREATE NEW TENANT TNC
        // ──────────────────────────────────────────────
        public async Task<TncTenant> CreateTncAsync(TncTenant tnc)
        {
            try
            {
                using var db = CreateTenantDb();
                db.TncTenants.Add(tnc);
                await db.SaveChangesAsync();
                return tnc;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CreateTncAsync failed: {ex}");
                throw;
            }
        }

        // ──────────────────────────────────────────────
        // UPDATE EXISTING TENANT TNC
        // ──────────────────────────────────────────────
        public async Task<bool> UpdateTncAsync(TncTenant tnc)
        {
            try
            {
                using var db = CreateTenantDb();
                var existingTnc = await db.TncTenants
                    .FirstOrDefaultAsync(t => t.Id == tnc.Id);

                if (existingTnc is null)
                    return false;

                existingTnc.Title = tnc.Title;
                existingTnc.Description = tnc.Description;

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateTncAsync failed for Id {tnc.Id}: {ex}");
                return false;
            }
        }

        // ──────────────────────────────────────────────
        // DELETE TENANT TNC BY ID
        // ──────────────────────────────────────────────
        public async Task<bool> DeleteTncAsync(int id)
        {
            try
            {
                using var db = CreateTenantDb();
                var tnc = await db.TncTenants
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tnc is null)
                    return false;

                db.TncTenants.Remove(tnc);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DeleteTncAsync failed for Id {id}: {ex}");
                return false;
            }
        }

        // ──────────────────────────────────────────────
        // FETCH TNCs FROM MASTER AND MAP TO TENANT
        // ──────────────────────────────────────────────
        public async Task<List<TncTenant>> FetchMasterTncsAsync()
        {
            try
            {
                var masterTncs = await _masterTncService.GetAllTncsAsync();

                return masterTncs.Select(m => new TncTenant
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] FetchMasterTncsAsync failed: {ex}");
                return new List<TncTenant>();
            }
        }
    }
}