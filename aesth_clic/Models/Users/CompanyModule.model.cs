using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Models.Users
{
    internal class CompanyModule
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public String module_type { get; set; } = string.Empty;


        public CompanyModule(int id, int company_id, string module_type)
        {
            this.id = id;
            this.company_id = company_id;
            this.module_type = module_type;
        }


        public CompanyModule() { }
    }
}
