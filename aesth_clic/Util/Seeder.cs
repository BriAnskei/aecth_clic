using BCrypt.Net;
using MySqlConnector;
using System;

namespace aesth_clic.Data
{
    public static class Seeder
    {
        private static string connectionString =
            "Server=localhost;Database=aesth_clic;User ID=root;Password=;Port=3306;";

        public static void SeedSuperAdmin()
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string checkQuery = "SELECT COUNT(*) FROM users WHERE role = 'super_admin'";
                using var checkCmd = new MySqlCommand(checkQuery, connection);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count == 0)
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword("SuperAdmin123");

                    string insertQuery = @"
                        INSERT INTO users 
                        (full_name, username, password, role, created_at)
                        VALUES
                        (@full_name, @username, @password, @role, NOW())";

                    using var insertCmd = new MySqlCommand(insertQuery, connection);

                    insertCmd.Parameters.AddWithValue("@full_name", "Super Admin");
                    insertCmd.Parameters.AddWithValue("@username", "superadmin");
                    insertCmd.Parameters.AddWithValue("@password", hashedPassword);
                    insertCmd.Parameters.AddWithValue("@role", "super_admin");

                    insertCmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                // Database-related error
                Console.WriteLine("Database error while seeding: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Any other unexpected error
                Console.WriteLine("Unexpected error while seeding: " + ex.Message);
            }
        }
    }
}