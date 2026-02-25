using aesth_clic.Models.Users;

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
    }
}
