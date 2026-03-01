using aesth_clic.Context;
using aesth_clic.Master.Dto.Company;
using aesth_clic.Master.Model;
using aesth_clic.Tenant.Model;
using aesth_clic.Tenant.Uti;
using aesth_clic.Util;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace aesth_clic.Master.Services
{
    public sealed class CompanyService
    {
        private readonly MasterDbContext _masterDb;
        private readonly TenantDbContextFactory _tenantFactory;

        public CompanyService(
            MasterDbContext masterDb,
            TenantDbContextFactory tenantFactory)
        {
            _masterDb = masterDb ?? throw new ArgumentNullException(nameof(masterDb));
            _tenantFactory = tenantFactory ?? throw new ArgumentNullException(nameof(tenantFactory));
        }

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
            GenerateClinicCode(client);
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
                var tenantDb = _tenantFactory.Create(dbName);

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

        private void GenerateClinicCode(Client client)
        {
            // TODO: Ensure uniqueness check before assigning

            var random = new Random();
            var code = random.Next(10000, 100000);

            client.ClinicCode = code.ToString();
        }

        #endregion
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