using aesth_clic.Tenant.Model;
using aesth_clic.Tenant.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Controller
{
    public class PatientMedicineController(PatientMedicineService patientMedicineService)
    {
        private readonly PatientMedicineService _service = patientMedicineService;

        // -------------------------
        // FETCH ALL by PrescriptionId
        // -------------------------
        public async Task<List<PatientMedicine>> GetAllByPrescriptionIdAsync(int prescriptionId)
        {
            if (prescriptionId <= 0)
                throw new ArgumentException("Invalid PrescriptionId.");

            return await _service.GetAllByPrescriptionIdAsync(prescriptionId);
        }

        // -------------------------
        // UPDATE quantity
        // -------------------------
        public async Task<bool> UpdateQuantityAsync(int id, int quantity)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid PatientMedicine id.");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            return await _service.UpdateAsync(id, quantity);
        }

        // -------------------------
        // DELETE a PatientMedicine
        // -------------------------
        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid PatientMedicine id.");

            return await _service.DeleteAsync(id);
        }
    }
}