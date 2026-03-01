using System;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace aesth_clic.Models.Users
{
    internal class User(int id, string fullName, string email, string username, string password, string? phoneNumber, string role, DateTime createdAt)
    {
        public int Id { get; set; } = id;
        public string? FullName { get; set; } = fullName;
        public string? Email { get; set; } = email;
        public string? Username { get; set; } = username;
        public string? Password { get; set; } = password;
        public string? PhoneNumber { get; set; } = phoneNumber;
        public string? Role { get; set; } = role;
        public DateTime CreatedAt { get; set; } = createdAt;

        // Validation function that throws immediately
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(FullName))
                throw new ValidationException("Full name is required.");

            if (string.IsNullOrWhiteSpace(Email))
                throw new ValidationException("Email is required.");
            else if (!IsValidEmail(Email))
                throw new ValidationException("Email format is invalid.");

            if (string.IsNullOrWhiteSpace(Username))
                throw new ValidationException("Username is required.");
            else if (Username.Length < 4)
                throw new ValidationException("Username must be at least 4 characters.");

            if (string.IsNullOrWhiteSpace(Password))
                throw new ValidationException("Password is required.");
            else if (Password.Length < 6)
                throw new ValidationException("Password must be at least 6 characters.");

            if (!string.IsNullOrWhiteSpace(PhoneNumber))
            {
                if (!Regex.IsMatch(PhoneNumber, @"^\+?[0-9]{7,15}$"))
                    throw new ValidationException("Phone number format is invalid.");
            }

            if (string.IsNullOrWhiteSpace(Role))
                throw new ValidationException("Role is required.");

        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}