using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace aesth_clic.Tenant.Dto.UserManagement
{
    public class UpdateUserDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        // Optional on update
        public string? Password { get; set; }

        public string Role { get; set; } = string.Empty;

        public void ValidateRequiredFields()
        {
            if (Id <= 0)
                throw new ValidationException("Valid user Id is required.");

            if (string.IsNullOrWhiteSpace(FullName))
                throw new ValidationException("Full name is required.");

            if (string.IsNullOrWhiteSpace(UserName))
                throw new ValidationException("Username is required.");

            if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 6)
                throw new ValidationException("Password must be at least 6 characters long.");

            if (string.IsNullOrWhiteSpace(Role))
                throw new ValidationException("Role is required.");

            var allowedRoles = new[] {  "doctor", "reciptionist", "pharmacist" };

            if (!allowedRoles.Any(r =>
                string.Equals(r, Role.ToLower(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new ValidationException("Role must be Admin, Doctor, Reciptionist, or Pharmacist.");
            }
        }
    }
}