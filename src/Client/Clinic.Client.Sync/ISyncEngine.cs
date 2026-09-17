namespace Clinic.Client.Sync;

/// <summary>
/// Drives durable sync intents toward the Clinic Authority and reconciles the local
/// projection. Frozen requirements honored: one OperationId → at most one authoritative
/// effect; unknown-outcome recovery by exact OperationId lookup, never blind replay and
/// never OperationId regeneration.
/// </summary>
public interface ISyncEngine
{
    /// <summary>Processes at most <paramref name="limit"/> retryable operations and returns how many made progress.</summary>
    Task<int> ProcessPendingAsync(int limit = 16, CancellationToken cancellationToken = default);
}