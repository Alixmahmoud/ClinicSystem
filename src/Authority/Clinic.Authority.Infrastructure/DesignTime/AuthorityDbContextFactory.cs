using Clinic.Authority.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clinic.Authority.Infrastructure.DesignTime;

/// <summary>
/// Design-time factory so `dotnet ef` can build the Authority PostgreSQL context for
/// migration generation without running the API host.
/// </summary>
public sealed class AuthorityDbContextFactory : IDesignTimeDbContextFactory<AuthorityDbContext>
{
    public AuthorityDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("CLINIC_AUTHORITY_CONNECTION")
                         ?? AuthorityDbConnections.Default;
        return new AuthorityDbContext(new DbContextOptionsBuilder<AuthorityDbContext>()
            .UseNpgsql(connection)
            .Options);
    }
}