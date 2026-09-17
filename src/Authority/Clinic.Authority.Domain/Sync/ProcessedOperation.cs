namespace Clinic.Authority.Domain.Sync;

/// <summary>
/// Idempotency record. Keyed by (WorkstationId, OperationId). Written in the same
/// transaction as the single authoritative business effect it guards.
/// </summary>
public sealed class ProcessedOperation
{
    private ProcessedOperation()
    {
    }

    public ProcessedOperation(
        Guid workstationId,
        Guid operationId,
        string commandType,
        string aggregateType,
        Guid aggregateId,
        long? resultingVersion,
        DateTimeOffset processedAtUtc)
    {
        WorkstationId = workstationId;
        OperationId = operationId;
        CommandType = commandType;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        ResultingVersion = resultingVersion;
        ProcessedAtUtc = processedAtUtc;
    }

    public long Id { get; private set; }
    public Guid WorkstationId { get; private set; }
    public Guid OperationId { get; private set; }
    public string CommandType { get; private set; } = string.Empty;
    public string AggregateType { get; private set; } = string.Empty;
    public Guid AggregateId { get; private set; }
    public long? ResultingVersion { get; private set; }
    public DateTimeOffset ProcessedAtUtc { get; private set; }
}