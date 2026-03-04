using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Model
{
    public class AccountStatus
    {
        public int Id { get; set; } = 0;

        public int AccountId { get; set; } = 0;

        public string Status { get; set; } = "acitve"; // default active (active, deactivated)

        public User? User { get; set; }
    }
}
