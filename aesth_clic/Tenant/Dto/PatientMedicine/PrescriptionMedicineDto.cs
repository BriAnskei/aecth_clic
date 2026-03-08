using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.DTO
{
    public class PrescriptionMedicineDto
    {
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreatePrescriptionRequest
    {
        public int PatientProcedureId { get; set; }


        public List<PrescriptionMedicineDto> Medicines { get; set; } = new();
    }
}
