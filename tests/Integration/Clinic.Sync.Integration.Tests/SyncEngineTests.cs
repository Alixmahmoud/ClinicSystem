using Clinic.Client.Domain.Appointments;
using Clinic.Client.Sync;
using Clinic.Contracts.Operations;
using Clinic.Sync.Integration.Tests.Fakes;
using Xunit;

namespace Clinic.Sync.Integration.Tests;

public sealed class SyncEngineTests
{
    private readonly InMemorySyncOperationRepository _syncOperations = new();
    private readonly InMemoryAppointmentRepository _appointments = new();
    private readonly CountingUnitOfWork _unitOfWork = new();
    private readonly ScriptedSyncTransport _transport = new();

    private Appointment SeedAppointment(long version = 2, long? confirmedVersion = 1)
    {
        var appointment = new Appointment(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1),
            AppointmentStatus.CheckedIn, version, confirmedVersion);
        _appointments.Items[appointment.Id] = appointment;
        return appointment;
    }

    private SyncOperation SeedOperation(Guid appointmentId, SyncOperationState state = SyncOperationState.Queued)
    {
        var operation = new SyncOperation(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Appointment", appointmentId, "CheckInAppointment",
            baseVersion: 1, commandPayloadJson: "{}", protocolVersion: SyncEnvelopeFactory.ProtocolVersion, createdAtUtc: DateTimeOffset.UtcNow);
        if (state != SyncOperationState.Queued)
        {
            operation.MarkSubmitting();
            if (state == SyncOperationState.UnknownOutcome) operation.MarkUnknownOutcome("lost reply");
        }

        _syncOperations.Items.Add(operation);
        return operation;
    }

    private SyncEngine CreateEngine() => new(_syncOperations, _appointments, _unitOfWork, _transport);

    [Fact]
    public async Task ProcessPending_WhenAuthorityAccepts_MarksCompleted_AndConfirmsVersion()
    {
        var appointment = SeedAppointment();
        var operation = SeedOperation(appointment.Id);
        _transport.SubmitHandler = envelope => SyncTransportResults.Definite(OperationResult.Accepted(
            envelope.OperationId, envelope.AggregateType, envelope.AggregateId, 2, DateTimeOffset.UtcNow));

        var progressed = await CreateEngine().ProcessPendingAsync();

        Assert.Equal(1, progressed);
        Assert.Equal(SyncOperationState.Completed, operation.State);
        Assert.Equal(OperationOutcome.Accepted, operation.ResultOutcome);
        Assert.Equal(2, appointment.ConfirmedVersion);
    }

    [Fact]
    public async Task ProcessPending_WhenOutcomeUnknown_RecoversByOperationId_WithoutBlindResubmit()
    {
        var appointment = SeedAppointment();
        var operation = SeedOperation(appointment.Id, SyncOperationState.UnknownOutcome);
        _transport.RecoveryHandler = (_, opId) => SyncTransportResults.Definite(OperationResult.AlreadyProcessed(
            opId, "Appointment", appointment.Id, 2, DateTimeOffset.UtcNow));

        var progressed = await CreateEngine().ProcessPendingAsync();

        Assert.Equal(1, progressed);
        Assert.Equal(SyncOperationState.Completed, operation.State);
        Assert.Equal(OperationOutcome.AlreadyProcessed, operation.ResultOutcome);
        Assert.Empty(_transport.SubmittedEnvelopes);
        Assert.Single(_transport.RecoveryLookups);
        Assert.Equal(operation.OperationId, _transport.RecoveryLookups[0].OperationId);
    }

    [Fact]
    public async Task ProcessPending_WhenRecoveryProvesNoEffect_ResubmitsTheSameOperationId()
    {
        var appointment = SeedAppointment();
        var operation = SeedOperation(appointment.Id, SyncOperationState.UnknownOutcome);
        _transport.RecoveryHandler = (_, _) => SyncTransportResults.Definite(null);
        _transport.SubmitHandler = envelope => SyncTransportResults.Definite(OperationResult.Accepted(
            envelope.OperationId, envelope.AggregateType, envelope.AggregateId, 2, DateTimeOffset.UtcNow));

        var progressed = await CreateEngine().ProcessPendingAsync();

        Assert.Equal(1, progressed);
        Assert.Equal(SyncOperationState.Completed, operation.State);
        var submitted = Assert.Single(_transport.SubmittedEnvelopes);
        Assert.Equal(operation.OperationId, submitted.OperationId);
    }

    [Fact]
    public async Task ProcessPending_OnTransientFailure_MarksUnknownOutcome_AndKeepsOperationId()
    {
        var appointment = SeedAppointment();
        var operation = SeedOperation(appointment.Id);
        _transport.SubmitHandler = _ => SyncTransportResults.TransientFailure();

        var progressed = await CreateEngine().ProcessPendingAsync();

        Assert.Equal(1, progressed);
        Assert.Equal(SyncOperationState.UnknownOutcome, operation.State);
        Assert.Equal(OperationFailureCategory.Network, operation.FailureCategory);
        Assert.Equal(1, operation.AttemptCount);
        Assert.Single(_transport.SubmittedEnvelopes);
    }

    [Fact]
    public async Task ProcessPending_OnDefinitiveAuthorizationFailure_IsTerminal_AndNeverRetried()
    {
        var appointment = SeedAppointment();
        var operation = SeedOperation(appointment.Id);
        _transport.SubmitHandler = envelope => SyncTransportResults.Definite(OperationResult.Failure(
            envelope.OperationId, OperationFailureCategory.Authorization, envelope.AggregateType, envelope.AggregateId,
            "not authorized", DateTimeOffset.UtcNow));

        await CreateEngine().ProcessPendingAsync();

        Assert.Equal(SyncOperationState.Rejected, operation.State);
        Assert.Equal(OperationFailureCategory.Authorization, operation.FailureCategory);
        Assert.False(operation.IsRetryable);

        var secondPass = await CreateEngine().ProcessPendingAsync();
        Assert.Equal(0, secondPass);
        Assert.Single(_transport.SubmittedEnvelopes);
    }

    [Fact]
    public async Task ProcessPending_OnTemporarilyUnavailable_StaysRetryable_AndSucceedsLater()
    {
        var appointment = SeedAppointment();
        var operation = SeedOperation(appointment.Id);
        _transport.SubmitHandler = envelope => SyncTransportResults.Definite(OperationResult.TemporarilyUnavailable(
            envelope.OperationId, envelope.AggregateType, envelope.AggregateId, "authority busy", DateTimeOffset.UtcNow));

        await CreateEngine().ProcessPendingAsync();
        Assert.Equal(SyncOperationState.TemporarilyUnavailable, operation.State);
        Assert.True(operation.IsRetryable);

        _transport.SubmitHandler = envelope => SyncTransportResults.Definite(OperationResult.Accepted(
            envelope.OperationId, envelope.AggregateType, envelope.AggregateId, 2, DateTimeOffset.UtcNow));
        await CreateEngine().ProcessPendingAsync();

        Assert.Equal(SyncOperationState.Completed, operation.State);
        Assert.Equal(2, _transport.SubmittedEnvelopes.Count);
        Assert.All(_transport.SubmittedEnvelopes, e => Assert.Equal(operation.OperationId, e.OperationId));
    }

    [Fact]
    public async Task ProcessPending_OnConflict_MarksConflict_Terminally()
    {
        var appointment = SeedAppointment();
        var operation = SeedOperation(appointment.Id);
        _transport.SubmitHandler = envelope => SyncTransportResults.Definite(OperationResult.Conflict(
            envelope.OperationId, envelope.AggregateType, envelope.AggregateId, 1, 7, DateTimeOffset.UtcNow));

        await CreateEngine().ProcessPendingAsync();

        Assert.Equal(SyncOperationState.Conflict, operation.State);
        Assert.Equal(OperationOutcome.Conflict, operation.ResultOutcome);
        Assert.False(operation.IsRetryable);
    }
}