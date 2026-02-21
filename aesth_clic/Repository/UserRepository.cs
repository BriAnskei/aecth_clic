using aesth_clic.Data;
using aesth_clic.Models.Users;
using MySqlConnector;
using System.Threading.Tasks;

namespace aesth_clic.Repository
{
    internal class UserRepository
    {
        private readonly DbConnectionFactory _db;

        public UserRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, full_name, username, password, phone_number, role, created_at
                FROM users
                WHERE username = @username
                LIMIT 1";
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new User(
                id: reader.GetInt32("id"),
                fullName: reader.GetString("full_name"),
                username: reader.GetString("username"),
                password: reader.GetString("password"),
                phoneNumber: reader.IsDBNull(reader.GetOrdinal("phone_number")) ? null : reader.GetString("phone_number"),
                role: reader.GetString("role"),
                createdAt: reader.GetDateTime("created_at")
            );
        }

        public async Task<Company?> GetCompanyByUserIdAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, user_id, name, status
                FROM companies
                WHERE user_id = @userId
                LIMIT 1";
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new Company(
                id: reader.GetInt32("id"),
                user_id: reader.GetInt32("user_id"),
                name: reader.GetString("name"),
                status: reader.GetString("status")
            );
        }

        public async Task<CompanyModule?> GetCompanyModuleByCompanyIdAsync(int companyId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, company_id, module_type
                FROM company_modules
                WHERE company_id = @companyId
                LIMIT 1";
            cmd.Parameters.AddWithValue("@companyId", companyId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new CompanyModule(
                id: reader.GetInt32("id"),
                company_id: reader.GetInt32("company_id"),
                module_type: reader.GetString("module_type")
            );
        }
    }
}