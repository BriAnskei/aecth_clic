using aesth_clic.Dto.SuperAdmin;
using aesth_clic.Models.Users;
using aesth_clic.Services.AccountsServices;
using aesth_clic.Services.AuthServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Controller
{
    internal class UserController(UserService userService)
    {
        private readonly UserService _userService = userService;


        public async Task<AdminClients> CreateAdminClientAsync(AdminClients adminClient)
        {
           return  await _userService.CreateAdminClientAsync(adminClient);
        }
    }
}
