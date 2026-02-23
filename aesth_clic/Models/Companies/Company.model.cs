using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Models.Companies
{
    internal class Company
    {

        public int id { get; set; }
        public int owner_id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string module_tier { get; set; }

        public Company(int id, int owner_id, string name, string status, string module_tier)
        {
            this.id = id;
            this.owner_id = owner_id;
            this.name = name;
            this.status = status;
            this.module_tier = module_tier;
        }


        public Company() {
            name = string.Empty;
            status = string.Empty;
            module_tier = string.Empty;
        }
    }



}
