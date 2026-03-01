using aesth_clic.Master.Dto;
using aesth_clic.Master.Dto.Company;
using aesth_clic.Master.Services;
using aesth_clic.Services.SuperAdminServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Master.Controller
{
    internal class AdminUserController(AdminClientService adminClientService)
    {

        private readonly AdminClientService _adminCLientService =
       adminClientService ?? throw new ArgumentNullException(nameof(adminClientService));

        public async Task UpdateClientAsync(UpdateAdminUserDto updateAdminUserDto)
        {
         await  adminClientService.UpdateAdminUserAsync(updateAdminUserDto);
        }

        public async Task<List<AdminClinicDetailsDto>> GetAllAdminClinicsAsync()
        {
            var res =  await _adminCLientService.GetAllAdminClinicsAsync();

          return res;
        }
    }
}
