using aesth_clic.Master.Model;
using aesth_clic.Tenant.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace aesth_clic.Master.Dto.Company
{
    public class NewClientUserDto(User adminUser, Client client)
    {
        public User AdminUser { get; set; } = adminUser ?? throw new ValidationException("User is required.");
        public Client Client { get; set; } = client ?? throw new ValidationException("Client is required.");

        // Validate function that throws immediately
        public void Validate()
        {
            // Validate User (throws on first error)
            AdminUser.ValidateForInsert();

            // Validate Client for insert (throws on first error)
            Client.ValidateForInsert();
        }
    }
}