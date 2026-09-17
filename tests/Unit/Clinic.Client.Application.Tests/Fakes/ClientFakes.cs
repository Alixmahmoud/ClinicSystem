using Clinic.Client.Domain.Abstractions;
using Clinic.Client.Domain.Appointments;
using Clinic.Client.Domain.Queue;
using Clinic.Client.Sync;

namespace Clinic.Client.Application.Tests.Fakes;

internal sealed class FakeAppointmentRepository : IAppointmentRepository
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

internal sealed class FakePatientFlowRepository : IPatientFlowRepository
{
    public List<PatientFlowEntry> Items { get; } = new();

    public Task AddAsync(PatientFlowEntry entry, CancellationToken cancellationToken = default)
    {
        Items.Add(entry);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsForAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(e => e.AppointmentId == appointmentId));
}

internal sealed class FakeSyncOperationRepository : ISyncOperationRepository
{
    public List<SyncOperation> Items { get; } = new();

    public Task<IReadOnlyList<SyncOperation>> GetProcessableAsync(int limit, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SyncOperation>>(
            Items.Where(o => o.IsRetryable).OrderBy(o => o.CreatedAtUtc).Take(limit).ToArray());

    public Task<SyncOperation?> GetPendingForAggregateAsync(
        string aggregateType, Guid aggregateId, string commandType, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(o =>
            o.IsRetryable
            && o.AggregateType == aggregateType
            && o.AggregateId == aggregateId
            && o.CommandType == commandType));

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

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}