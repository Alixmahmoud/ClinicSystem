using System.IO;
using Clinic.Client.Infrastructure;
using Clinic.Client.Infrastructure.Persistence;
using Clinic.Client.Infrastructure.Persistence.Repositories;
using Clinic.Client.Sync;
using Clinic.Contracts.Development;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Clinic.Client.Infrastructure.Tests;

public sealed class WorkstationPersistenceTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"clinic-ws-test-{Guid.NewGuid():N}.db");
    private readonly string _connectionString;

    public WorkstationPersistenceTests()
    {
        _connectionString = WorkstationDbConnections.ForPath(_databasePath);
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    private WorkstationDbContext NewContext() => WorkstationDbConnections.CreateContext(_connectionString);

    [Fact]
    public async Task Initialize_AppliesMigrations_AndSeedsBootstrapFixture()
    {
        await WorkstationDbInitializer.InitializeAsync(_connectionString);

        await using var db = NewContext();
        var settings = Assert.Single(db.WorkstationSettings);
        Assert.Equal(DevelopmentSeed.WorkstationId, settings.WorkstationId);

        var appointment = await db.Appointments.FindAsync(DevelopmentSeed.AppointmentId);
        Assert.NotNull(appointment);
        Assert.Equal(1, appointment.Version);
    }

    [Fact]
    public async Task Initialize_IsIdempotent()
    {
        await WorkstationDbInitializer.InitializeAsync(_connectionString);
        await WorkstationDbInitializer.InitializeAsync(_connectionString);

        await using var db = NewContext();
        Assert.Single(db.WorkstationSettings);
        Assert.Single(db.Appointments);
    }

    [Fact]
    public async Task Appointment_PersistsAcrossContexts_Durably()
    {
        await WorkstationDbInitializer.InitializeAsync(_connectionString);

        await using (var db = NewContext())
        {
            var appointment = await db.Appointments.FindAsync(DevelopmentSeed.AppointmentId);
            appointment!.CheckIn();
            appointment.ConfirmVersion(2);
            await db.SaveChangesAsync();
        }

        await using var reloaded = NewContext();
        var persisted = await reloaded.Appointments.FindAsync(DevelopmentSeed.AppointmentId);
        Assert.NotNull(persisted);
        Assert.Equal(Domain.Appointments.AppointmentStatus.CheckedIn, persisted.Status);
        Assert.Equal(2, persisted.Version);
        Assert.Equal(2, persisted.ConfirmedVersion);
    }

    [Fact]
    public async Task SyncOperation_RoundTrips_AndIsFoundAsPending()
    {
        await WorkstationDbInitializer.InitializeAsync(_connectionString);
        var appointmentId = DevelopmentSeed.AppointmentId;
        var operationId = Guid.NewGuid();

        await using (var db = NewContext())
        {
            await new SyncOperationRepository(db).AddAsync(new SyncOperation(
                operationId,
                DevelopmentSeed.WorkstationId,
                DevelopmentSeed.ReceptionUserId,
                "Appointment",
                appointmentId,
                "CheckInAppointment",
                baseVersion: 1,
                commandPayloadJson: "{}",
                protocolVersion: SyncEnvelopeFactory.ProtocolVersion,
                createdAtUtc: DateTimeOffset.UtcNow));
            await db.SaveChangesAsync();
        }

        await using var reloaded = NewContext();
        var repository = new SyncOperationRepository(reloaded);
        var pending = await repository.GetPendingForAggregateAsync("Appointment", appointmentId, "CheckInAppointment");

        Assert.NotNull(pending);
        Assert.Equal(operationId, pending.OperationId);
        Assert.Equal(SyncOperationState.Queued, pending.State);
        Assert.Equal(1, pending.BaseVersion);
        Assert.Single(await repository.GetProcessableAsync(10));
        Assert.Single(await repository.ListPendingAsync());
    }

    [Fact]
    public async Task SyncOperation_OperationIdIsUniquePerWorkstationStore()
    {
        await WorkstationDbInitializer.InitializeAsync(_connectionString);
        var operationId = Guid.NewGuid();

        await using var db = NewContext();
        var repository = new SyncOperationRepository(db);
        await repository.AddAsync(CreateOperation(operationId));
        await repository.AddAsync(CreateOperation(operationId));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private static SyncOperation CreateOperation(Guid operationId) => new(
        operationId,
        DevelopmentSeed.WorkstationId,
        DevelopmentSeed.ReceptionUserId,
        "Appointment",
        DevelopmentSeed.AppointmentId,
        "CheckInAppointment",
        baseVersion: 1,
        commandPayloadJson: "{}",
        protocolVersion: SyncEnvelopeFactory.ProtocolVersion,
        createdAtUtc: DateTimeOffset.UtcNow);
}