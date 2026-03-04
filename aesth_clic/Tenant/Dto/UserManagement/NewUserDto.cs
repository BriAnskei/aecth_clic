using System;

namespace aesth_clic.Tenant.Dto.UserManagement
{
    public class NewUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Validation method that throws an exception if any field is empty
        public void ValidateRequiredFields()
        {
            if (string.IsNullOrWhiteSpace(FullName))
                throw new ArgumentException("FullName is required and cannot be empty.");

            if (string.IsNullOrWhiteSpace(Email))
                throw new ArgumentException("Email is required and cannot be empty.");

            if (string.IsNullOrWhiteSpace(PhoneNumber))
                throw new ArgumentException("PhoneNumber is required and cannot be empty.");

            if (string.IsNullOrWhiteSpace(Role))
                throw new ArgumentException("Role is required and cannot be empty.");

            if (string.IsNullOrWhiteSpace(UserName))
                throw new ArgumentException("UserName is required and cannot be empty.");

            if (string.IsNullOrWhiteSpace(Password))
                throw new ArgumentException("Password is required and cannot be empty.");
        }
    }
}