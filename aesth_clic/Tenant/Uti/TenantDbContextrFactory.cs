using aesth_clic.Context;
using Microsoft.EntityFrameworkCore;
using System;

public class TenantDbContextFactory
{
    private readonly string _baseConnection;

    public TenantDbContextFactory()
    {
        _baseConnection =
            "Server=localhost\\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True;";
    }

    public TenantDbContext Create(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentException("Invalid database name.");

        var options = new DbContextOptionsBuilder<TenantDbContext>();

        var connection = $"{_baseConnection}Database={databaseName};";

        options.UseSqlServer(connection);

        return new TenantDbContext(options.Options);
    }
}