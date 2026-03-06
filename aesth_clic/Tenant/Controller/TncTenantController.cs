using aesth_clic.Tenant.Model;
using aesth_clic.Tenant.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Controller
{
    internal sealed class TncTenantController(TncTenantService tncService)
    {
        private readonly TncTenantService _tncService =
            tncService ?? throw new ArgumentNullException(nameof(tncService));

        // ──────────────────────────────────────────────
        // GET ALL TENANT TNCs
        // ──────────────────────────────────────────────
        public Task<List<TncTenant>> GetAllTncsAsync()
            => _tncService.GetAllTncsAsync();

        // ──────────────────────────────────────────────
        // GET TNC BY ID
        // ──────────────────────────────────────────────
        public Task<TncTenant?> GetTncByIdAsync(int id)
            => _tncService.GetTncByIdAsync(id);

        // ──────────────────────────────────────────────
        // CREATE NEW TENANT TNC
        // ──────────────────────────────────────────────
        public Task<TncTenant> CreateTncAsync(TncTenant tnc)
            => _tncService.CreateTncAsync(tnc);

        // ──────────────────────────────────────────────
        // UPDATE EXISTING TENANT TNC
        // ──────────────────────────────────────────────
        public Task<bool> UpdateTncAsync(TncTenant tnc)
            => _tncService.UpdateTncAsync(tnc);

        // ──────────────────────────────────────────────
        // DELETE TENANT TNC BY ID
        // ──────────────────────────────────────────────
        public Task<bool> DeleteTncAsync(int id)
            => _tncService.DeleteTncAsync(id);

        // ──────────────────────────────────────────────
        // FETCH MASTER TNCs
        // ──────────────────────────────────────────────
        public Task<List<TncTenant>> FetchMasterTncsAsync()
            => _tncService.FetchMasterTncsAsync();
    }
}