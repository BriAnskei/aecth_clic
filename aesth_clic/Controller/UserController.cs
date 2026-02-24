using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using aesth_clic.Models.Users;
using aesth_clic.Services.AccountsServices;

namespace aesth_clic.Controller
{
    internal class UserController(UserService userService)
    {
        private readonly UserService _userService =
            userService ?? throw new ArgumentNullException(nameof(userService));

        // ==============================
        // CREATE
        // ==============================
        public async Task<AdminClients> CreateAdminClientAsync(AdminClients adminClient)
        {
            if (adminClient is null)
                throw new ArgumentNullException(nameof(adminClient));

            return await _userService.CreateAdminClientAsync(adminClient);
        }

        // ==============================
        // READ
        // ==============================
        public async Task<List<AdminClients>> GetAllAdminsAsync()
        {
            return await _userService.GetAllAdminsAsync();
        }

        // ==============================
        // UPDATE
        // ==============================
        public async Task UpdateAdminClientAsync(AdminClients adminClient)
        {
            if (adminClient is null)
                throw new ArgumentNullException(nameof(adminClient));

            await _userService.UpdateAdminClientAsync(adminClient);
        }

        public async Task UpdateAdminTierAsync(int companyId, string newTier)
        {
            await _userService.UpdateAdminTierAsync(companyId, newTier);
        }

        // ==============================
        // DELETE
        // ==============================
        public async Task DeleteAdminClientAsync(int id)
        {
            await _userService.DeleteAdminClientAsync(id);
        }
    }
}
