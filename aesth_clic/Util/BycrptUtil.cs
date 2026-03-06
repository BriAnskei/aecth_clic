


using aesth_clic.Tenant.Model;
using System;

namespace aesth_clic.Util
{
    internal class BycrptUtil
    {
        public static void HashUserPassword(User user)
        {
            if (user.Password == "")
                return;

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Password = hashedPassword;
        }

        public static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
        }


        public static string HashStringPaswword(string password)
        {
            if (password == "")
                throw new ArgumentException("Password cannot be empty", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password);

        }
    }
}
