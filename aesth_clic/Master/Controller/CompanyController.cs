using System;
using System.Threading.Tasks;
using aesth_clic.Master.Dto.Company;
using aesth_clic.Master.Services;

namespace aesth_clic.Master.Controller
{
    internal class CompanyController(CompanyService companyService)
    {
        private readonly CompanyService _companyService =
            companyService ?? throw new ArgumentNullException(nameof(companyService));

        // ==============================
        // CREATE CLINIC (Client + Tenant DB + Admin User)
        // ==============================
        public async Task CreateClinicAsync(NewClientUserDto newClientUserDto)
        {
            if (newClientUserDto == null)
                throw new ArgumentNullException(nameof(newClientUserDto));

            newClientUserDto.Validate();

            await _companyService.CreateClinicAsync(newClientUserDto);
        }



        public async Task UpdateClientStatusAsync(string clinicCode, string newStatus)
        {
            await _companyService.UpdateClientStatusAsync(clinicCode, newStatus); 
        }

        public async Task DeleteClientAsync(string clinicCode)
        {
            await _companyService.DeleteClientAsync(clinicCode);    

        }


    }
}