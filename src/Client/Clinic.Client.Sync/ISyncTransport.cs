using Clinic.Contracts.Operations;

namespace Clinic.Client.Sync;

/// <summary>
/// Frozen: transport status must not be collapsed into business outcomes. The transport
/// reports whether it received a definitive Authority reply; the sync engine maps that to
/// the operation lifecycle and never lets HTTP status masquerade as a business outcome.
/// </summary>
public interface ISyncTransport
{
    Task<SyncSubmitTransportResult> SubmitAsync(OperationEnvelope envelope, CancellationToken cancellationToken = default);
    Task<SyncSubmitTransportResult> GetResultAsync(Guid workstationId, Guid operationId, CancellationToken cancellationToken = default);
}

/// <param name="DefiniteReplyReceived">The Authority (or its absence of a record) answered definitively.</param>
/// <param name="Result">Definitive authoritative result, if one was returned. Null on a 404 recovery lookup.</param>
/// <param name="RetryableTransportFailure">Request failed before a definitive reply (network/timeout/5xx). Only meaningful when <paramref name="DefiniteReplyReceived"/> is false.</param>
public sealed record SyncSubmitTransportResult(bool DefiniteReplyReceived, OperationResult? Result, bool RetryableTransportFailure);

public static class SyncTransportResults
{
    public static SyncSubmitTransportResult Definite(OperationResult? result) => new(true, result, false);
    public static SyncSubmitTransportResult TransientFailure() => new(false, null, true);
    public static SyncSubmitTransportResult NonRetryableFailure() => new(false, null, false);
}