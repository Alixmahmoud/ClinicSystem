using Clinic.Client.Domain.Abstractions;
using Clinic.Client.Domain.Appointments;
using Clinic.Contracts.Operations;

namespace Clinic.Client.Sync;

/// <summary>
/// Application-level sync engine. Order of operations:
///  1. Queued / TemporarilyUnavailable intents are submitted directly (same OperationId).
///  2. Submitting / UnknownOutcome intents are recovered by exact OperationId lookup first;
///     a 404 proof of no effect safely re-queues the SAME OperationId for submission.
///  3. Every definitive authoritative reply maps to a terminal or retryable lifecycle state;
///     definitive failures (security, validation, protocol) are never blindly retried.
///  4. Accepted / AlreadyProcessed reconcile the local confirmed version under the same
///     transaction that commits the state transition.
/// </summary>
public sealed class SyncEngine : ISyncEngine
{
    private readonly ISyncOperationRepository _syncOperationRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISyncTransport _transport;

    public SyncEngine(
        ISyncOperationRepository syncOperationRepository,
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork,
        ISyncTransport transport)
    {
        _syncOperationRepository = syncOperationRepository;
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
        _transport = transport;
    }

    public async Task<int> ProcessPendingAsync(int limit = 16, CancellationToken cancellationToken = default)
    {
        var operations = await _syncOperationRepository.GetProcessableAsync(limit, cancellationToken);
        var progressed = 0;

        foreach (var operation in operations)
        {
            var submission = await SubmitOrRecoverAsync(operation, cancellationToken);
            var terminalOrRequeued = await ApplySubmissionAsync(operation, submission, cancellationToken);
            if (terminalOrRequeued)
            {
                progressed++;
            }
        }

        return progressed;
    }

    private async Task<SyncSubmitTransportResult> SubmitOrRecoverAsync(SyncOperation operation, CancellationToken cancellationToken)
    {
        if (operation.State is SyncOperationState.Submitting or SyncOperationState.UnknownOutcome)
        {
            // Never blindly re-submit an operation whose authoritative outcome is unknown.
            var recovery = await _transport.GetResultAsync(operation.OriginWorkstationId, operation.OperationId, cancellationToken);
            if (!recovery.DefiniteReplyReceived)
            {
                return recovery;
            }

            if (recovery.Result is null)
            {
                // 404: Authority recorded no effect. Safe to re-submit the same OperationId.
                operation.MarkSubmitting();
                return await _transport.SubmitAsync(SyncEnvelopeFactory.CreateEnvelope(operation), cancellationToken);
            }

            return recovery;
        }

        // Queued or TemporarilyUnavailable: Authority already answered "temporarily unavailable",
        // or we never started — direct submission with the same OperationId.
        operation.MarkSubmitting();
        return await _transport.SubmitAsync(SyncEnvelopeFactory.CreateEnvelope(operation), cancellationToken);
    }

    private async Task<bool> ApplySubmissionAsync(
        SyncOperation operation,
        SyncSubmitTransportResult submission,
        CancellationToken cancellationToken)
    {
        if (!submission.DefiniteReplyReceived)
        {
            if (submission.RetryableTransportFailure)
            {
                operation.MarkUnknownOutcome("No definitive Authority reply (network/timeout/server fault); outcome unknown until recovered by OperationId lookup.");
            }
            else
            {
                operation.MarkRejected(OperationResult.Failure(
                    operation.OperationId,
                    OperationFailureCategory.Network,
                    operation.AggregateType,
                    operation.AggregateId,
                    "Transport failed with no definitive Authority reply.",
                    DateTimeOffset.UtcNow));
            }

            await PersistAsync(operation, cancellationToken);
            return true;
        }

        var result = submission.Result;
        if (result is null)
        {
            operation.MarkUnknownOutcome("Authority replied without a definitive result; treated as unknown outcome.");
            await PersistAsync(operation, cancellationToken);
            return true;
        }

        switch (result.Outcome)
        {
            case OperationOutcome.Accepted:
            case OperationOutcome.AlreadyProcessed:
                await ReconcileAppointmentAsync(operation.AggregateId, result.NewVersion, cancellationToken);
                operation.MarkCompleted(result);
                break;
            case OperationOutcome.Conflict:
                operation.MarkConflict(result);
                break;
            case OperationOutcome.Rejected:
            case OperationOutcome.DependencyBlocked:
                operation.MarkRejected(result);
                break;
            case OperationOutcome.TemporarilyUnavailable:
                operation.MarkTemporarilyUnavailable(result);
                break;
            default:
                // Definitive failure categories never collapse into a generic error and are
                // terminal: security/validation/protocol faults must not be blindly retried.
                operation.MarkRejected(result);
                break;
        }

        await PersistAsync(operation, cancellationToken);
        return true;
    }

    private async Task ReconcileAppointmentAsync(Guid appointmentId, long? confirmedVersion, CancellationToken cancellationToken)
    {
        if (confirmedVersion is null)
        {
            return;
        }

        var appointment = await _appointmentRepository.GetAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return;
        }

        appointment.ConfirmVersion(confirmedVersion.Value);
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }

    private async Task PersistAsync(SyncOperation operation, CancellationToken cancellationToken)
    {
        await _syncOperationRepository.UpdateAsync(operation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}