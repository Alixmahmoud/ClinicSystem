using System.Text.Json;
using Clinic.Authority.Domain.Abstractions;
using Clinic.Authority.Domain.Security;
using Clinic.Authority.Domain.Queue;
using Clinic.Authority.Domain.Sync;
using Clinic.Contracts.Operations;

namespace Clinic.Authority.Application.CheckIn;

/// <summary>
/// Executes the frozen Authority processing pipeline for CheckInAppointment. Controller and
/// endpoint code must NOT implement domain rules; this use case is the sole implementation.
/// </summary>
public sealed class CheckInAppointmentProcessor : ICheckInAppointmentProcessor
{
    private readonly IUserAccountStore _userAccountStore;
    private readonly IWorkstationTrustStore _workstationTrustStore;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientFlowRepository _patientFlowRepository;
    private readonly IProcessedOperationRepository _processedOperationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IAuthoritativeChangeRepository _authoritativeChangeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckInAppointmentProcessor(
        IUserAccountStore userAccountStore,
        IWorkstationTrustStore workstationTrustStore,
        IAuthorizationService authorizationService,
        IAppointmentRepository appointmentRepository,
        IPatientFlowRepository patientFlowRepository,
        IProcessedOperationRepository processedOperationRepository,
        IAuditLogRepository auditLogRepository,
        IAuthoritativeChangeRepository authoritativeChangeRepository,
        IUnitOfWork unitOfWork)
    {
        _userAccountStore = userAccountStore;
        _workstationTrustStore = workstationTrustStore;
        _authorizationService = authorizationService;
        _appointmentRepository = appointmentRepository;
        _patientFlowRepository = patientFlowRepository;
        _processedOperationRepository = processedOperationRepository;
        _auditLogRepository = auditLogRepository;
        _authoritativeChangeRepository = authoritativeChangeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult> ProcessAsync(OperationEnvelope envelope, CancellationToken cancellationToken = default)
    {
        var processedAt = DateTimeOffset.UtcNow;

        // 1. Validate envelope (structural / protocol).
        var (valid, failureCategory, failureMessage) = ValidateEnvelope(envelope);
        if (!valid)
        {
            return OperationResult.Failure(
                envelope.OperationId,
                failureCategory,
                envelope.AggregateType,
                envelope.AggregateId,
                failureMessage,
                processedAt);
        }

        // Decode the typed command payload.
        if (!CheckInAppointmentCommandCodec.TryDecode(envelope.CommandPayloadJson, out var command, out var payloadError))
        {
            return OperationResult.Failure(
                envelope.OperationId,
                OperationFailureCategory.MalformedProtocol,
                envelope.AggregateType,
                envelope.AggregateId,
                payloadError,
                processedAt);
        }

        if (command.AppointmentId != envelope.AggregateId)
        {
            return OperationResult.Failure(
                envelope.OperationId,
                OperationFailureCategory.Validation,
                envelope.AggregateType,
                envelope.AggregateId,
                $"Command payload AppointmentId {command.AppointmentId} does not match envelope AggregateId {envelope.AggregateId}.",
                processedAt);
        }

        var baseVersion = envelope.BaseVersion!.Value;

        // 2. Authenticate: human identity + workstation trust (fail closed).
        var user = await _userAccountStore.FindByIdAsync(envelope.UserId, cancellationToken);
        if (user is null)
        {
            return OperationResult.Failure(
                envelope.OperationId,
                OperationFailureCategory.Authentication,
                envelope.AggregateType,
                envelope.AggregateId,
                "Unknown user identity.",
                processedAt);
        }

        var workstationTrusted = await _workstationTrustStore.IsTrustedAsync(envelope.OriginWorkstationId, cancellationToken);
        if (!workstationTrusted)
        {
            return OperationResult.Failure(
                envelope.OperationId,
                OperationFailureCategory.Authentication,
                envelope.AggregateType,
                envelope.AggregateId,
                "Workstation identity is not trusted.",
                processedAt);
        }

        // 3. Authorize (Authority-side authorization is authoritative).
        var authorized = await _authorizationService.CanCheckInAppointmentAsync(envelope.UserId, cancellationToken);
        if (!authorized)
        {
            return OperationResult.Failure(
                envelope.OperationId,
                OperationFailureCategory.Authorization,
                envelope.AggregateType,
                envelope.AggregateId,
                $"User '{user.DisplayName}' is not authorized for CheckInAppointment.",
                processedAt);
        }

        // 4. Check idempotency: one OperationId → at most one authoritative business effect.
        var alreadyProcessed = await _processedOperationRepository.GetAsync(
            envelope.OriginWorkstationId, envelope.OperationId, cancellationToken);
        if (alreadyProcessed is not null)
        {
            return OperationResult.AlreadyProcessed(
                envelope.OperationId,
                envelope.AggregateType,
                envelope.AggregateId,
                alreadyProcessed.ResultingVersion,
                processedAt);
        }

        // 5. Validate dependencies: CheckInAppointment has no inter-operation dependencies in
        //    this slice — the stage is executed explicitly and its verdict is "none required".
        //    This leaves the pipeline stage present rather than silently skipped.

        // 6. Validate BaseVersion / concurrency (optimistic concurrency: Version + BaseVersion).
        var appointment = await _appointmentRepository.GetAsync(envelope.AggregateId, cancellationToken);
        if (appointment is null)
        {
            return OperationResult.Rejected(
                envelope.OperationId,
                envelope.AggregateType,
                envelope.AggregateId,
                envelope.BaseVersion,
                "Appointment not found.",
                processedAt);
        }

        if (appointment.Version != baseVersion)
        {
            return OperationResult.Conflict(
                envelope.OperationId,
                envelope.AggregateType,
                envelope.AggregateId,
                baseVersion,
                appointment.Version,
                processedAt);
        }

        // 7. Execute domain operation (domain invariant enforcement lives in the aggregate).
        try
        {
            appointment.CheckIn();
        }
        catch (Domain.Common.DomainException ex)
        {
            return OperationResult.Rejected(
                envelope.OperationId,
                envelope.AggregateType,
                envelope.AggregateId,
                envelope.BaseVersion,
                ex.Message,
                processedAt);
        }

        // Check-in derives doctor and queue automatically: the queue/patient-flow state is a
        // separate aggregate (Patient ≠ Appointment ≠ Visit ≠ Patient Flow remains distinct).
        var flowEntry = new PatientFlowEntry(
            Guid.NewGuid(),
            appointment.Id,
            appointment.PatientId,
            appointment.DoctorId,
            processedAt);

        // 8-10. Persist transaction, create audit, publish authoritative change — one atomic unit.
        await _processedOperationRepository.AddAsync(
            new ProcessedOperation(
                envelope.OriginWorkstationId,
                envelope.OperationId,
                CheckInAppointmentCommandType.Value,
                AppointmentAggregateType.Value,
                appointment.Id,
                appointment.Version,
                processedAt),
            cancellationToken);

        await _patientFlowRepository.AddAsync(flowEntry, cancellationToken);

        await _auditLogRepository.AddAsync(
            new Domain.Audit.AuditLogEntry(
                envelope.OperationId,
                envelope.UserId,
                envelope.OriginWorkstationId,
                AppointmentAggregateType.Value,
                appointment.Id,
                "CheckInAppointment",
                JsonSerializer.Serialize(new
                {
                    appointment.PatientId,
                    appointment.DoctorId,
                    Status = appointment.Status.ToString(),
                    appointment.Version
                }),
                processedAt),
            cancellationToken);

        await _authoritativeChangeRepository.AddAsync(
            new AuthoritativeChange(
                envelope.OperationId,
                AppointmentAggregateType.Value,
                appointment.Id,
                appointment.Version,
                CheckInAppointmentCommandType.Value,
                JsonSerializer.Serialize(new
                {
                    appointment.PatientId,
                    appointment.DoctorId,
                    Status = appointment.Status.ToString(),
                    appointment.Version
                }),
                processedAt),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult.Accepted(
            envelope.OperationId,
            AppointmentAggregateType.Value,
            appointment.Id,
            appointment.Version,
            processedAt);
    }

    private static (bool IsValid, OperationFailureCategory Category, string Message) ValidateEnvelope(OperationEnvelope envelope)
    {
        if (envelope.OperationId == Guid.Empty)
        {
            return (false, OperationFailureCategory.MalformedProtocol, "OperationId is required.");
        }

        if (envelope.OriginWorkstationId == Guid.Empty)
        {
            return (false, OperationFailureCategory.MalformedProtocol, "OriginWorkstationId is required.");
        }

        if (envelope.UserId == Guid.Empty)
        {
            return (false, OperationFailureCategory.MalformedProtocol, "UserId is required.");
        }

        if (!string.Equals(envelope.AggregateType, AppointmentAggregateType.Value, StringComparison.Ordinal))
        {
            return (false, OperationFailureCategory.MalformedProtocol,
                $"Unsupported AggregateType '{envelope.AggregateType}' for this processor.");
        }

        if (!string.Equals(envelope.CommandType, CheckInAppointmentCommandType.Value, StringComparison.Ordinal))
        {
            return (false, OperationFailureCategory.MalformedProtocol,
                $"Unsupported CommandType '{envelope.CommandType}' for this processor.");
        }

        if (envelope.AggregateId == Guid.Empty)
        {
            return (false, OperationFailureCategory.Validation, "AggregateId is required.");
        }

        // CheckInAppointment modifies an existing aggregate, so BaseVersion must be supplied.
        if (envelope.BaseVersion is null)
        {
            return (false, OperationFailureCategory.Validation,
                "BaseVersion is required for CheckInAppointment (optimistic concurrency).");
        }

        return (true, OperationFailureCategory.Validation, string.Empty);
    }
}