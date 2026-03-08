using aesth_clic.Tenant.DTO;
using aesth_clic.Tenant.Model;
using aesth_clic.Tenant.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Controller
{
    public class PrescriptionController(PrescriptionService prescriptionService)
    {
        private readonly PrescriptionService _service = prescriptionService;

      

        // -------------------------
        // GET ALL
        // -------------------------
        public async Task<List<Prescription>> GetAllPrescriptionsAsync()
        {
            return await _service.GetAllAsync();
        }

    }
}