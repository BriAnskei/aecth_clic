using aesth_clic.Tenant.Model;
using aesth_clic.Tenant.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Controller
{
    public class ProcedurePaymentController(ProcedurePaymentService procedurePaymentService)
    {
        private readonly ProcedurePaymentService _procedurePaymentService = procedurePaymentService;

   
        // -------------------------
        // MARK AS COMPLETED
        // -------------------------
        public async Task<ProcedurePayment> MarkPaymentCompletedAsync(int paymentId)
        {
            if (paymentId <= 0)
                throw new ArgumentException("Invalid payment id.");

            return await _procedurePaymentService.MarkCompletedAsync(paymentId);
        }

        // -------------------------
        // READ ALL
        // -------------------------
        public async Task<List<ProcedurePayment>> GetAllPaymentsAsync()
        {
            return await _procedurePaymentService.GetAllAsync();
        }

        // -------------------------
        // READ BY ID
        // -------------------------
        public async Task<ProcedurePayment?> GetPaymentByIdAsync(int paymentId)
        {
            if (paymentId <= 0)
                throw new ArgumentException("Invalid payment id.");

            var allPayments = await _procedurePaymentService.GetAllAsync();
            return allPayments.Find(p => p.Id == paymentId);
        }
    }
}