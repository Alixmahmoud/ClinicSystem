using Clinic.Client.Application.Identity;
using Clinic.Client.Domain.Abstractions;
using Clinic.Client.Domain.Appointments;
using Clinic.Client.Domain.Common;
using Clinic.Client.Domain.Queue;
using Clinic.Client.Sync;
using Clinic.Contracts.Commands;

namespace Clinic.Client.Application.CheckIn;

/// <summary>
/// Local-first CheckInAppointment use case.
/// Frozen rules honored:
///  - Local business change and durable sync intent are written in ONE atomic transaction.
///  - The exact OperationId is reused for a pending retryable intent, never regenerated.
///  - Domain invariant (Scheduled → CheckedIn only) is enforced locally by the aggregate;
///    the Authority re-enforces it and remains the source of truth.
/// </summary>
public sealed class CheckInAppointmentUseCase
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientFlowRepository _patientFlowRepository;
    private readonly ISyncOperationRepository _syncOperationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentIdentity _identity;

    public CheckInAppointmentUseCase(
        IAppointmentRepository appointmentRepository,
        IPatientFlowRepository patientFlowRepository,
        ISyncOperationRepository syncOperationRepository,
        IUnitOfWork unitOfWork,
        ICurrentIdentity identity)
    {
        _appointmentRepository = appointmentRepository;
        _patientFlowRepository = patientFlowRepository;
        _syncOperationRepository = syncOperationRepository;
        _unitOfWork = unitOfWork;
        _identity = identity;
    }

    public async Task<CheckInAppointmentResult> CheckInAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return new CheckInAppointmentResult(
                CheckInAppointmentLocalOutcome.AppointmentNotFound,
                appointmentId,
                Guid.Empty,
                0,
                false,
                "Appointment not found on this workstation.");
        }

        var existing = await _syncOperationRepository.GetPendingForAggregateAsync(
            AppointmentAggregateType.Value,
            appointmentId,
            CheckInAppointmentCommandName.Value,
            cancellationToken);

        if (appointment.Status == AppointmentStatus.CheckedIn)
        {
            return new CheckInAppointmentResult(
                CheckInAppointmentLocalOutcome.AlreadyCheckedIn,
                appointment.Id,
                existing?.OperationId ?? Guid.Empty,
                appointment.Version,
                existing is not null,
                existing is not null
                    ? "Already checked in locally; sync is still pending with the same OperationId."
                    : "Already checked in and synchronized.");
        }

        DomainException? domainError;
        try
        {
            appointment.CheckIn();
            domainError = null;
        }
        catch (InvalidTransitionException ex)
        {
            domainError = ex;
        }

        if (domainError is not null)
        {
            return new CheckInAppointmentResult(
                CheckInAppointmentLocalOutcome.InvalidTransition,
                appointment.Id,
                Guid.Empty,
                appointment.Version,
                existing is not null,
                domainError.Message);
        }

        var utcNow = DateTimeOffset.UtcNow;
        var operationId = existing?.OperationId ?? Guid.NewGuid();
        var baseVersion = existing?.BaseVersion ?? appointment.Version - 1;
        var payload = CheckInAppointmentCommandCodec.Encode(new CheckInAppointmentCommand(appointment.Id));

        if (existing is null)
        {
            var syncOperation = new SyncOperation(
                operationId,
                _identity.WorkstationId,
                _identity.UserId,
                AppointmentAggregateType.Value,
                appointment.Id,
                CheckInAppointmentCommandName.Value,
                baseVersion,
                payload,
                SyncEnvelopeFactory.ProtocolVersion,
                utcNow);

            await _syncOperationRepository.AddAsync(syncOperation, cancellationToken);
        }

        // The queue/patient-flow entry is a separate aggregate; a fresh one only when the
        // appointment transition itself is fresh (they are committed atomically together).
        if (!await _patientFlowRepository.ExistsForAppointmentAsync(appointment.Id, cancellationToken))
        {
            await _patientFlowRepository.AddAsync(
                new PatientFlowEntry(
                    Guid.NewGuid(),
                    appointment.Id,
                    appointment.PatientId,
                    appointment.DoctorId,
                    utcNow),
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CheckInAppointmentResult(
            CheckInAppointmentLocalOutcome.CheckedInAndQueuedForSync,
            appointment.Id,
            operationId,
            appointment.Version,
            true,
            null);
    }
}