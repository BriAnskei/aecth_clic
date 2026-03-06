using aesth_clic.Master.Model;
using aesth_clic.Master.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Master.Controller
{
    internal sealed class TncMasterController(TncMasterService tncService)
    {
        private readonly TncMasterService _tncService =
            tncService ?? throw new ArgumentNullException(nameof(tncService));

        // ──────────────────────────────────────────────
        // GET ALL TNCs
        // ──────────────────────────────────────────────
        public Task<List<TncMaster>> GetAllTncsAsync()
            => _tncService.GetAllTncsAsync();

        // ──────────────────────────────────────────────
        // GET TNC BY ID
        // ──────────────────────────────────────────────
        public Task<TncMaster?> GetTncByIdAsync(int id)
            => _tncService.GetTncByIdAsync(id);

        // ──────────────────────────────────────────────
        // CREATE NEW TNC
        // ──────────────────────────────────────────────
        public Task<TncMaster> CreateTncAsync(TncMaster tnc)
            => _tncService.CreateTncAsync(tnc);

        // ──────────────────────────────────────────────
        // UPDATE EXISTING TNC
        // ──────────────────────────────────────────────
        public Task<bool> UpdateTncAsync(TncMaster tnc)
            => _tncService.UpdateTncAsync(tnc);

        // ──────────────────────────────────────────────
        // DELETE TNC BY ID
        // ──────────────────────────────────────────────
        public Task<bool> DeleteTncAsync(int id)
            => _tncService.DeleteTncAsync(id);
    }
}