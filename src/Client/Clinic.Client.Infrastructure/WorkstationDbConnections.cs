using System.IO;
using Clinic.Client.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Client.Infrastructure;

/// <summary>
/// Connection string handling for the workstation-local SQLite store.
/// </summary>
public static class WorkstationDbConnections
{
    public const string DefaultFileName = "clinic-workstation.db";

    public static string DefaultLocal =>
        ForPath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClinicSystem",
            DefaultFileName));

    public static string ForPath(string dbFilePath) => $"Data Source={dbFilePath}";

    public static WorkstationDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<WorkstationDbContext>()
            .UseSqlite(connectionString)
            .Options);

    /// <summary>Creates the directory holding the SQLite file so a first run cannot fail on a missing folder.</summary>
    public static void EnsureStoreDirectory(string connectionString)
    {
        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}

/// <summary>
/// Applies the workstation SQLite migrations and idempotently seeds the development
/// bootstrap fixture (matching the Authority's DevelopmentSeed values).
/// </summary>
public static class WorkstationDbInitializer
{
    public static async Task InitializeAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        WorkstationDbConnections.EnsureStoreDirectory(connectionString);
        await using var db = WorkstationDbConnections.CreateContext(connectionString);
        await db.Database.MigrateAsync(cancellationToken);
        await SeedAsync(db, cancellationToken);
    }

    public static async Task SeedAsync(WorkstationDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.WorkstationSettings.AnyAsync(cancellationToken))
        {
            db.WorkstationSettings.Add(new Persistence.Settings.WorkstationSettings(
                Clinic.Contracts.Development.DevelopmentSeed.WorkstationId,
                Clinic.Contracts.Development.DevelopmentSeed.WorkstationDisplayName));
        }

        if (!await db.Appointments.AnyAsync(a => a.Id == Clinic.Contracts.Development.DevelopmentSeed.AppointmentId, cancellationToken))
        {
            db.Appointments.Add(new Domain.Appointments.Appointment(
                Clinic.Contracts.Development.DevelopmentSeed.AppointmentId,
                Clinic.Contracts.Development.DevelopmentSeed.PatientId,
                Clinic.Contracts.Development.DevelopmentSeed.DoctorId,
                scheduledAtUtc: DateTimeOffset.UtcNow.AddHours(1),
                Domain.Appointments.AppointmentStatus.Scheduled,
                version: 1,
                confirmedVersion: 1));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}