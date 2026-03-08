using System.ComponentModel.DataAnnotations;

namespace aesth_clic.Tenant.Model
{
    public class PatientMedicine
    {
        public int Id { get; set; }

        // FK to Prescription
        public int PrescriptionId { get; set; }

        public Prescription? Prescription { get; set; }

        // FK to Medicine
        public int MedicineId { get; set; }

        public Medicine? Medicine { get; set; }

        // Quantity prescribed
        public int Quantity { get; set; }

        public void ValidateForInsert()
        {
            if (MedicineId <= 0)
                throw new ValidationException("MedicineId is required.");

            if (PrescriptionId <= 0)
                throw new ValidationException("PrescriptionId is required.");

            if (Quantity <= 0)
                throw new ValidationException("Quantity must be greater than zero.");
        }
    }
}