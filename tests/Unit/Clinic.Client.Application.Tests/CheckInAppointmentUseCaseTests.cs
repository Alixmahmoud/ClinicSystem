using Clinic.Client.Application.CheckIn;
using Clinic.Client.Application.Identity;
using Clinic.Client.Application.Tests.Fakes;
using Clinic.Client.Domain.Appointments;
using Clinic.Client.Sync;
using Xunit;

namespace Clinic.Client.Application.Tests;

public sealed class CheckInAppointmentUseCaseTests
{
    private readonly FakeAppointmentRepository _appointments = new();
    private readonly FakePatientFlowRepository _patientFlow = new();
    private readonly FakeSyncOperationRepository _syncOperations = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CheckInAppointmentUseCase CreateUseCase() =>
        new(_appointments, _patientFlow, _syncOperations, _unitOfWork, new CurrentIdentity
        {
            UserId = Guid.NewGuid(),
            UserName = "reception",
            WorkstationId = Guid.NewGuid(),
            WorkstationName = "FRONT-DESK-1"
        });

    private Appointment SeedScheduled(long version = 1)
    {
        var appointment = new Appointment(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1),
            AppointmentStatus.Scheduled, version, confirmedVersion: version);
        _appointments.Items[appointment.Id] = appointment;
        return appointment;
    }

    [Fact]
    public async Task CheckIn_WhenScheduled_AppliesLocalTransition_AndPersistsSyncIntentInOneSave()
    {
        var appointment = SeedScheduled();
        var useCase = CreateUseCase();

        var result = await useCase.CheckInAsync(appointment.Id);

        Assert.Equal(CheckInAppointmentLocalOutcome.CheckedInAndQueuedForSync, result.Outcome);
        Assert.Equal(AppointmentStatus.CheckedIn, appointment.Status);
        Assert.Equal(2, appointment.Version);

        var syncOperation = Assert.Single(_syncOperations.Items);
        Assert.Equal(SyncOperationState.Queued, syncOperation.State);
        Assert.Equal(1, syncOperation.BaseVersion);
        Assert.Equal(appointment.Id, syncOperation.AggregateId);
        Assert.Equal(result.OperationId, syncOperation.OperationId);

        Assert.Single(_patientFlow.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CheckIn_WithExistingPendingIntent_ReusesTheSameOperationId()
    {
        var appointment = SeedScheduled();
        var identity = new CurrentIdentity
        {
            UserId = Guid.NewGuid(), UserName = "reception", WorkstationId = Guid.NewGuid(), WorkstationName = "FRONT-DESK-1"
        };
        var existingOperationId = Guid.NewGuid();
        _syncOperations.Items.Add(new SyncOperation(
            existingOperationId,
            identity.WorkstationId,
            identity.UserId,
            AppointmentAggregateType.Value,
            appointment.Id,
            CheckInAppointmentCommandName.Value,
            baseVersion: 1,
            commandPayloadJson: "{\"appointmentId\":\"" + appointment.Id + "\"}",
            protocolVersion: SyncEnvelopeFactory.ProtocolVersion,
            createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(-5)));
        _syncOperations.Items[^1].MarkUnknownOutcome("prior attempt lost its reply");

        var useCase = new CheckInAppointmentUseCase(
            _appointments, _patientFlow, _syncOperations, _unitOfWork, identity);

        var result = await useCase.CheckInAsync(appointment.Id);

        Assert.Equal(CheckInAppointmentLocalOutcome.CheckedInAndQueuedForSync, result.Outcome);
        Assert.Equal(existingOperationId, result.OperationId);
        Assert.Single(_syncOperations.Items);
        Assert.Equal(2, result.LocalVersion);
    }

    [Fact]
    public async Task CheckIn_WhenAlreadyCheckedIn_ReturnsAlreadyCheckedIn_AndAddsNoNewIntent()
    {
        var appointment = SeedScheduled();
        await CreateUseCase().CheckInAsync(appointment.Id);

        var result = await CreateUseCase().CheckInAsync(appointment.Id);

        Assert.Equal(CheckInAppointmentLocalOutcome.AlreadyCheckedIn, result.Outcome);
        Assert.True(result.HasPendingSync);
        Assert.Single(_syncOperations.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CheckIn_WhenAppointmentUnknown_ReturnsNotFound()
    {
        var result = await CreateUseCase().CheckInAsync(Guid.NewGuid());

        Assert.Equal(CheckInAppointmentLocalOutcome.AppointmentNotFound, result.Outcome);
        Assert.Empty(_syncOperations.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}