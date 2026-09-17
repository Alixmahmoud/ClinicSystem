namespace Clinic.Authority.Domain.Audit;

/// <summary>
/// Business audit entry. Separate from security events, technical logs and
/// synchronization diagnostics. Created inside the same transaction as the business effect.
/// </summary>
public sealed class AuditLogEntry
{
    private AuditLogEntry()
    {
    }

    public AuditLogEntry(
        Guid operationId,
        Guid userId,
        Guid workstationId,
        string aggregateType,
        Guid aggregateId,
        string actionName,
        string detailJson,
        DateTimeOffset atUtc)
    {
        OperationId = operationId;
        UserId = userId;
        WorkstationId = workstationId;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        ActionName = actionName;
        DetailJson = detailJson;
        AtUtc = atUtc;
    }

    public long Id { get; private set; }
    public Guid OperationId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid WorkstationId { get; private set; }
    public string AggregateType { get; private set; } = string.Empty;
    public Guid AggregateId { get; private set; }
    public string ActionName { get; private set; } = string.Empty;
    public string DetailJson { get; private set; } = string.Empty;
    public DateTimeOffset AtUtc { get; private set; }
}