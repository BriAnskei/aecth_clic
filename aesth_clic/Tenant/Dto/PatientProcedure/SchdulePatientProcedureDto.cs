using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Dto.PatientProcedure
{
    public class SchedulePatientProcedureDto
    {
        public int PatientProcedureId { get; set; }

        public int AssignedDoctorId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public void ValidateRequiredFields()
        {
            if (PatientProcedureId <= 0)
                throw new Exception("PatientProcedureId is required.");

            if (AssignedDoctorId <= 0)
                throw new Exception("AssignedDoctorId is required.");

            if (AppointmentDate == default)
                throw new Exception("AppointmentDate is required.");
        }
    }
}
