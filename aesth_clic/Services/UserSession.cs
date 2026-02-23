using aesth_clic.Models.Companies;
using aesth_clic.Models.Users;

namespace aesth_clic.Services
{
    internal class UserSession
    {
        public static User? CurrentUser { get; private set; }
        public static Company? CurrentCompany { get; private set; }


        public static bool IsLoggedIn => CurrentUser is not null;
        public static bool IsSuperAdmin => CurrentUser?.Role == "SuperAdmin";

        public UserSession(User user, Company company)
        {
            CurrentUser = user;
            CurrentCompany = company;

        }

        public static void Clear()
        {
            CurrentUser = null;
            CurrentCompany = null;
       
        }
    }
}