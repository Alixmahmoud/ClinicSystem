using System.IO;
using Clinic.Client.Application.CheckIn;
using Clinic.Client.Application.Identity;
using Clinic.Client.Infrastructure;
using Clinic.Client.Infrastructure.Persistence;
using Clinic.Client.Infrastructure.Persistence.Repositories;
using Clinic.Client.Sync;
using Clinic.Contracts.Development;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ClientAppointmentStatus = Clinic.Client.Domain.Appointments.AppointmentStatus;

namespace Clinic.Persistence.Integration.Tests;

public sealed class LocalPersistenceTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"clinic-persistence-test-{Guid.NewGuid():N}.db");
    private readonly string _connectionString;

    public LocalPersistenceTests()
    {
        _connectionString = WorkstationDbConnections.ForPath(_databasePath);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    private WorkstationDbContext NewContext() =>
        WorkstationDbConnections.CreateContext(_connectionString);

    private static CheckInAppointmentUseCase CreateUseCase(WorkstationDbContext db) =>
        new(
            new AppointmentRepository(db),
            new PatientFlowRepository(db),
            new SyncOperationRepository(db),
            new WorkstationUnitOfWork(db),
            new CurrentIdentity
            {
                UserId = DevelopmentSeed.ReceptionUserId,
                UserName = DevelopmentSeed.ReceptionUsername,
                WorkstationId = DevelopmentSeed.WorkstationId,
                WorkstationName = DevelopmentSeed.WorkstationDisplayName
            });

    [Fact]
    public async Task CheckIn_CommitsLocalProjectionAndSyncIntentInOneTransaction()
    {
        await WorkstationDbInitializer.InitializeAsync(_connectionString);

        await using (var db = NewContext())
        {
            var result = await CreateUseCase(db).CheckInAsync(DevelopmentSeed.AppointmentId);
            Assert.Equal(CheckInAppointmentLocalOutcome.CheckedInAndQueuedForSync, result.Outcome);
        }

        await using var verification = NewContext();
        var appointment = await verification.Appointments.FindAsync(DevelopmentSeed.AppointmentId);
        Assert.NotNull(appointment);
        Assert.Equal(ClientAppointmentStatus.CheckedIn, appointment.Status);
        Assert.Equal(2, appointment.Version);

        var pending = await new SyncOperationRepository(verification)
            .GetPendingForAggregateAsync("Appointment", DevelopmentSeed.AppointmentId, "CheckInAppointment");
        Assert.NotNull(pending);
        Assert.Equal(SyncOperationState.Queued, pending.State);
        Assert.Single(verification.PatientFlowEntries);
    }

    [Fact]
    public async Task CheckIn_PendingSyncIntentSurvivesWorkstationRestart()
    {
        await WorkstationDbInitializer.InitializeAsync(_connectionString);
        Guid operationId;

        await using (var db = NewContext())
        {
            var result = await CreateUseCase(db).CheckInAsync(DevelopmentSeed.AppointmentId);
            operationId = result.OperationId;
        }

        // Simulate a process restart: brand new context over the same file.
        await using var restarted = NewContext();
        var pending = await new SyncOperationRepository(restarted)
            .GetPendingForAggregateAsync("Appointment", DevelopmentSeed.AppointmentId, "CheckInAppointment");

        Assert.NotNull(pending);
        Assert.Equal(operationId, pending.OperationId);
        Assert.Equal(1, pending.BaseVersion);
    }

    [Fact]
    public async Task CheckIn_WhenSyncIntentViolatesTheUniqueOperationId_BusinessChangeIsRolledBack()
    {
        await WorkstationDbInitializer.InitializeAsync(_connectionString);
        var operationId = Guid.NewGuid();

        await using (var db = NewContext())
        {
            var appointment = await db.Appointments.FindAsync(DevelopmentSeed.AppointmentId);
            appointment!.CheckIn();

            var repository = new SyncOperationRepository(db);
            await repository.AddAsync(CreateOperation(operationId));
            await repository.AddAsync(CreateOperation(operationId));

            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }

        await using var verification = NewContext();
        var persisted = await verification.Appointments.FindAsync(DevelopmentSeed.AppointmentId);
        Assert.NotNull(persisted);
        Assert.Equal(ClientAppointmentStatus.Scheduled, persisted.Status);
        Assert.Equal(1, persisted.Version);
        Assert.Empty(verification.SyncOperations);
        Assert.Empty(verification.PatientFlowEntries);
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