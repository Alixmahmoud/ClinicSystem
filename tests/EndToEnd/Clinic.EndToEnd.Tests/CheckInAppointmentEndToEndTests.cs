using System.IO;
using Clinic.Authority.Api;
using Clinic.Client.Application.CheckIn;
using Clinic.Client.Application.Identity;
using Clinic.Client.Infrastructure;
using Clinic.Client.Infrastructure.Persistence;
using Clinic.Client.Infrastructure.Persistence.Repositories;
using Clinic.Client.Sync;
using Clinic.Contracts.Development;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AuthorityAppointmentStatus = Clinic.Authority.Domain.Appointments.AppointmentStatus;
using ClientAppointmentStatus = Clinic.Client.Domain.Appointments.AppointmentStatus;

namespace Clinic.EndToEnd.Tests;

/// <summary>
/// Exercises the complete vertical slice against real PostgreSQL and a real HTTP Authority:
/// WPF-tier use case → local SQLite → durable sync intent → HTTP → Authority pipeline →
/// PostgreSQL → authoritative result → local sync state.
/// </summary>
public sealed class CheckInAppointmentEndToEndTests : IAsyncLifetime
{
    private const string UnreachableAuthorityUrl = "http://127.0.0.1:9";

    private AuthorityTestDatabase _database = null!;
    private WebApplication _authority = null!;
    private string _authorityBaseUrl = null!;
    private string _sqlitePath = null!;
    private string _sqliteConnectionString = null!;

    public async Task InitializeAsync()
    {
        _database = await AuthorityTestDatabase.CreateAsync();

        _authority = Program.BuildApp(["--urls", "http://127.0.0.1:0"], _database.ConnectionString);
        await _authority.StartAsync();
        _authorityBaseUrl = _authority.Services
            .GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();

        _sqlitePath = Path.Combine(Path.GetTempPath(), $"clinic-e2e-{Guid.NewGuid():N}.db");
        _sqliteConnectionString = WorkstationDbConnections.ForPath(_sqlitePath);
        await WorkstationDbInitializer.InitializeAsync(_sqliteConnectionString);
    }

    public async Task DisposeAsync()
    {
        await _authority.StopAsync();
        await _authority.DisposeAsync();

        SqliteConnection.ClearAllPools();
        if (File.Exists(_sqlitePath))
        {
            File.Delete(_sqlitePath);
        }

        await _database.DisposeAsync();
    }

    private (WorkstationDbContext Db, CheckInAppointmentUseCase UseCase, SyncEngine Engine) CreateClient(string authorityUrl)
    {
        var db = WorkstationDbConnections.CreateContext(_sqliteConnectionString);
        var identity = new CurrentIdentity
        {
            UserId = DevelopmentSeed.ReceptionUserId,
            UserName = DevelopmentSeed.ReceptionUsername,
            WorkstationId = DevelopmentSeed.WorkstationId,
            WorkstationName = DevelopmentSeed.WorkstationDisplayName
        };

        var appointments = new AppointmentRepository(db);
        var patientFlow = new PatientFlowRepository(db);
        var syncOperations = new SyncOperationRepository(db);
        var unitOfWork = new WorkstationUnitOfWork(db);
        var transport = new AuthorityHttpTransport(authorityUrl);

        var useCase = new CheckInAppointmentUseCase(appointments, patientFlow, syncOperations, unitOfWork, identity);
        var engine = new SyncEngine(syncOperations, appointments, unitOfWork, transport);
        return (db, useCase, engine);
    }

    [Fact]
    public async Task FullPath_LocalCheckIn_SynchronizesAndReconcilesLocalState()
    {
        var (db, useCase, engine) = CreateClient(_authorityBaseUrl);

        var checkIn = await useCase.CheckInAsync(DevelopmentSeed.AppointmentId);
        Assert.Equal(CheckInAppointmentLocalOutcome.CheckedInAndQueuedForSync, checkIn.Outcome);

        var progressed = await engine.ProcessPendingAsync();
        Assert.Equal(1, progressed);

        db.ChangeTracker.Clear();
        var localOperation = await db.SyncOperations.SingleAsync();
        Assert.Equal(SyncOperationState.Completed, localOperation.State);
        Assert.Equal(2, localOperation.ResultNewVersion);
        var localAppointment = await db.Appointments.FindAsync(DevelopmentSeed.AppointmentId);
        Assert.Equal(2, localAppointment!.ConfirmedVersion);

        await using var authorityDb = _database.CreateContext();
        var authoritativeAppointment = await authorityDb.Appointments.SingleAsync(a => a.Id == DevelopmentSeed.AppointmentId);
        Assert.Equal(AuthorityAppointmentStatus.CheckedIn, authoritativeAppointment.Status);
        Assert.Equal(2, authoritativeAppointment.Version);
        Assert.Equal(1, await authorityDb.ProcessedOperations.CountAsync(p => p.OperationId == checkIn.OperationId));
        Assert.Equal(1, await authorityDb.PatientFlowEntries.CountAsync(e => e.AppointmentId == DevelopmentSeed.AppointmentId));
        Assert.Equal(1, await authorityDb.AuthoritativeChanges.CountAsync(c => c.OperationId == checkIn.OperationId));

        db.Dispose();
    }

