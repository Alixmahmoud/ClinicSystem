namespace Clinic.Contracts.Operations;

/// <summary>
/// Definitive result of processing (or attempting to process) one OperationEnvelope.
/// Exactly one of Outcome / FailureCategory is set. This preserves the frozen
/// distinction between business outcomes and failure categories.
/// </summary>
public sealed record OperationResult(
    Guid OperationId,
    OperationOutcome? Outcome,
    OperationFailureCategory? FailureCategory,
    string AggregateType,
    Guid AggregateId,
    long? NewVersion,
    string? Message,
    DateTimeOffset ProcessedAtUtc)
{
    public bool IsAccepted => Outcome == OperationOutcome.Accepted;
    public bool IsAlreadyProcessed => Outcome == OperationOutcome.AlreadyProcessed;

    public static OperationResult Accepted(
        Guid operationId,
        string aggregateType,
        Guid aggregateId,
        long newVersion,
        DateTimeOffset processedAtUtc) =>
        new(operationId, OperationOutcome.Accepted, null, aggregateType, aggregateId, newVersion, null, processedAtUtc);

    public static OperationResult AlreadyProcessed(
        Guid operationId,
        string aggregateType,
        Guid aggregateId,
        long? newVersion,
        DateTimeOffset processedAtUtc) =>
        new(operationId, OperationOutcome.AlreadyProcessed, null, aggregateType, aggregateId, newVersion, null, processedAtUtc);

    public static OperationResult Conflict(
        Guid operationId,
        string aggregateType,
        Guid aggregateId,
        long expectedBaseVersion,
        long actualVersion,
        DateTimeOffset processedAtUtc) =>
        new(operationId, OperationOutcome.Conflict, null, aggregateType, aggregateId, actualVersion,
            $"Concurrency conflict: expected BaseVersion {expectedBaseVersion}, current version is {actualVersion}.",
            processedAtUtc);

    public static OperationResult Rejected(
        Guid operationId,
        string aggregateType,
        Guid aggregateId,
        long? version,
        string reason,
        DateTimeOffset processedAtUtc) =>
        new(operationId, OperationOutcome.Rejected, null, aggregateType, aggregateId, version, reason, processedAtUtc);

    public static OperationResult DependencyBlocked(
        Guid operationId,
        string aggregateType,
        Guid aggregateId,
        string reason,
        DateTimeOffset processedAtUtc) =>
        new(operationId, OperationOutcome.DependencyBlocked, null, aggregateType, aggregateId, null, reason, processedAtUtc);

    public static OperationResult TemporarilyUnavailable(
        Guid operationId,
        string aggregateType,
        Guid aggregateId,
        string reason,
        DateTimeOffset processedAtUtc) =>
        new(operationId, OperationOutcome.TemporarilyUnavailable, null, aggregateType, aggregateId, null, reason, processedAtUtc);

    public static OperationResult Failure(
        Guid operationId,
        OperationFailureCategory category,
        string aggregateType,
        Guid aggregateId,
        string message,
        DateTimeOffset processedAtUtc) =>
        new(operationId, null, category, aggregateType, aggregateId, null, message, processedAtUtc);
}