namespace Clinic.Client.Domain.Abstractions;

/// <summary>
/// Single transaction boundary for a workstation use case backed by the local SQLite store:
/// the local business change and the durable sync intent (SyncOperation row) are committed
/// atomically on one SaveChangesAsync.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}