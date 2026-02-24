using aesth_clic.Data;
using aesth_clic.Models.Companies;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Repository
{
    internal class CompanyRepository
    {
        private readonly DbConnectionFactory _db;

        public CompanyRepository(DbConnectionFactory db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // ==============================
        // CREATE
        // ==============================
        public async Task<int> CreateAsync(
     Company company,
     MySqlConnection conn,
     MySqlTransaction transaction)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            cmd.CommandText = @"
        INSERT INTO company (owner_id, name, status, module_tier)
        VALUES (@owner_id, @name, @status, @module_tier);
        SELECT LAST_INSERT_ID();";

            cmd.Parameters.AddWithValue("@owner_id", company.owner_id);
            cmd.Parameters.AddWithValue("@name", company.name);
            cmd.Parameters.AddWithValue("@status", company.status);
            cmd.Parameters.AddWithValue("@module_tier", company.module_tier);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // ==============================
        // READ - Get By Id
        // ==============================
        public async Task<Company?> GetByIdAsync(int companyId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, owner_id, name, status, module_tier
                FROM company
                WHERE id = @id
                LIMIT 1";

            cmd.Parameters.AddWithValue("@id", companyId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return MapCompany(reader);
        }

        // ==============================
        // READ - Get By Owner Id
        // ==============================
        public async Task<Company?> GetCompanyByOwnerIdAsync(int ownerId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, owner_id, name, status, module_tier
                FROM company
                WHERE owner_id = @owner_id
                LIMIT 1";

            cmd.Parameters.AddWithValue("@owner_id", ownerId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return MapCompany(reader);
        }

        // ==============================
        // READ - Get All
        // ==============================
        public async Task<List<Company>> GetAllAsync()
        {
            var companies = new List<Company>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, owner_id, name, status, module_tier
                FROM company";

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                companies.Add(MapCompany(reader));
            }

            return companies;
        }

        // ==============================
        // UPDATE
        // ==============================
        public async Task<bool> UpdateAsync(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE company
                SET owner_id = @owner_id,
                    name = @name,
                    status = @status,
                    module_tier = @module_tier
                WHERE id = @id";

            cmd.Parameters.AddWithValue("@id", company.id);
            cmd.Parameters.AddWithValue("@owner_id", company.owner_id);
            cmd.Parameters.AddWithValue("@name", company.name);
            cmd.Parameters.AddWithValue("@status", company.status);
            cmd.Parameters.AddWithValue("@module_tier", company.module_tier);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // ==============================
        // DELETE
        // ==============================
        public async Task<bool> DeleteAsync(int companyId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                DELETE FROM company
                WHERE id = @id";

            cmd.Parameters.AddWithValue("@id", companyId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

  
        private Company MapCompany(dynamic reader)
        {
            return new Company(
                id: reader.GetInt32("id"),
                owner_id: reader.GetInt32("owner_id"),
                name: reader.GetString("name"),
                status: reader.GetString("status"),
                module_tier: reader.GetString("module_tier")
            );
        }
    }
}