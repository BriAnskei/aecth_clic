//using System.Threading.Tasks;
//using aesth_clic.Models.Companies;
//using aesth_clic.Models.Users;
//using aesth_clic.Repository;
//using aesth_clic.Util;

//namespace aesth_clic.Services.AuthServices
//{
//    internal class AuthService(
//        UserRepository userRepository,
//        CompanyRepository companyRepository,
//        AccountStatusRepository accountStatusRepository
//    )
//    {
//        private readonly UserRepository _userRepository = userRepository;
//        private readonly CompanyRepository _companyRepository = companyRepository;
//        private readonly AccountStatusRepository _accountStatusRepository = accountStatusRepository;

//        private const string SuperAdminRole = "super_admin";

//        public async Task<(bool Success, string Message)> LoginAsync(
//            string username,
//            string password,
//            string role
//        )
//        {
//            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
//                return (false, "Username and password are required.");

//            var user = await _userRepository.GetByUsernameAsync(username);

//            bool isPasswordValid = BycrptUtil.VerifyPassword(
//                password,
//                user?.Password ?? string.Empty
//            );

//            if (user is null || isPasswordValid || user.Role != role)
//            {
//                return (false, "Invalid username or password.");
//            }

//            var accessCheck = await VerifyAccountAccessibility(user);
//            if (!accessCheck.Success)
//                return accessCheck;

//            await BuildSessionAsync(user);

//            return (true, "Login successful.");
//        }

//        private async Task<(bool Success, string Message)> VerifyAccountAccessibility(User user)
//        {
//            bool isUserSuperAdmin = user.Role == "super_admin";
//            bool isUserAdmin = user.Role == "admin";

//            if (!isUserSuperAdmin)
//            {
//                Company? company = null;
//                AccountStatus? accountStatus = null;

//                if (isUserAdmin)
//                {
//                    company = await _companyRepository.GetCompanyByOwnerIdAsync(user.Id);
//                }
//                else
//                {
//                    accountStatus = await _accountStatusRepository.GetByUserIdAsync(user.Id);

//                    if (accountStatus is null)
//                        return (false, "Account status not found.");

//                    company = await _companyRepository.GetByIdAsync(accountStatus.CompanyId);
//                }

//                if (company is null)
//                    return (false, "Company not found.");

//                if (accountStatus != null && accountStatus.Status == "inactive")
//                    return (false, "Your account is inactive.");

//                if (company.status == "inactive")
//                    return (false, "Company is inactive.");
//            }

//            return (true, "Access granted");
//        }

//        private async Task BuildSessionAsync(User user)
//        {
//            if (user.Role == SuperAdminRole)
//            {
//                _ = new UserSession(user, new Company());
//                return;
//            }

//            var company = await _companyRepository.GetCompanyByOwnerIdAsync(user.Id);

//            _ = new UserSession(user, company ?? new Company());
//        }

//        public void Logout()
//        {
//            UserSession.Clear();
//        }
//    }
//}
