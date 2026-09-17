namespace Clinic.Contracts.Operations;

/// <summary>
/// Frozen domain distinction: OperationId is NOT RequestId / CorrelationId.
/// It is the exact idempotency identity of one synchronized business operation.
/// One OperationId can create at most one authoritative business effect.
/// </summary>
public sealed record OperationEnvelope(
    Guid OperationId,
    Guid OriginWorkstationId,
    Guid UserId,
    string AggregateType,
    Guid AggregateId,
    string CommandType,
    long? BaseVersion,
    DateTimeOffset CreatedAtUtc,
    string ProtocolVersion,
    string CommandPayloadJson);