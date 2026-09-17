using Clinic.Authority.Domain.Sync;

namespace Clinic.Authority.Domain.Abstractions;

/// <summary>
/// Idempotency store: exactly one OperationId may create at most one authoritative
/// business effect. The idempotency record is written in the same transaction as the
/// business effect.
/// </summary>
public interface IProcessedOperationRepository
{
    Task<ProcessedOperation?> GetAsync(Guid workstationId, Guid operationId, CancellationToken cancellationToken = default);
    Task AddAsync(ProcessedOperation processedOperation, CancellationToken cancellationToken = default);
}