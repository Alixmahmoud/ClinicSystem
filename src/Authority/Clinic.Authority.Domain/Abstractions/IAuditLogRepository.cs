using Clinic.Authority.Domain.Audit;

namespace Clinic.Authority.Domain.Abstractions;

/// <summary>
/// Business audit port. Business audit is NOT generic logging and NOT technical logs.
/// </summary>
public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}