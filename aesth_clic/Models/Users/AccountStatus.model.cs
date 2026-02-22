using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Models.Users
{
    internal class AccountStatus(int id, int userId, int companyId, string status)
    {

        public int Id { get; set; } = id;
        public int UserId { get; set; } = userId;
        public int CompanyId { get; set; } = companyId;
        public string Status { get; set; } = status;



    }


    
}
