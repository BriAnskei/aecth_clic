using System;
using System.ComponentModel.DataAnnotations;

namespace aesth_clic.Tenant.Model
{
    public class Medicine
    {
        public int Id { get; set; } = 0;

     
        public string Name { get; set; } = string.Empty;

       
        public int Stock { get; set; } = 0;

        public DateTime LastStockIn { get; set; } = DateTime.UtcNow;

    
        public string Unit { get; set; } = string.Empty; // e.g., mg, ml, tablet

      
        public DateTime ExpiryDate { get; set; }

        public void ValidateForInsert()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ValidationException("Medicine Name is required.");

            if (Stock < 0)
                throw new ValidationException("Stock cannot be negative.");

            if (string.IsNullOrWhiteSpace(Unit))
                throw new ValidationException("Unit is required.");

            if (ExpiryDate <= DateTime.UtcNow)
                throw new ValidationException("ExpiryDate must be a future date.");
        }

        public void ValidateStockUpdate(int addedStock)
        {
            if (addedStock <= 0)
                throw new ValidationException("Added stock must be greater than zero.");
        }
    }
}