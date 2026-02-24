using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using aesth_clic.Data;
using aesth_clic.Models.Companies;
using aesth_clic.Models.Users;
using MySqlConnector;

namespace aesth_clic.Repository
{
    internal class UserRepository(DbConnectionFactory db)
    {
        private readonly DbConnectionFactory _db =
            db ?? throw new ArgumentNullException(nameof(db));

        #region CREATE

        public async Task<int> CreateAsync(
            User user,
            MySqlConnection conn,
            MySqlTransaction transaction
        )
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            cmd.CommandText =
                @"
        INSERT INTO users 
        (full_name, email, username, password, phone_number, role, created_at)
        VALUES
        (@fullName, @email, @username, @password, @phoneNumber, @role, @createdAt);
        SELECT LAST_INSERT_ID();";

            AddUserParameters(cmd, user);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        #endregion

        #region READ

        public async Task<User?> GetByIdAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM users WHERE id = @id LIMIT 1";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return MapUser(reader);
        }

        public async Task<AdminClients> FindAdminClientByIdAsync(
            int userId,
            MySqlConnection conn,
            MySqlTransaction transaction
        )
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            cmd.CommandText =
                @"
        SELECT 
            u.id as user_id,
            u.full_name,
            u.email,
            u.username,
            u.password,
            u.phone_number,
            u.role,
            u.created_at,
            c.id as company_id,
            c.owner_id,
            c.name as company_name,
            c.status as company_status,
            c.module_tier
        FROM users u
        INNER JOIN company c ON u.id = c.owner_id
        WHERE u.id = @userId";

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                throw new InvalidOperationException($"User with ID {userId} not found");

            return MapToAdminClient(reader);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM users WHERE username = @username LIMIT 1";
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return MapUser(reader);
        }

        public async Task<List<User>> GetAllAsync()
        {
            var users = new List<User>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM users";

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }

        public async Task<List<AdminClients>> GetAllAdminsAsync()
        {
            var admins = new List<AdminClients>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();

            cmd.CommandText =
                @"
        SELECT 
            u.id as user_id,
            u.full_name,
            u.email,
            u.username,
            u.password,
            u.phone_number,
            u.role,
            u.created_at,
            c.id as company_id,
            c.owner_id,
            c.name as company_name,
            c.status as company_status,
            c.module_tier
        FROM users u
        INNER JOIN company c ON u.id = c.owner_id
        WHERE LOWER(u.role) = 'admin'
        ORDER BY u.created_at DESC";

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                admins.Add(MapToAdminClient(reader));
            }

            return admins;
        }

        #endregion

        #region UPDATE

        public async Task<bool> UpdateUserAsync(
            User user,
            MySqlConnection conn,
            MySqlTransaction transaction
        )
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            var query = new StringBuilder();
            query.AppendLine("UPDATE users SET");
            query.AppendLine("    full_name = @fullName,");
            query.AppendLine("    email = @email,");
            query.AppendLine("    username = @username,");
            query.AppendLine("    phone_number = @phoneNumber,");
            query.AppendLine("    role = @role");

            // Only update password if provided
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                query.AppendLine("    ,password = @password");
                cmd.Parameters.AddWithValue("@password", user.Password);
            }

            query.AppendLine("WHERE id = @id");

            cmd.CommandText = query.ToString();

            AddUserParameters(cmd, user);
            cmd.Parameters.AddWithValue("@id", user.Id);

            var affected = await cmd.ExecuteNonQueryAsync();
            return affected > 0;
        }

        #endregion

        #region DELETE

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM users WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            var affected = await cmd.ExecuteNonQueryAsync();
            return affected > 0;
        }

        #endregion

        #region Private Helpers

        private static void AddUserParameters(MySqlCommand cmd, User user)
        {
            cmd.Parameters.AddWithValue("@fullName", user.FullName);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@username", user.Username);
            cmd.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@role", user.Role);
        }

        private static AdminClients MapToAdminClient(MySqlDataReader reader)
        {
            var user = new User
            {
                Id = reader.GetInt32("user_id"),
                FullName = reader.GetString("full_name"),
                Email = reader.GetString("email"),
                Username = reader.GetString("username"),
                Password = reader.GetString("password"),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("phone_number"))
                    ? null
                    : reader.GetString("phone_number"),
                Role = reader.GetString("role"),
                CreatedAt = reader.GetDateTime("created_at"),
            };

            if (!user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"User with ID {user.Id} is not an admin");

            var company = new Company
            {
                id = reader.GetInt32("company_id"),
                owner_id = reader.GetInt32("owner_id"),
                name = reader.GetString("company_name"),
                status = reader.GetString("company_status"),
                module_tier = reader.GetString("module_tier"),
            };

            return new AdminClients(user, company);
        }

        private static User MapUser(MySqlDataReader reader)
        {
            return new User(
                id: reader.GetInt32("id"),
                fullName: reader.GetString("full_name"),
                email: reader.GetString("email"),
                username: reader.GetString("username"),
                password: reader.GetString("password"),
                phoneNumber: reader.IsDBNull(reader.GetOrdinal("phone_number"))
                    ? null
                    : reader.GetString("phone_number"),
                role: reader.GetString("role"),
                createdAt: reader.GetDateTime("created_at")
            );
        }

        #endregion
    }
}
