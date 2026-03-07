using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Dto.PatientProcedure
{
    public class NewPatientProcedureDto
    {
        public int PatientId { get; set; }
        public int ProcedureId { get; set; }

        public void ValidateRequiredFields()
        {
            if (PatientId <= 0)
                throw new Exception("PatientId is required.");

            if (ProcedureId <= 0)
                throw new Exception("ProcedureId is required.");
        }
    }
}
