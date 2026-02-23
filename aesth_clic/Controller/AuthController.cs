using aesth_clic.Services.AuthServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Controller
{
    internal class AuthController(AuthService authService)
    {
        private readonly AuthService _authService = authService;

        public Task<(bool Success, string Message)> LoginAsync(string username, string password, string role)
            => _authService.LoginAsync(username, password, role);

        public void Logout()
            => _authService.Logout();
    }
}
