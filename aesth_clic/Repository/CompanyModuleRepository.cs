using aesth_clic.Data;
using aesth_clic.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Repository
{
    internal class CompanyModuleRepository
    {
        private readonly DbConnectionFactory _db;

        public CompanyModuleRepository(DbConnectionFactory db)
        {
            _db = db;
        }



        public async Task<CompanyModule?> GetCompanyModuleByCompanyIdAsync(int companyId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, company_id, module_type
                FROM company_module
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
