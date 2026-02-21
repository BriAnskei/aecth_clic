using aesth_clic.Models.Users;
using aesth_clic.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Services.AuthServices
{
    internal class AuthService
    {
        private readonly UserRepository userRepository;
        private readonly CompanyRepository companyRepository;
        private readonly CompanyModuleRepository companyModuleRepository;

        private const string SuperAdminRole = "SuperAdmin";

        public AuthService(UserRepository userRepository, CompanyRepository companyRepository, CompanyModuleRepository companyModuleRepository)
        {
            this.userRepository = userRepository;
            this.companyRepository = companyRepository;
            this.companyModuleRepository = companyModuleRepository;
        }

        public async Task<(bool Success, string Message)> LoginAsync(string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Username and password are required.");

            var user = await userRepository.GetUserByUsernameAsync(username);

            if (user is null || !VerifyPassword(password, user.Password!) || user.Role != role)
                return (false, "Invalid username or password.");

            await BuildSessionAsync(user);

            return (true, "Login successful.");
        }


        private static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            Boolean isPasswordValid =  BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);

            return isPasswordValid;
        }


        private async Task BuildSessionAsync(User user)
        {
            // SuperAdmin has no company — session only needs user data
            if (user.Role == SuperAdminRole)
            {
                _ = new UserSession(user, new Company(), new CompanyModule());
                return;
            }

            var company = await companyRepository.GetCompanyByUserIdAsync(user.Id);
            var companyModule = company is not null
                ? await companyModuleRepository.GetCompanyModuleByCompanyIdAsync(company.id)
                : null;

            _ = new UserSession(
                user,
                company ?? new Company(),
                companyModule ?? new CompanyModule()
            );
        }

        public  void Logout()
        {
            UserSession.Clear();
        }
    }
}
