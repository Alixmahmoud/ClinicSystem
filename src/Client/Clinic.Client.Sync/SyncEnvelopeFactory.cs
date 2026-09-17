using Clinic.Client.Sync;
using Clinic.Contracts.Operations;

namespace Clinic.Client.Sync;

/// <summary>
/// Builds the wire-level OperationEnvelope for a durable sync intent. The envelope carries
/// the exact OperationId identity and is stable across retries, so the Authority can deduplicate.
/// </summary>
public static class SyncEnvelopeFactory
{
    /// <summary>Open transport detail (not an API version token).</summary>
    public const string ProtocolVersion = "1.0";

    public static OperationEnvelope CreateEnvelope(SyncOperation operation) =>
        new(
            operation.OperationId,
            operation.OriginWorkstationId,
            operation.UserId,
            operation.AggregateType,
            operation.AggregateId,
            operation.CommandType,
            operation.BaseVersion,
            operation.CreatedAtUtc,
            operation.ProtocolVersion,
            operation.CommandPayloadJson);
}