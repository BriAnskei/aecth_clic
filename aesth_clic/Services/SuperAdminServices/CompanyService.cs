using System;
using System.Threading.Tasks;
using aesth_clic.Repository;

namespace aesth_clic.Services.SuperAdminServices
{
    internal class CompanyService
    {
        private readonly CompanyRepository _companyRepository;

        public CompanyService(CompanyRepository companyRepository)
        {
            _companyRepository =
                companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        }

        // Update only company status
        public async Task<bool> UpdateCompanyStatusAsync(int companyId, string newStatus)
        {
            if (companyId <= 0)
                throw new ArgumentException("Invalid owner ID.", nameof(companyId));

            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Status cannot be empty.", nameof(newStatus));

            return await _companyRepository.UpdateStatusAsync(companyId, newStatus);
        }
    }
}
