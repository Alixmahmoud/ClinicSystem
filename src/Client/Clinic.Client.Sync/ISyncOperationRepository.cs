namespace Clinic.Client.Sync;

/// <summary>
/// Repository port for durable sync intents. Implementation lives in Client Infrastructure
/// (the same SQLite transaction that commits the local business change).
/// </summary>
public interface ISyncOperationRepository
{
    /// <summary>Retryable operations (queued, in-flight, temporarily unavailable, unknown outcome),
    /// oldest first, deterministically ordered.</summary>
    Task<IReadOnlyList<SyncOperation>> GetProcessableAsync(int limit, CancellationToken cancellationToken = default);

    /// <summary>Earliest retryable sync intent for a given aggregate/command, used to reuse the
    /// exact OperationId instead of generating a new one.</summary>
    Task<SyncOperation?> GetPendingForAggregateAsync(
        string aggregateType,
        Guid aggregateId,
        string commandType,
        CancellationToken cancellationToken = default);

    /// <summary>All retryable sync intents, for building the workstation's sync-status view.</summary>
    Task<IReadOnlyList<SyncOperation>> ListPendingAsync(CancellationToken cancellationToken = default);

    Task AddAsync(SyncOperation operation, CancellationToken cancellationToken = default);
    Task UpdateAsync(SyncOperation operation, CancellationToken cancellationToken = default);
}