    [Fact]
    public async Task ResubmittingTheSameOperationId_ProducesExactlyOneAuthoritativeEffect()
    {
        var (db, useCase, engine) = CreateClient(_authorityBaseUrl);
        var checkIn = await useCase.CheckInAsync(DevelopmentSeed.AppointmentId);
        await engine.ProcessPendingAsync();

        await using (var clientDb = WorkstationDbConnections.CreateContext(_sqliteConnectionString))
        {
            var operation = await clientDb.SyncOperations.SingleAsync();
            using var transport = new AuthorityHttpTransport(_authorityBaseUrl);
            var duplicate = await transport.SubmitAsync(SyncEnvelopeFactory.CreateEnvelope(operation));
            Assert.True(duplicate.DefiniteReplyReceived);
            Assert.Equal(Contracts.Operations.OperationOutcome.AlreadyProcessed, duplicate.Result!.Outcome);
        }

        await using var authorityDb = _database.CreateContext();
        Assert.Equal(1, await authorityDb.ProcessedOperations.CountAsync(p => p.OperationId == checkIn.OperationId));
        Assert.Equal(1, await authorityDb.PatientFlowEntries.CountAsync(e => e.AppointmentId == DevelopmentSeed.AppointmentId));

        db.Dispose();
    }

    [Fact]
    public async Task AuthorityUnavailable_LocalStatePersists_ThenRecoversWhenConnectivityReturns()
    {
        // Local-first: check-in commits even though the Authority is unreachable.
        var (offlineDb, offlineUseCase, offlineEngine) = CreateClient(UnreachableAuthorityUrl);
        await offlineUseCase.CheckInAsync(DevelopmentSeed.AppointmentId);

        var progressed = await offlineEngine.ProcessPendingAsync();
        Assert.Equal(1, progressed);

        offlineDb.ChangeTracker.Clear();
        var offlineOperation = await offlineDb.SyncOperations.SingleAsync();
        Assert.Equal(SyncOperationState.UnknownOutcome, offlineOperation.State);
        var offlineAppointment = await offlineDb.Appointments.FindAsync(DevelopmentSeed.AppointmentId);
        Assert.Equal(ClientAppointmentStatus.CheckedIn, offlineAppointment!.Status);
        offlineDb.Dispose();

        // Connectivity returns: recovery lookup proves no effect, then the SAME OperationId is submitted.
        var (onlineDb, _, onlineEngine) = CreateClient(_authorityBaseUrl);
        await onlineEngine.ProcessPendingAsync();

        onlineDb.ChangeTracker.Clear();
        var recovered = await onlineDb.SyncOperations.SingleAsync();
        Assert.Equal(SyncOperationState.Completed, recovered.State);
        Assert.Equal(offlineOperation.OperationId, recovered.OperationId);

        await using var authorityDb = _database.CreateContext();
        Assert.Equal(1, await authorityDb.ProcessedOperations.CountAsync(p => p.OperationId == recovered.OperationId));
        Assert.Equal(1, await authorityDb.PatientFlowEntries.CountAsync(e => e.AppointmentId == DevelopmentSeed.AppointmentId));
        onlineDb.Dispose();
    }

    [Fact]
    public async Task UnknownOutcome_WhenAuthorityAlreadyProcessed_IsRecoveredWithoutDuplicateEffect()
    {
        // 1. Local check-in, then a lost reply leaves the operation with an unknown outcome.
        var (db, useCase, offlineEngine) = CreateClient(UnreachableAuthorityUrl);
        var checkIn = await useCase.CheckInAsync(DevelopmentSeed.AppointmentId);
        await offlineEngine.ProcessPendingAsync();
        Assert.True(checkIn.OperationId != Guid.Empty);
        db.Dispose();

        // 2. The Authority actually processes the operation (the reply never reached the workstation).
        Guid operationId;
        await using (var clientDb = WorkstationDbConnections.CreateContext(_sqliteConnectionString))
        {
            var operation = await clientDb.SyncOperations.SingleAsync();
            operationId = operation.OperationId;
            using var transport = new AuthorityHttpTransport(_authorityBaseUrl);
            var delivered = await transport.SubmitAsync(SyncEnvelopeFactory.CreateEnvelope(operation));
            Assert.True(delivered.DefiniteReplyReceived);
            Assert.Equal(Contracts.Operations.OperationOutcome.Accepted, delivered.Result!.Outcome);
        }

        // 3. Sync engine recovery: lookup by exact OperationId discovers AlreadyProcessed — no replay.
        var (recoveredDb, _, onlineEngine) = CreateClient(_authorityBaseUrl);
        await onlineEngine.ProcessPendingAsync();

        recoveredDb.ChangeTracker.Clear();
        var recovered = await recoveredDb.SyncOperations.SingleAsync();
        Assert.Equal(SyncOperationState.Completed, recovered.State);
        Assert.Equal(Contracts.Operations.OperationOutcome.AlreadyProcessed, recovered.ResultOutcome);
        Assert.Equal(operationId, recovered.OperationId);

        await using var authorityDb = _database.CreateContext();
        Assert.Equal(1, await authorityDb.ProcessedOperations.CountAsync(p => p.OperationId == operationId));
        Assert.Equal(1, await authorityDb.PatientFlowEntries.CountAsync(e => e.AppointmentId == DevelopmentSeed.AppointmentId));
        recoveredDb.Dispose();
    }
}