using Clinic.Client.Domain.Abstractions;
using Clinic.Client.Domain.Appointments;
using Clinic.Client.Sync;
using Clinic.Contracts.Operations;

namespace Clinic.Sync.Integration.Tests.Fakes;

internal sealed class InMemorySyncOperationRepository : ISyncOperationRepository
{
    public List<SyncOperation> Items { get; } = new();

    public Task<IReadOnlyList<SyncOperation>> GetProcessableAsync(int limit, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SyncOperation>>(
            Items.Where(o => o.IsRetryable).OrderBy(o => o.CreatedAtUtc).Take(limit).ToArray());

    public Task<SyncOperation?> GetPendingForAggregateAsync(
        string aggregateType, Guid aggregateId, string commandType, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(o =>
            o.IsRetryable && o.AggregateType == aggregateType && o.AggregateId == aggregateId && o.CommandType == commandType));

    public Task<IReadOnlyList<SyncOperation>> ListPendingAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SyncOperation>>(Items.Where(o => o.IsRetryable).ToArray());

    public Task AddAsync(SyncOperation operation, CancellationToken cancellationToken = default)
    {
        Items.Add(operation);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(SyncOperation operation, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

internal sealed class InMemoryAppointmentRepository : IAppointmentRepository
{
    public Dictionary<Guid, Appointment> Items { get; } = new();

    public Task<Appointment?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.TryGetValue(id, out var appointment) ? appointment : null);

    public Task<IReadOnlyList<Appointment>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Appointment>>(Items.Values.ToArray());

    public Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        Items[appointment.Id] = appointment;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        Items[appointment.Id] = appointment;
        return Task.CompletedTask;
    }
}

internal sealed class CountingUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}

internal sealed class ScriptedSyncTransport : ISyncTransport
{
    public List<OperationEnvelope> SubmittedEnvelopes { get; } = new();
    public List<(Guid WorkstationId, Guid OperationId)> RecoveryLookups { get; } = new();

    public Func<OperationEnvelope, SyncSubmitTransportResult> SubmitHandler { get; set; } =
        _ => SyncTransportResults.TransientFailure();

    public Func<Guid, Guid, SyncSubmitTransportResult> RecoveryHandler { get; set; } =
        (_, _) => SyncTransportResults.TransientFailure();

    public Task<SyncSubmitTransportResult> SubmitAsync(OperationEnvelope envelope, CancellationToken cancellationToken = default)
    {
        SubmittedEnvelopes.Add(envelope);
        return Task.FromResult(SubmitHandler(envelope));
    }

    public Task<SyncSubmitTransportResult> GetResultAsync(Guid workstationId, Guid operationId, CancellationToken cancellationToken = default)
    {
        RecoveryLookups.Add((workstationId, operationId));
        return Task.FromResult(RecoveryHandler(workstationId, operationId));
    }
}