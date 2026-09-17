using Clinic.Client.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clinic.Client.Infrastructure.DesignTime;

/// <summary>
/// Design-time factory so `dotnet ef` can build the workstation SQLite context for
/// migration generation without running the desktop app.
/// </summary>
public sealed class WorkstationDbContextFactory : IDesignTimeDbContextFactory<WorkstationDbContext>
{
    public WorkstationDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("CLINIC_WORKSTATION_CONNECTION")
                         ?? WorkstationDbConnections.ForPath(WorkstationDbConnections.DefaultFileName);
        return new WorkstationDbContext(new DbContextOptionsBuilder<WorkstationDbContext>()
            .UseSqlite(connection)
            .Options);
    }
}