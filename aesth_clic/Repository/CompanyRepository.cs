using aesth_clic.Data;
using aesth_clic.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Repository
{
    internal class CompanyRepository
    {

        private readonly DbConnectionFactory _db;

        public CompanyRepository(DbConnectionFactory db)
        {
            _db = db;
        }


        public async Task<Company?> GetCompanyByUserIdAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, user_id, name, status
                FROM company
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

    }
}
