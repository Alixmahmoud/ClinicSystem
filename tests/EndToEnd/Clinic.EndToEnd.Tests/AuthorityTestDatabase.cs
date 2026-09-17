using Clinic.Authority.Infrastructure;
using Clinic.Authority.Infrastructure.Persistence;
using Npgsql;

namespace Clinic.EndToEnd.Tests;

/// <summary>
/// Real PostgreSQL database for an end-to-end test: migrated, seeded, and dropped after.
/// </summary>
internal sealed class AuthorityTestDatabase : IAsyncDisposable
{
    private const string MasterConnection =
        "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123456";

    private AuthorityTestDatabase(string name)
    {
        Name = name;
        ConnectionString = AuthorityDbConnections.ForDatabase(name);
    }

    public string Name { get; }
    public string ConnectionString { get; }

    public static async Task<AuthorityTestDatabase> CreateAsync()
    {
        var database = new AuthorityTestDatabase($"clinic_e2e_{Guid.NewGuid():N}");
        await AuthorityDbInitializer.MigrateAsync(database.ConnectionString);
        await AuthorityDbInitializer.SeedAsync(database.ConnectionString);
        return database;
    }

    public AuthorityDbContext CreateContext() => AuthorityDbConnections.CreateContext(ConnectionString);

    public async ValueTask DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(MasterConnection);
        await connection.OpenAsync();

        await using (var terminate = connection.CreateCommand())
        {
            terminate.CommandText =
                "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @name AND pid <> pg_backend_pid()";
            terminate.Parameters.AddWithValue("name", Name);
            await terminate.ExecuteNonQueryAsync();
        }

        await using var drop = connection.CreateCommand();
        drop.CommandText = $"DROP DATABASE IF EXISTS \"{Name}\"";
        await drop.ExecuteNonQueryAsync();
    }
}