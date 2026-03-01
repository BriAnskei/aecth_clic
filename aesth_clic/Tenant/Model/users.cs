using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace aesth_clic.Tenant.Model
{
    public class User
    {
        public int Id { get; set; } = 0;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;


        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty; // admin, Doctor, Reciptionist, Pharmacist

        public DateTime CreatedAt { get; set; }

        // Validate for insertion/creation: all required fields
        public void ValidateForInsert()
        {
            if (string.IsNullOrWhiteSpace(FullName))
                throw new ValidationException("Full name is required.");

            if (string.IsNullOrWhiteSpace(PhoneNumber))
                throw new ValidationException("PhoneNumber is required.");

            if (string.IsNullOrWhiteSpace(Email))
                throw new ValidationException("Email is required.");

            if (string.IsNullOrWhiteSpace(Username))
                throw new ValidationException("Username is required.");

            if (string.IsNullOrWhiteSpace(Password))
                throw new ValidationException("Password is required.");



            if (Password.Length < 6)
                throw new ValidationException("Password must be at least 6 characters long.");

            if (string.IsNullOrWhiteSpace(Role))
                throw new ValidationException("Role is required.");

            var allowedRoles = new[] { "Admin", "Doctor", "Reciptionist", "Pharmacist" };
            if (!allowedRoles.Any(r => string.Equals(r, Role, StringComparison.OrdinalIgnoreCase)))
                throw new ValidationException("Role must be Admin, Doctor, Reciptionist, or Pharmacist.");
        }

        // Validate for update: might have different rules (e.g., password optional)
        public void ValidateForUpdate()
        {
            if (string.IsNullOrWhiteSpace(FullName))
                throw new ValidationException("Full name is required.");

            if (string.IsNullOrWhiteSpace(Username))
                throw new ValidationException("Username is required.");

            // Password might be optional on update, only validate if provided
            if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 6)
                throw new ValidationException("Password must be at least 6 characters long.");

            if (string.IsNullOrWhiteSpace(Role))
                throw new ValidationException("Role is required.");

            var allowedRoles = new[] { "admin", "doctor", "reciptionist", "pharmacist" };
            if (!allowedRoles.Contains(Role.ToLower()))
                throw new ValidationException("Role must be Admin, Doctor, Reciptionist, or Pharmacist.");
        }

     
    }
}