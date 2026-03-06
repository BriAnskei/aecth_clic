using aesth_clic.Context;
using aesth_clic.Master.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aesth_clic.Master.Services
{
    internal sealed class TncMasterService(
        MasterDbContext masterDb)
    {
        private readonly MasterDbContext _masterDb =
            masterDb ?? throw new ArgumentNullException(nameof(masterDb));

        // ──────────────────────────────────────────────
        // GET ALL TERMS AND CONDITIONS
        // ──────────────────────────────────────────────
        public async Task<List<TncMaster>> GetAllTncsAsync()
        {
            try
            {
                return await _masterDb.TncMaster
                    .OrderBy(t => t.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAllTncsAsync failed: {ex}");
                return new List<TncMaster>();
            }
        }

        // ──────────────────────────────────────────────
        // GET TNC BY ID
        // ──────────────────────────────────────────────
        public async Task<TncMaster?> GetTncByIdAsync(int id)
        {
            try
            {
                return await _masterDb.TncMaster
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetTncByIdAsync failed for Id {id}: {ex}");
                return null;
            }
        }

        // ──────────────────────────────────────────────
        // CREATE NEW TNC
        // ──────────────────────────────────────────────
        public async Task<TncMaster> CreateTncAsync(TncMaster tnc)
        {
            try
            {
                _masterDb.TncMaster.Add(tnc);
                await _masterDb.SaveChangesAsync();
                return tnc;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CreateTncAsync failed: {ex}");
                throw;
            }
        }

        // ──────────────────────────────────────────────
        // UPDATE EXISTING TNC
        // ──────────────────────────────────────────────
        public async Task<bool> UpdateTncAsync(TncMaster tnc)
        {
            try
            {
                var existingTnc = await _masterDb.TncMaster
                    .FirstOrDefaultAsync(t => t.Id == tnc.Id);

                if (existingTnc is null)
                    return false;

                existingTnc.Description = tnc.Description;

                await _masterDb.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateTncAsync failed for Id {tnc.Id}: {ex}");
                return false;
            }
        }

        // ──────────────────────────────────────────────
        // DELETE TNC BY ID
        // ──────────────────────────────────────────────
        public async Task<bool> DeleteTncAsync(int id)
        {
            try
            {
                var tnc = await _masterDb.TncMaster
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tnc is null)
                    return false;

                _masterDb.TncMaster.Remove(tnc);
                await _masterDb.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DeleteTncAsync failed for Id {id}: {ex}");
                return false;
            }
        }
    }
}