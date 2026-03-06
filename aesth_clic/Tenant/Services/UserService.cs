using aesth_clic.Session;
using aesth_clic.Tenant.Model;
using aesth_clic.Util;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Services
{
    public sealed class UserService(TenantDbContextFactory tenantFactory) : TenantServiceBase
    {

        private readonly TenantDbContextFactory _tenantFactory = tenantFactory;


        public async Task AddUserAsync(User newUser)
        {
            using var tenantDb = CreateTenantDb();
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                // Validate
                newUser.ValidateForInsert();

                // Hash password
                BycrptUtil.HashUserPassword(newUser);

                newUser.CreatedAt = DateTime.UtcNow;

                // 1️⃣ Create User
                tenantDb.Users.Add(newUser);
                await tenantDb.SaveChangesAsync();

                // 2️⃣ Create Account Status for that user
                var accountStatus = new AccountStatus
                {
                    AccountId = newUser.Id,   // now we have the generated Id
                    Status = "Active"
                };

                tenantDb.AccountsStatus.Add(accountStatus);
                await tenantDb.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            using var tenantDb = CreateTenantDb();

            return await tenantDb.Users
                                 .Include(u => u.AccountStatus)
                                 .Where(u => u.AccountStatus != null)   // only users with account status(excluded admin)
                                 .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var tenantDb = CreateTenantDb();

            return await tenantDb.Users
                                 .FirstOrDefaultAsync(u => u.Id == id);
        }


        public async Task UpdateUserAsync(User updatedUser)
        {
            using var tenantDb = CreateTenantDb();

            var existingUser = await tenantDb.Users
                                              .FirstOrDefaultAsync(u => u.Id == updatedUser.Id);

            if (existingUser == null)
                throw new Exception("User not found.");



            existingUser.FullName = updatedUser.FullName;
            existingUser.Email = updatedUser.Email;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;
            existingUser.Username = updatedUser.Username;
            existingUser.Role = updatedUser.Role;

            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
            {
                BycrptUtil.HashUserPassword(updatedUser);
                existingUser.Password = updatedUser.Password;
            }

            await tenantDb.SaveChangesAsync();
        }

        public async Task UpdateAccountStatusAsync(int userId, string newStatus)
        {
            using var tenantDb = CreateTenantDb();

            var accountStatus = await tenantDb.AccountsStatus
                                              .FirstOrDefaultAsync(a => a.AccountId == userId);

            if (accountStatus == null)
                throw new Exception("Account status not found.");

            var allowedStatuses = new[] { "Active", "Deactivated" };

            if (!allowedStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                throw new Exception("Status must be Active or Deactivated.");

            accountStatus.Status = newStatus;

            await tenantDb.SaveChangesAsync();
        }


        public async Task DeleteUserAsync(int id)
        {
            using var tenantDb = CreateTenantDb();

            var user = await tenantDb.Users.FindAsync(id);

            if (user == null)
                throw new Exception("User not found.");

            tenantDb.Users.Remove(user);

            await tenantDb.SaveChangesAsync();
        }

    }
}
