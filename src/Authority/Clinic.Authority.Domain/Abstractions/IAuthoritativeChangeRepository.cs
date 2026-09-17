using Clinic.Authority.Domain.Sync;

namespace Clinic.Authority.Domain.Abstractions;

/// <summary>
/// Authoritative change publication port (change feed / outbox). Incoming authoritative
/// change and cursor advancement must be atomic; the row is created inside the same
/// transaction as the business effect.
/// </summary>
public interface IAuthoritativeChangeRepository
{
    Task AddAsync(AuthoritativeChange change, CancellationToken cancellationToken = default);
}