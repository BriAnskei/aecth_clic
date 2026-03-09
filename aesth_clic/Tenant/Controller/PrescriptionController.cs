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
            try
            {
                return await _service.GetAllAsync();
            }
            catch (Exception ex)
            {
                // Debugging output
                Console.WriteLine($"Error in GetAllPrescriptionsAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                // Optionally rethrow so the caller still sees the error
                throw;
            }
        }
        public async Task<Prescription?> GetByProcedureIdAsync(int patientProcedureId)
        {
            return await _service.GetByProcedureIdAsync(patientProcedureId);
        }


        public async Task<Prescription> UpdateAsync(
 int patientProcedureId,
 List<PrescriptionMedicineDto> medicines)
        {
           return await _service.UpdateAsync(patientProcedureId, medicines);
        }

        public async Task<Prescription> MarkCompletedAsync(int patientProcedureId)
        {
            return await _service.MarkCompletedAsync(patientProcedureId);
        }

    }
}