using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using aesth_clic.Models.Companies;
using aesth_clic.Models.Users;
using aesth_clic.Repository;
using aesth_clic.Util;

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
            adminClient.ValidateAdminClient();
            return await _transactionManager.ExecuteAsync(
                async (conn, transaction) =>
                {
                    // already validated, so we can safely access these properties
                    User newUserData = adminClient.User!;
                    Company company = adminClient.Company!;

                    BycrptUtil.HashUserPassword(newUserData);

                    int adminClientId = await _userRepository.CreateAsync(
                        newUserData,
                        conn,
                        transaction
                    );

                    company.owner_id = adminClientId;

                    await _companyRepository.CreateAsync(company, conn, transaction);

                    return await _userRepository.FindAdminClientByIdAsync(
                        adminClientId,
                        conn,
                        transaction
                    );
                }
            );
        }

        public async Task<List<AdminClients>> GetAllAdminsAsync()
        {
            return await _userRepository.GetAllAdminsAsync();
        }

        public async Task deleteAdminCleint(int id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public async Task UpdateAdminTierAsync(int companyId, string newTier)
        {
            if (companyId <= 0)
                throw new ArgumentException("Invalid company ID.", nameof(companyId));

            if (string.IsNullOrWhiteSpace(newTier))
                throw new ArgumentException("Tier cannot be empty.", nameof(newTier));

            var updated = await _companyRepository.Updatetier(companyId, newTier);

            if (!updated)
                throw new InvalidOperationException("Tier update failed.");
        }

        public async Task UpdateAdminClientAsync(AdminClients adminClient)
        {
            adminClient.ValidateAdminClient();

            await _transactionManager.ExecuteAsync(
                async (conn, transaction) =>
                {
                    User user = adminClient.User!;
                    BycrptUtil.HashUserPassword(user);

                    await _userRepository.UpdateUserAsync(user, conn, transaction);
                    await _companyRepository.UpdateCompanyNameAsync(
                        adminClient.Company!,
                        conn,
                        transaction
                    );
                }
            );
        }

        public async Task DeleteAdminClientAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid admin ID.", nameof(id));

            var deleted = await _userRepository.DeleteAsync(id);

            if (!deleted)
                throw new InvalidOperationException("Admin not found or not deleted.");
        }
    }
}
