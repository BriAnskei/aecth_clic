using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aesth_clic.Models.Companies;
using aesth_clic.Models.Users;

namespace aesth_clic.Dto.SuperAdmin
{
    internal class NewAdminClient
    {
        public  User? User { get; private set; }
        public  Company? Company { get; private set; }


        public NewAdminClient(User user, Company company)
        {
            User = user;
            Company = company;
        }
    }
}
