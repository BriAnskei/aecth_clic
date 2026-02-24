using aesth_clic.Data;
using aesth_clic.Models.Companies;
using aesth_clic.Models.Users;
using aesth_clic.Repository;
using aesth_clic.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace aesth_clic.Services.AccountsServices
{
    internal class UserService(
           UserRepository userRepository,
           CompanyRepository companyRepository,
           AccountStatusRepository accountStatusRepository,
            TransactionManager transactionManager
        )
    {

        private readonly UserRepository _userRepository = userRepository;
        private readonly CompanyRepository _companyRepository = companyRepository;
        private readonly AccountStatusRepository _accountStatusRepository = accountStatusRepository;
        private readonly TransactionManager _transactionManager = transactionManager;




        public async Task<AdminClients> CreateAdminClientAsync(AdminClients adminClient)
        {
            return await _transactionManager.ExecuteAsync(async (conn, transaction) =>
            {
                User? newUserData = adminClient.User;
                Company? company = adminClient.Company;

                if (newUserData is null || company is null)
                    throw new ArgumentNullException("User or Company cannot be null.");

                BycrptUtil.HashUserPassword(newUserData);

                int adminClientId = await _userRepository.CreateAsync(
                    newUserData, conn, transaction);

                company.owner_id = adminClientId;

                await _companyRepository.CreateAsync(
                    company, conn, transaction);
                return await _userRepository.FindAdminClientByIdAsync(adminClientId, conn, transaction);
            });
        }


        public async Task<List<AdminClients>> GetAllAdminsAsync()
        {
            return await _userRepository.GetAllAdminsAsync();
        }


    }
}
