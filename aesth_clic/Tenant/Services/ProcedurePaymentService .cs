using aesth_clic.Context;
using aesth_clic.Session;
using aesth_clic.Tenant.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Services
{
    public sealed class ProcedurePaymentService : TenantServiceBase
    {
        /// <summary>
        /// Create a ProcedurePayment inside an external transaction
        /// </summary>
        public async Task<ProcedurePayment> CreateAsync(
            TenantDbContext db,
            int patientProcedureId,
            int serviceMenuId,
            string status = "pending")
        {
            if (patientProcedureId <= 0)
                throw new ArgumentException("PatientProcedureId is required.");

            if (serviceMenuId <= 0)
                throw new ArgumentException("ServiceMenuId is required.");

            var payment = new ProcedurePayment
            {
                PatientProcedureId = patientProcedureId,
                ServiceMenuId = serviceMenuId,
                Status = status
            };

            db.Set<ProcedurePayment>().Add(payment);
            await db.SaveChangesAsync();

            return payment;
        }

        /// <summary>
        /// Mark a payment as completed
        /// </summary>
        public async Task<ProcedurePayment> MarkCompletedAsync(int paymentId)
        {
            using var db = CreateTenantDb();
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var payment = await db.Set<ProcedurePayment>()
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                    throw new Exception("ProcedurePayment not found.");

                if (payment.Status.Equals("completed", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("ProcedurePayment is already completed.");

                payment.Status = "completed";

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return payment;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Get all ProcedurePayments with all related data
        /// </summary>
        public async Task<List<ProcedurePayment>> GetAllAsync()
        {
            using var db = CreateTenantDb();

            return await db.Set<ProcedurePayment>()
                .Include(pp => pp.PatientProcedure!)  // null-forgiving
                    .ThenInclude(p => p.User)          // assigned doctor
                .Include(pp => pp.PatientProcedure!)
                    .ThenInclude(p => p.Patient)       // patient
                .Include(pp => pp.PatientProcedure!)
                    .ThenInclude(p => p.ServiceMenu)   // procedure/service
                .Include(pp => pp.PatientProcedure!)
                    .ThenInclude(p => p.Prescription!) // null-forgiving
                        .ThenInclude(pr => pr.PatientMedicines)
                            .ThenInclude(pm => pm.Medicine)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}