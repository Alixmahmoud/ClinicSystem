using Clinic.Authority.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Authority.Infrastructure;

/// <summary>
/// Connection string handling for local development and the end-to-end/integration tests.
/// The credential here is the local development PostgreSQL superuser; production credential
/// handling is a deployment open detail and must not be shipped in application config.
/// </summary>
public static class AuthorityDbConnections
{
    public const string Default = "Host=localhost;Port=5432;Database=clinic_authority;Username=postgres;Password=123456";

    /// <summary>Connection string for a specific database name on the local dev server.</summary>
    public static string ForDatabase(string databaseName) =>
        $"Host=localhost;Port=5432;Database={databaseName};Username=postgres;Password=123456";

    public static AuthorityDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<AuthorityDbContext>()
            .UseNpgsql(connectionString)
            .Options);
}

public static class AuthorityDbInitializer
{
    /// <summary>
    /// Applies the separate Authority PostgreSQL migrations. EF Core creates the database
    /// itself when it does not exist.
    /// </summary>
    public static async Task MigrateAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var db = AuthorityDbConnections.CreateContext(connectionString);
        await db.Database.MigrateAsync(cancellationToken);
    }

    public static async Task SeedAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var db = AuthorityDbConnections.CreateContext(connectionString);
        await Seeding.AuthoritySeeder.SeedAsync(db, cancellationToken);
    }
}