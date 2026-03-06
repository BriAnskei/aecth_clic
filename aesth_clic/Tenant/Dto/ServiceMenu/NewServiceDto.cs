using System;
using System.Text.RegularExpressions;

namespace aesth_clic.Tenant.Dto.ServiceMenu
{
    public class NewServiceDto
    {
        public string Name { get; set; } = string.Empty;
        public double Prive { get; set; } = 0.0;
        public int addedById { get; set; } = 0;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Service name is required");

            if (Name.Length < 3)
                throw new ArgumentException("Service name must be at least 3 characters long");

            if (Name.Length > 100)
                throw new ArgumentException("Service name cannot exceed 100 characters");

            if (!Regex.IsMatch(Name, @"^[a-zA-Z0-9\s\-_&.]+$"))
                throw new ArgumentException("Service name contains invalid characters");

            if (Prive <= 0)
                throw new ArgumentException("Price must be greater than 0");

            if (Prive > 999999.99)
                throw new ArgumentException("Price exceeds maximum allowed value");

            if (Math.Round(Prive, 2) != Prive)
                throw new ArgumentException("Price cannot have more than 2 decimal places");

            if (addedById <= 0)
                throw new ArgumentException("Valid user ID is required");
        }
    }
}