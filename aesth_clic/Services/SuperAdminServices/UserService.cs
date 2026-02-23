using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aesth_clic.Dto.SuperAdmin;
using aesth_clic.Models.Companies;
using aesth_clic.Models.Users;
using aesth_clic.Repository;
using Org.BouncyCastle.Asn1.Cmp;

namespace aesth_clic.Services.AccountsServices
{
    internal class UserService(
           UserRepository userRepository,
           CompanyRepository companyRepository,
           AccountStatusRepository accountStatusRepository)
    {

        private readonly UserRepository _userRepository = userRepository;
        private readonly CompanyRepository _companyRepository = companyRepository;
        private readonly AccountStatusRepository _accountStatusRepository = accountStatusRepository;




        // Super Admin action
        public async Task<AdminClients> CreateAdminClientAsync(AdminClients adminClient)
        {
            User? newUserData = adminClient.User;
            Company? company = adminClient.Company;

            if (newUserData is null || company is null)
                throw new ArgumentNullException("User or Company cannot be null.");

            int adminClientId = await _userRepository.CreateAsync(newUserData);


            company.owner_id = adminClientId;
             await _companyRepository.CreateAsync(company);


            // fetch data after creation
            var newAdminClientData = await _userRepository.FindAdminClientByIdAsync(adminClientId);
            return newAdminClientData;
        }




    }
}
