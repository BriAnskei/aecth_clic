using aesth_clic.Context;
using aesth_clic.Master.Dto.Company;
using aesth_clic.Master.Model;
using aesth_clic.Tenant.Model;

using aesth_clic.Util;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace aesth_clic.Master.Services
{
    public sealed class CompanyService(
        MasterDbContext masterDb,
        TenantDbContextFactory tenantFactory)
    {
        private readonly MasterDbContext _masterDb = masterDb ?? throw new ArgumentNullException(nameof(masterDb));
        private readonly TenantDbContextFactory _tenantFactory = tenantFactory ?? throw new ArgumentNullException(nameof(tenantFactory));

        public async Task CreateClinicAsync(NewClientUserDto request)
        {
            ValidateRequest(request);

            var client = request.Client;
            var adminUser = request.AdminUser;

            InitializeClient(client);

            await SaveClientToMasterDatabaseAsync(client);

            try
            {
                await InitializeTenantDatabaseAsync(client.DbName, adminUser);
            }
            catch
            {
                await RollbackClientAsync(client);
                throw;
            }
        }

        #region Private Methods

        private static void ValidateRequest(NewClientUserDto request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.Client is null)
                throw new ArgumentException("Client cannot be null.", nameof(request.Client));

            if (request.AdminUser is null)
                throw new ArgumentException("Admin user cannot be null.", nameof(request.AdminUser));
        }

        private void InitializeClient(Client client)
        {
            client.GenerateDbName();
         
        }

        private async Task SaveClientToMasterDatabaseAsync(Client client)
        {
            try
            {
                _masterDb.Clients.Add(client);
                await _masterDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                LogError("Failed to save client to master database.", ex);
                throw new MasterDatabaseException(
                    "Failed to create clinic in master database.", ex);
            }
        }

        private async Task InitializeTenantDatabaseAsync(string dbName, User adminUser)
        {
            try
            {
              using  var tenantDb = _tenantFactory.Create(dbName);

                // migrate all tables of tenant
                await tenantDb.Database.EnsureCreatedAsync();

                BycrptUtil.HashUserPassword(adminUser);
                tenantDb.Users.Add(adminUser);
                await tenantDb.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                LogError("Tenant database update failed.", ex);
                throw;
            }
            catch (Exception ex)
            {
                LogError("Tenant database initialization failed.", ex);
                throw new TenantDatabaseException(
                    "Clinic was created in master DB but tenant DB setup failed.", ex);
            }
        }

        private async Task RollbackClientAsync(Client client)
        {
            try
            {
                _masterDb.Clients.Remove(client);
                await _masterDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                LogError("Rollback failed for master database.", ex);
            }
        }

        private static void LogError(string message, Exception ex)
        {
            Debug.WriteLine(message);
            Debug.WriteLine(ex.ToString());
        }

       


           public async Task<Client> FetchClientAdminByCLinicCOde(string clinicCode)
        {
            var client = await _masterDb.Clients
                .FirstOrDefaultAsync(c => c.ClinicCode == clinicCode);

            if (client == null)
                throw new Exception("Client not found.");

            return client;
        }







        #endregion


        public async Task UpdateClientStatusAsync(string clinicCode, string newStatus)
        {
         
            // 1️⃣ Fetch client
            var client = await _masterDb.Clients
                .FirstOrDefaultAsync(c => c.ClinicCode == clinicCode);

            if (client == null)
                throw new Exception("Client not found.");

            // 2️⃣ Update status
            client.Status = newStatus.ToLower();

            // 3️⃣ Save changes
            await _masterDb.SaveChangesAsync();
        }


        public async Task UpdateClientTierAsync(string clinicCode, string newTier)
        {

            // Normalize tier value
            newTier = newTier.Trim().ToLower();


            // Fetch client
            var client = await _masterDb.Clients
                .FirstOrDefaultAsync(c => c.ClinicCode == clinicCode);

            if (client == null)
                throw new Exception("Client not found.");

            // Update tier
            client.Tier = newTier;

            // Save changes
            await _masterDb.SaveChangesAsync();
        }



        public async Task DeleteClientAsync(string clinicCode)
        {
            if (string.IsNullOrWhiteSpace(clinicCode))
                throw new ArgumentException("Invalid clinic code.");

            var client = await _masterDb.Clients
                .FirstOrDefaultAsync(c => c.ClinicCode == clinicCode);

            if (client == null)
                throw new Exception("Client not found.");

            var dbName = client.DbName;

            // Safety validation (VERY important)
            if (!System.Text.RegularExpressions.Regex.IsMatch(dbName, @"^[a-zA-Z0-9_]+$"))
                throw new Exception("Invalid database name format.");

            try
            {
                // Build safe dynamic SQL using QUOTENAME
                var sql = @"
            DECLARE @sql NVARCHAR(MAX);

            SET @sql = N'
                ALTER DATABASE ' + QUOTENAME(@dbName) + ' SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE ' + QUOTENAME(@dbName);

            EXEC sp_executesql @sql;
        ";

                await _masterDb.Database.ExecuteSqlRawAsync(
                    sql,
                    new SqlParameter("@dbName", dbName)
                );

                // Remove client record from master
                _masterDb.Clients.Remove(client);
                await _masterDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting client {clinicCode}: {ex}");

                throw new Exception(
                    $"Failed to delete client {clinicCode}.", ex);
            }
        }




    }

    public class MasterDatabaseException : Exception
    {
        public MasterDatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class TenantDatabaseException : Exception
    {
        public TenantDatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }








   
}