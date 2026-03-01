using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Master.Dto
{
    public class AdminClinicDetailsDto
    {
        public int ClientId { get; set; }         
        public string ClinicCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int UserId { get; set; }        

        public string ClinicName { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
