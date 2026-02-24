using System;
using System.Threading.Tasks;
using aesth_clic.Services.SuperAdminServices;

namespace aesth_clic.Controller
{
    internal class CompanyController
    {
        private readonly CompanyService _companyService;

        public CompanyController(CompanyService companyService)
        {
            _companyService =
                companyService ?? throw new ArgumentNullException(nameof(companyService));
        }

        // Update only company status
        public async Task<bool> UpdateCompanyStatusAsync(int companyId, string newStatus)
        {
            return await _companyService.UpdateCompanyStatusAsync(companyId, newStatus);
        }
    }
}
