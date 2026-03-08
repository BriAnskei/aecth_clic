using System;
using System.ComponentModel.DataAnnotations;

namespace aesth_clic.Tenant.Model
{
    public class ProcedurePayement
    {
        public int Id { get; set; }

        // FK to PatientProcedure
        public int PatientProcedureId { get; set; }

        public PatientProcedure? PatientProcedure { get; set; }

        // FK to Procedure -> ServiceMenuId
        public int ServiceMenuId { get; set; }

        public ServiceMenu? ServiceMenu { get; set; }

        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Validates the entity before insert
        /// </summary>
        public void ValidateForInsert()
        {
            if (PatientProcedureId <= 0)
                throw new ValidationException("PatientProcedureId is required.");

            if (ServiceMenuId <= 0)
                throw new ValidationException("ServiceMenuId is required.");

            if (string.IsNullOrWhiteSpace(Status))
                throw new ValidationException("Status is required.");
        }
    }
}