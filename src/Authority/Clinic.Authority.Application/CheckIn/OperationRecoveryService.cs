using Clinic.Authority.Application.CheckIn;
using Clinic.Authority.Domain.Abstractions;
using Clinic.Contracts.Operations;

namespace Clinic.Authority.Application.CheckIn;

/// <summary>
/// Unknown-outcome recovery: the workstation looks its OperationId up on the Authority
/// after an outcome was lost. If a definitive result was recorded, the Authority returns it
/// (AlreadyProcessed + resulting version). If none was recorded, no business effect was
/// committed and the same OperationId may be re-submitted safely (never blind replay, never
/// a regenerated OperationId).
/// </summary>
public interface IOperationRecoveryService
{
    Task<OperationResult?> GetResultByOperationIdAsync(
        Guid workstationId,
        Guid operationId,
        CancellationToken cancellationToken = default);
}

public sealed class OperationRecoveryService(IProcessedOperationRepository processedOperationRepository)
    : IOperationRecoveryService
{
    public async Task<OperationResult?> GetResultByOperationIdAsync(
        Guid workstationId,
        Guid operationId,
        CancellationToken cancellationToken = default)
    {
        var processed = await processedOperationRepository.GetAsync(workstationId, operationId, cancellationToken);
        if (processed is null)
        {
            return null;
        }

        return OperationResult.AlreadyProcessed(
            processed.OperationId,
            processed.AggregateType,
            processed.AggregateId,
            processed.ResultingVersion,
            processed.ProcessedAtUtc);
    }
}