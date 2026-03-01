using aesth_clic.Context;
using aesth_clic.Master.Controller;
using aesth_clic.Master.Dto;
using aesth_clic.Master.Dto.Company;
using aesth_clic.Tenant.Uti;
using aesth_clic.Util;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Master.Services
{
 
        public sealed class AdminClientService(
       MasterDbContext masterDb,
       TenantDbContextFactory tenantFactory)
        {
            private readonly MasterDbContext _masterDb = masterDb ?? throw new ArgumentNullException(nameof(masterDb));
            private readonly TenantDbContextFactory _tenantFactory = tenantFactory ?? throw new ArgumentNullException(nameof(tenantFactory));


        public async Task UpdateAdminUserAsync(UpdateAdminUserDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.ClinicCode))
                throw new ArgumentException("Clinic code is required.");

            // 1️⃣ Find client in Master DB using ClinicCode
            var client = await _masterDb.Clients
                .FirstOrDefaultAsync(c => c.ClinicCode == request.ClinicCode);

            if (client == null)
                throw new Exception("Clinic not found.");

            // 2️⃣ Connect to Tenant DB
            var tenantDb = _tenantFactory.Create(client.DbName);

            // 3️⃣ Find Admin User
            var adminUser = await tenantDb.Users
                .FirstOrDefaultAsync(u => u.Role.ToLower() == "admin");

            if (adminUser == null)
                throw new Exception("Admin user not found.");

            // 4️⃣ Update fields
            adminUser.FullName = request.FullName;
            adminUser.Email = request.Email;
            adminUser.PhoneNumber = request.PhoneNumber;
            adminUser.Username = request.Username;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                adminUser.Password = BycrptUtil.HashStringPaswword(request.Password);
            }

            await tenantDb.SaveChangesAsync();
        }


        public async Task<AdminClinicDetailsDto> GetAdminClinicDetailsAsync(string clinicCode)
        {
            if (string.IsNullOrWhiteSpace(clinicCode))
                throw new ArgumentException("Clinic code is required.");

            // 1️⃣ Get clinic from Master DB
            var client = await _masterDb.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClinicCode == clinicCode);

            if (client == null)
                throw new Exception("Clinic not found.");

            // 2️⃣ Connect to Tenant DB
            var tenantDb = _tenantFactory.Create(client.DbName);

            // 3️⃣ Get admin user
            var adminUser = await tenantDb.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Role.ToLower() == "admin");

            if (adminUser == null)
                throw new Exception("Admin user not found.");

            // 4️⃣ Map to DTO
            return new AdminClinicDetailsDto
            {
                FullName = adminUser.FullName,
                Email = adminUser.Email,
                PhoneNumber = adminUser.PhoneNumber,
                ClinicName = client.ClinicName,
                Tier = client.Tier,
                Status = client.Status
            };
        }

        public async Task<List<AdminClinicDetailsDto>> GetAllAdminClinicsAsync()
        {
            var result = new List<AdminClinicDetailsDto>();

        
            var clients = await _masterDb.Clients
                .AsNoTracking()
                .ToListAsync();

            foreach (var client in clients)
            {
                try
                {
                    var tenantDb = _tenantFactory.Create(client.DbName);


                    var adminUser = await tenantDb.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Role.ToLower() == "admin");

                    if (adminUser == null)
                        continue; // skip if no admin found

                    // 4️⃣ Map to DTO
                    result.Add(new AdminClinicDetailsDto
                    {
                        ClientId = client.Id,
                        ClinicCode = client.ClinicCode,
                        FullName = adminUser.FullName,
                        Email = adminUser.Email,
                        PhoneNumber = adminUser.PhoneNumber,
                        Username = adminUser.Username,
                        UserId = adminUser.Id,
                        ClinicName = client.ClinicName,
                        Tier = client.Tier,
                        Status = client.Status
                    });
                }
                catch
                {
                    // Optionally log error and skip broken tenant
                    continue;
                }
            }

            return result;
        }

    }
}
