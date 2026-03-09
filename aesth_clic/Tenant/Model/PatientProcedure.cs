using System;
using System.ComponentModel.DataAnnotations;

namespace aesth_clic.Tenant.Model
{
    public class PatientProcedure
    {
        public int Id { get; set; } = 0;

        public int PatientId { get; set; }

        public int ProcedureId { get; set; }

        public int? AssignedDoctorId { get; set; }

        public DateTime? AppointmentDate { get; set; }

        public DateTime? ProcedureDate { get; set; }

        public string Status { get; set; } = string.Empty; // scheduled, completed, cancelled

        public DateTime CreatedAt { get; set; }


        public User? User { get; set; } // assigned doctor
        public ServiceMenu? ServiceMenu { get; set; } // procedure details

        public Patient Patient { get; set; } = null!; // patient details

        public Prescription? Prescription { get; set; } // associated prescription


        public void ValidateForInsert()
        {
            if (PatientId <= 0)
                throw new ValidationException("PatientId is required.");

            if (ProcedureId <= 0)
                throw new ValidationException("ProcedureId is required.");

            if (!Status.Equals("pending", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("New patient procedure must start with 'pending' status.");
        }

        public void ValidateForScheduling()
        {
            if (AssignedDoctorId == null || AssignedDoctorId <= 0)
                throw new ValidationException("AssignedDoctorId is required.");

            if (AppointmentDate == null || AppointmentDate == default)
                throw new ValidationException("AppointmentDate is required.");
        }

        public void ValidateForProcedureDate()
        {
            if (ProcedureDate == null)
                throw new ValidationException("ProcedureDate is required.");

            if (AppointmentDate == null)
                throw new ValidationException("AppointmentDate must exist before procedure.");

            if (ProcedureDate < AppointmentDate)
                throw new ValidationException("ProcedureDate cannot be earlier than AppointmentDate.");
        }
    }
}