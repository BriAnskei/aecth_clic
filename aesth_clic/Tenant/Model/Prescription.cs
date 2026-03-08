using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace aesth_clic.Tenant.Model
{
    public class Prescription
    {
        public int Id { get; set; }

        // Foreign key to PatientProcedure
        public int PatientProcedureId { get; set; }

        public PatientProcedure? PatientProcedure { get; set; }




        // e.g. Pending, Released, Completed
      
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public List<PatientMedicine> PatientMedicines { get; set; } = new List<PatientMedicine>();

        public void ValidateForInsert()
        {
            if (PatientProcedureId <= 0)
                throw new ValidationException("PatientProcedureId is required.");

            if (string.IsNullOrWhiteSpace(Status))
                throw new ValidationException("Status is required.");
        }
    }
}