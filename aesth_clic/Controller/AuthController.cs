using aesth_clic.Tenant.Model;
using System.Threading.Tasks;

namespace aesth_clic.Controller
{
    public class AuthController
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }


        public async Task<User> LoginAsync(string clinicCode, string username, string password)
        {
            var user = await _authService.LoginAsync(clinicCode, username, password);
            return user;

        }
    }
}