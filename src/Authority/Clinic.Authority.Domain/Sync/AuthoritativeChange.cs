namespace Clinic.Authority.Domain.Sync;

/// <summary>
/// Authoritative change feed / outbox entry. The workstation receives incoming
/// authoritative state via this business-operation oriented change; the change and the
/// cursor advancement (on the consuming side) must be atomic. Raw row replication is forbidden.
/// </summary>
public sealed class AuthoritativeChange
{
    private AuthoritativeChange()
    {
    }

    public AuthoritativeChange(
        Guid operationId,
        string aggregateType,
        Guid aggregateId,
        long version,
        string commandType,
        string changeJson,
        DateTimeOffset occurredAtUtc)
    {
        OperationId = operationId;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        Version = version;
        CommandType = commandType;
        ChangeJson = changeJson;
        OccurredAtUtc = occurredAtUtc;
        Published = false;
    }

    public long Id { get; private set; }
    public Guid OperationId { get; private set; }
    public string AggregateType { get; private set; } = string.Empty;
    public Guid AggregateId { get; private set; }
    public long Version { get; private set; }
    public string CommandType { get; private set; } = string.Empty;
    public string ChangeJson { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public bool Published { get; private set; }
}