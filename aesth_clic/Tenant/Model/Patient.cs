using System;
using System.Text.RegularExpressions;

namespace aesth_clic.Tenant.Model
{
    public class Patient
    {
        public int Id { get; set; } = 0;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; } = 0;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(FullName))
                throw new ArgumentException("Full name is required.");

            if (string.IsNullOrWhiteSpace(Gender))
                throw new ArgumentException("Gender is required.");

            if (Age <= 0 || Age > 120)
                throw new ArgumentException("Age must be between 1 and 120.");

            if (string.IsNullOrWhiteSpace(Email))
                throw new ArgumentException("Email is required.");

            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Invalid email format.");

            if (string.IsNullOrWhiteSpace(PhoneNumber))
                throw new ArgumentException("Phone number is required.");

            if (string.IsNullOrWhiteSpace(Address))
                throw new ArgumentException("Address is required.");
        }
    }
}