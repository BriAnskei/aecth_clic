using aesth_clic.Context;
using aesth_clic.Master.Model;
using aesth_clic.Session;
using aesth_clic.Tenant.Model;
using aesth_clic.Util;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

public class AuthService
{
    private readonly MasterDbContext _masterDb;
    private readonly TenantDbContextFactory _tenantFactory;

    public AuthService(
        MasterDbContext masterDb,
        TenantDbContextFactory tenantFactory)
    {
        _masterDb = masterDb;
        _tenantFactory = tenantFactory;
    }

    public async Task<User> LoginAsync(
      string clinicCode,
      string username,
      string password)
    {
        Client? client = null;
        User? user = null;

        if (clinicCode == "0000")
        {
            user = await VerifySuperAdmin(username, password);
            client = new Client();
        }
        else
        {
            var clientAuthResponse = await ClientLogin(username, password, clinicCode);
            client = clientAuthResponse.client;
            user = clientAuthResponse.user;
        }

        AppSession.Instance.Login(client, user);
        return user;
    }


    private async Task<(User user, Client client)> ClientLogin(string username, string password, string clinicCode)
    {
     
        var client = await _masterDb.Clients
            .FirstOrDefaultAsync(c => c.ClinicCode == clinicCode);

        if (client == null)
            throw new Exception("Clinic not found or inactive.");

        if (client.Status != "active")
            throw new UnauthorizedAccessException("Your Company has been deactivated by the administrator.");

      
        using var tenantDb = _tenantFactory.Create(client.DbName);

  
        var user = await tenantDb.Users
            .Include(u => u.AccountStatus) // include account status
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            throw new Exception("Invalid credentials.");

     
        if (!user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            if (user.AccountStatus == null)
            {
                throw new Exception("Account status not found for this user.");
            }

            if (!user.AccountStatus.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Your account has been deactivated by the administrator.");
            }
        }

       
        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            throw new Exception("Invalid credentials.");

      
        return (user, client);
    }


    private async  Task<User> VerifySuperAdmin(string username, string password)
    {
        var admin = await _masterDb.Admins.FirstOrDefaultAsync(u => u.Username == username);


        if (admin == null || admin != null && admin.Password != password)
        {
            throw new Exception("Invalid credentials.");
        }

     
        return new User
        {
            Id = admin!.Id,
            FullName = admin.FullName!,
            Username = admin.Username!,
            Password = admin.Password!,
            Email = "", 
            PhoneNumber = "",
            Role = "super_admin", // Set role as Admin
            CreatedAt = DateTime.UtcNow
        };

    }
}