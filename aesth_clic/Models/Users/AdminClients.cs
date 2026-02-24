using System;
using aesth_clic.Models.Companies;

namespace aesth_clic.Models.Users
{
    internal class AdminClients
    {
        public User? User { get; set; }
        public Company? Company { get; set; }

        public AdminClients(User? user, Company? companies)
        {
            User = user;
            Company = companies;
        }

        public void ValidateAdminClient()
        {
            if (User is null)
                throw new ArgumentException("User cannot be null.");

            if (Company is null)
                throw new ArgumentException("Company cannot be null.");
        }
    }
}
