using aesth_clic.Data;
using aesth_clic.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Repository
{
    internal class AccountStatusRepository
    {
        private readonly DbConnectionFactory _db;

        public AccountStatusRepository(DbConnectionFactory db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // ==============================
        // CREATE
        // ==============================
        public async Task<int> CreateAsync(AccountStatus accountStatus)
        {
            if (accountStatus == null)
                throw new ArgumentNullException(nameof(accountStatus));

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO account_status (company_id, user_id, status)
                VALUES (@company_id, @user_id, @status);
                SELECT LAST_INSERT_ID();";

            cmd.Parameters.AddWithValue("@company_id", accountStatus.CompanyId);
            cmd.Parameters.AddWithValue("@user_id", accountStatus.UserId);
            cmd.Parameters.AddWithValue("@status", accountStatus.Status);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // ==============================
        // READ - Get By Id
        // ==============================
        public async Task<AccountStatus?> GetByIdAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, company_id, user_id, status
                FROM account_status
                WHERE id = @id
                LIMIT 1";

            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return MapAccountStatus(reader);
        }

        // ==============================
        // READ - Get By User Id
        // ==============================
        public async Task<AccountStatus?> GetByUserIdAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, company_id, user_id, status
                FROM account_status
                WHERE user_id = @user_id
                LIMIT 1";

            cmd.Parameters.AddWithValue("@user_id", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return MapAccountStatus(reader);
        }

        // ==============================
        // READ - Get By Company Id
        // ==============================
        public async Task<AccountStatus?> GetByCompanyIdAsync(int companyId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, company_id, user_id, status
                FROM account_status
                WHERE company_id = @company_id
                LIMIT 1";

            cmd.Parameters.AddWithValue("@company_id", companyId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return MapAccountStatus(reader);
        }

        // ==============================
        // READ - Get All
        // ==============================
        public async Task<List<AccountStatus>> GetAllAsync()
        {
            var results = new List<AccountStatus>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, company_id, user_id, status
                FROM account_status";

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(MapAccountStatus(reader));
            }

            return results;
        }

        // ==============================
        // UPDATE
        // ==============================
        public async Task<bool> UpdateAsync(AccountStatus accountStatus)
        {
            if (accountStatus == null)
                throw new ArgumentNullException(nameof(accountStatus));

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE account_status
                SET company_id = @company_id,
                    user_id = @user_id,
                    status = @status
                WHERE id = @id";

            cmd.Parameters.AddWithValue("@id", accountStatus.Id);
            cmd.Parameters.AddWithValue("@company_id", accountStatus.CompanyId);
            cmd.Parameters.AddWithValue("@user_id", accountStatus.UserId);
            cmd.Parameters.AddWithValue("@status", accountStatus.Status);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // ==============================
        // DELETE
        // ==============================
        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                DELETE FROM account_status
                WHERE id = @id";

            cmd.Parameters.AddWithValue("@id", id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // ==============================
        // Mapper
        // ==============================
        private AccountStatus MapAccountStatus(dynamic reader)
        {
            return new AccountStatus(
                id: reader.GetInt32("id"),
                userId: reader.GetInt32("user_id"),
                companyId: reader.GetInt32("company_id"),
                status: reader.GetString("status")
            );
        }
    }
}