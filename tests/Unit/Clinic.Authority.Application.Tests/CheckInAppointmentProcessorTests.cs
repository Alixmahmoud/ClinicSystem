using Clinic.Authority.Application.CheckIn;
using Clinic.Authority.Application.Tests.Fakes;
using Clinic.Authority.Domain.Appointments;
using Clinic.Authority.Domain.Security;
using Clinic.Contracts.Commands;
using Clinic.Contracts.Operations;
using Xunit;

namespace Clinic.Authority.Application.Tests;

public sealed class CheckInAppointmentProcessorTests
{
    private readonly InMemoryUserAccountStore _users = new();
    private readonly InMemoryWorkstationTrustStore _workstations = new();
    private readonly StubAuthorizationService _authorization = new();
    private readonly InMemoryAppointmentRepository _appointments = new();
    private readonly InMemoryPatientFlowRepository _patientFlow = new();
    private readonly InMemoryProcessedOperationRepository _processedOperations = new();
    private readonly InMemoryAuditLogRepository _audit = new();
    private readonly InMemoryAuthoritativeChangeRepository _changes = new();
    private readonly CountingUnitOfWork _unitOfWork = new();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _workstationId = Guid.NewGuid();

    public CheckInAppointmentProcessorTests()
    {
        _users.ById[_userId] = new UserAccount(_userId, UserRole.Reception, "reception");
        _workstations.Trusted.Add(_workstationId);
    }

    private CheckInAppointmentProcessor CreateProcessor() =>
        new(_users, _workstations, _authorization, _appointments, _patientFlow, _processedOperations,
            _audit, _changes, _unitOfWork);

    private Appointment SeedAppointment(long version = 1)
    {
        var appointment = new Appointment(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1),
            AppointmentStatus.Scheduled, version);
        _appointments.Items[appointment.Id] = appointment;
        return appointment;
    }

    private OperationEnvelope CreateEnvelope(
        Guid appointmentId,
        long? baseVersion = 1,
        Guid? operationId = null,
        string commandType = CheckInAppointmentCommandType.Value,
        string aggregateType = AppointmentAggregateType.Value,
        Guid? userId = null,
        Guid? workstationId = null)
    {
        return new OperationEnvelope(
            operationId ?? Guid.NewGuid(),
            workstationId ?? _workstationId,
            userId ?? _userId,
            aggregateType,
            appointmentId,
            commandType,
            baseVersion,
            DateTimeOffset.UtcNow,
            "1.0",
            CheckInAppointmentCommandCodec.Encode(new CheckInAppointmentCommand(appointmentId)));
    }

    [Fact]
    public async Task Process_WithValidEnvelope_ReturnsAccepted_AndPersistsOneAtomicEffect()
    {
        var appointment = SeedAppointment();
        var envelope = CreateEnvelope(appointment.Id);

        var result = await CreateProcessor().ProcessAsync(envelope);

        Assert.Equal(OperationOutcome.Accepted, result.Outcome);
        Assert.Null(result.FailureCategory);
        Assert.Equal(2, result.NewVersion);
        Assert.Equal(AppointmentStatus.CheckedIn, appointment.Status);

        Assert.Single(_processedOperations.Items);
        Assert.Single(_patientFlow.Items);
        Assert.Single(_audit.Items);
        Assert.Single(_changes.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Process_WithSameOperationIdAgain_ReturnsAlreadyProcessed_WithoutSecondEffect()
    {
        var appointment = SeedAppointment();
        var envelope = CreateEnvelope(appointment.Id);
        var processor = CreateProcessor();

        var first = await processor.ProcessAsync(envelope);
        var second = await processor.ProcessAsync(envelope);

        Assert.Equal(OperationOutcome.Accepted, first.Outcome);
        Assert.Equal(OperationOutcome.AlreadyProcessed, second.Outcome);
        Assert.Equal(2, second.NewVersion);
        Assert.Single(_processedOperations.Items);
        Assert.Single(_patientFlow.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Process_WhenUserUnknown_ReturnsAuthenticationFailure_AndPersistsNothing()
    {
        var appointment = SeedAppointment();
        var envelope = CreateEnvelope(appointment.Id, userId: Guid.NewGuid());

        var result = await CreateProcessor().ProcessAsync(envelope);

        Assert.Equal(OperationFailureCategory.Authentication, result.FailureCategory);
        Assert.Null(result.Outcome);
        Assert.Empty(_processedOperations.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
    }

    [Fact]
    public async Task Process_WhenWorkstationNotTrusted_ReturnsAuthenticationFailure_FailClosed()
    {
        var appointment = SeedAppointment();
        var envelope = CreateEnvelope(appointment.Id, workstationId: Guid.NewGuid());

        var result = await CreateProcessor().ProcessAsync(envelope);

        Assert.Equal(OperationFailureCategory.Authentication, result.FailureCategory);
        Assert.Empty(_processedOperations.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Process_WhenUserNotAuthorized_ReturnsAuthorizationFailure_AndPersistsNothing()
    {
        var appointment = SeedAppointment();
        _authorization.AllowCheckIn = false;
        var envelope = CreateEnvelope(appointment.Id);

        var result = await CreateProcessor().ProcessAsync(envelope);

        Assert.Equal(OperationFailureCategory.Authorization, result.FailureCategory);
        Assert.Empty(_processedOperations.Items);
        Assert.Empty(_patientFlow.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Process_WhenBaseVersionMismatch_ReturnsConflict_AndPersistsNothing()
    {
        var appointment = SeedAppointment(version: 4);
        var envelope = CreateEnvelope(appointment.Id, baseVersion: 1);

        var result = await CreateProcessor().ProcessAsync(envelope);

        Assert.Equal(OperationOutcome.Conflict, result.Outcome);
        Assert.Equal(4, result.NewVersion);
        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        Assert.Empty(_processedOperations.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Process_WhenBaseVersionMissing_ReturnsValidationFailure()
    {
        var appointment = SeedAppointment();
        var envelope = CreateEnvelope(appointment.Id, baseVersion: null);

        var result = await CreateProcessor().ProcessAsync(envelope);

        Assert.Equal(OperationFailureCategory.Validation, result.FailureCategory);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Process_WhenCommandTypeUnsupported_ReturnsMalformedProtocol()
    {
        var appointment = SeedAppointment();
        var envelope = CreateEnvelope(appointment.Id, commandType: "SomeOtherCommand");

        var result = await CreateProcessor().ProcessAsync(envelope);

        Assert.Equal(OperationFailureCategory.MalformedProtocol, result.FailureCategory);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Process_WhenPayloadAppointmentIdDoesNotMatchAggregate_ReturnsValidationFailure()
    {
        var appointment = SeedAppointment();
        var envelope = new OperationEnvelope(
            Guid.NewGuid(),
            _workstationId,
            _userId,
            AppointmentAggregateType.Value,
            appointment.Id,
            CheckInAppointmentCommandType.Value,
            1,
            DateTimeOffset.UtcNow,
            "1.0",
            CheckInAppointmentCommandCodec.Encode(new CheckInAppointmentCommand(Guid.NewGuid())));

        var result = await CreateProcessor().ProcessAsync(envelope);

        Assert.Equal(OperationFailureCategory.Validation, result.FailureCategory);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}