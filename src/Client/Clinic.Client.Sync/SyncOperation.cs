using Clinic.Contracts.Operations;

namespace Clinic.Client.Sync;

/// <summary>
/// Durable sync intent record. One SyncOperation row exists per unique OperationId and is
/// written atomically with the local business change. The OperationId is the exact
/// idempotency identity and is never regenerated on retry. Non-terminal states are retryable.
/// </summary>
public sealed class SyncOperation
{
    public SyncOperation(
        Guid operationId,
        Guid originWorkstationId,
        Guid userId,
        string aggregateType,
        Guid aggregateId,
        string commandType,
        long? baseVersion,
        string commandPayloadJson,
        string protocolVersion,
        DateTimeOffset createdAtUtc)
    {
        if (operationId == Guid.Empty) throw new ArgumentException("OperationId is required.", nameof(operationId));
        if (originWorkstationId == Guid.Empty) throw new ArgumentException("OriginWorkstationId is required.", nameof(originWorkstationId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (aggregateId == Guid.Empty) throw new ArgumentException("AggregateId is required.", nameof(aggregateId));
        if (string.IsNullOrWhiteSpace(aggregateType)) throw new ArgumentException("AggregateType is required.", nameof(aggregateType));
        if (string.IsNullOrWhiteSpace(commandType)) throw new ArgumentException("CommandType is required.", nameof(commandType));
        if (string.IsNullOrWhiteSpace(commandPayloadJson)) throw new ArgumentException("Command payload is required.", nameof(commandPayloadJson));

        OperationId = operationId;
        OriginWorkstationId = originWorkstationId;
        UserId = userId;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        CommandType = commandType;
        BaseVersion = baseVersion;
        CommandPayloadJson = commandPayloadJson;
        ProtocolVersion = protocolVersion;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        State = SyncOperationState.Queued;
    }

    private SyncOperation()
    {
    }

    public long Id { get; private set; }
    public Guid OperationId { get; private set; }
    public Guid OriginWorkstationId { get; private set; }
    public Guid UserId { get; private set; }
    public string AggregateType { get; private set; } = string.Empty;
    public Guid AggregateId { get; private set; }
    public string CommandType { get; private set; } = string.Empty;
    public long? BaseVersion { get; private set; }
    public string CommandPayloadJson { get; private set; } = string.Empty;
    public string ProtocolVersion { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public SyncOperationState State { get; private set; }
    public OperationOutcome? ResultOutcome { get; private set; }
    public long? ResultNewVersion { get; private set; }
    public OperationFailureCategory? FailureCategory { get; private set; }
    public string? ResultMessage { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>True when the operation is not terminal and may be (re)submitted or recovered.</summary>
    public bool IsRetryable =>
        State is SyncOperationState.Queued
            or SyncOperationState.Submitting
            or SyncOperationState.TemporarilyUnavailable
            or SyncOperationState.UnknownOutcome;

    public bool IsTerminal =>
        State is SyncOperationState.Completed
            or SyncOperationState.Rejected
            or SyncOperationState.Conflict;

    public void MarkSubmitting()
    {
        GuardMutable();
        State = SyncOperationState.Submitting;
        AttemptCount++;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>After a recovery lookup proved the Authority recorded no effect, safely re-queue.</summary>
    public void MarkQueuedForRetry()
    {
        GuardMutable();
        State = SyncOperationState.Queued;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkCompleted(OperationResult result) => ApplyResult(result, SyncOperationState.Completed);

    public void MarkRejected(OperationResult result) => ApplyResult(result, SyncOperationState.Rejected);

    public void MarkConflict(OperationResult result) => ApplyResult(result, SyncOperationState.Conflict);

    public void MarkTemporarilyUnavailable(OperationResult result) => ApplyResult(result, SyncOperationState.TemporarilyUnavailable);

    public void MarkUnknownOutcome(string reason)
    {
        GuardMutable();
        State = SyncOperationState.UnknownOutcome;
        ResultOutcome = null;
        ResultNewVersion = null;
        FailureCategory = OperationFailureCategory.Network;
        ResultMessage = reason;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private void ApplyResult(OperationResult result, SyncOperationState targetState)
    {
        GuardMutable();
        State = targetState;
        ResultOutcome = result.Outcome;
        ResultNewVersion = result.NewVersion;
        FailureCategory = result.FailureCategory;
        ResultMessage = result.Message;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private void GuardMutable()
    {
        if (IsTerminal)
        {
            throw new InvalidOperationException(
                $"SyncOperation '{OperationId}' is terminal ({State}) and cannot transition.");
        }
    }
}