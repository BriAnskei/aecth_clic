using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Models.Users
{
    internal class User
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }  
        public string? Role { get; set; }          
        public DateTime CreatedAt { get; set; }

        // Constructor
        public User(int id, string fullName, string username, string password, string? phoneNumber, string role, DateTime createdAt)
        {
            Id = id;
            FullName = fullName;
            Username = username;
            Password = password;
            PhoneNumber = phoneNumber;
            Role = role;
            CreatedAt = createdAt;
        }

        // Optional: parameterless constructor for ORM (like Entity Framework)
        public User() { }
    }
}
