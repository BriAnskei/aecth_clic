using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Models.Users
{
    internal class Company
    {

        public int id { get; set; }
        public int user_id { get; set; }
        public string name { get; set; }
        public string status { get; set; }


        public Company(int id, int user_id, string name, string status)
        {
            this.id = id;
            this.user_id = user_id;
            this.name = name;
            this.status = status;
        }


        public Company() {
            this.name = string.Empty;
            this.status = string.Empty;
        }
    }



}
