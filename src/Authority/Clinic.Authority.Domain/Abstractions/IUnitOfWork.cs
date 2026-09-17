namespace Clinic.Authority.Domain.Abstractions;

/// <summary>
/// Single transaction boundary for an Authority use case: all repository mutations
/// performed before one SaveChangesAsync are committed atomically (business change,
/// idempotency record, audit, authoritative change).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}