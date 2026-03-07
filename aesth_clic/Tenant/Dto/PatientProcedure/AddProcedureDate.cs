using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Dto.PatientProcedure
{
    public class AddProcedureDateDto
    {
        public int PatientProcedureId { get; set; }

        public DateTime ProcedureDate { get; set; }

        public void ValidateRequiredFields()
        {
            if (PatientProcedureId <= 0)
                throw new Exception("PatientProcedureId is required.");

            if (ProcedureDate == default)
                throw new Exception("ProcedureDate is required.");
        }
    }
}
