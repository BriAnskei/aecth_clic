using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aesth_clic.Models.Companies;
using aesth_clic.Models;

namespace aesth_clic.Models.Users
{
    internal class AdminClients
    {
        public User? User { get; set; }
        public Company? Company { get; set; }

            public AdminClients(User? user, Company? companies) { 
            
            User = user; Company = companies;
        }
    }
}
