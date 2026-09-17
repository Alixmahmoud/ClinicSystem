using Clinic.Client.Domain.Abstractions;
using Clinic.Client.Domain.Appointments;
using Clinic.Client.Domain.Queue;
using Clinic.Client.Infrastructure.Persistence;
using Clinic.Client.Sync;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Client.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository(WorkstationDbContext db) : IAppointmentRepository
{
    public Task<Appointment?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Appointments.FindAsync([id], cancellationToken).AsTask();

    public async Task<IReadOnlyList<Appointment>> ListAsync(CancellationToken cancellationToken = default) =>
        await db.Appointments.AsNoTracking()
            .OrderBy(a => a.ScheduledAtUtc)
            .ThenBy(a => a.Id)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default) =>
        db.Appointments.AddAsync(appointment, cancellationToken).AsTask();

    public Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        // The aggregate is tracked by the shared context; the unit of work commits it.
        return Task.CompletedTask;
    }
}

public sealed class PatientFlowRepository(WorkstationDbContext db) : IPatientFlowRepository
{
    public Task AddAsync(PatientFlowEntry entry, CancellationToken cancellationToken = default) =>
        db.PatientFlowEntries.AddAsync(entry, cancellationToken).AsTask();

    public Task<bool> ExistsForAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default) =>
        db.PatientFlowEntries.AsNoTracking().AnyAsync(e => e.AppointmentId == appointmentId, cancellationToken);
}

public sealed class WorkstationUnitOfWork(WorkstationDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}

public sealed class SyncOperationRepository(WorkstationDbContext db) : ISyncOperationRepository
{
    private static readonly SyncOperationState[] RetryableStates =
    {
        SyncOperationState.Queued,
        SyncOperationState.Submitting,
        SyncOperationState.TemporarilyUnavailable,
        SyncOperationState.UnknownOutcome
    };

    public async Task<IReadOnlyList<SyncOperation>> GetProcessableAsync(int limit, CancellationToken cancellationToken = default) =>
        await db.SyncOperations
            .Where(o => RetryableStates.Contains(o.State))
            .OrderBy(o => o.CreatedAtUtc)
            .ThenBy(o => o.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task<SyncOperation?> GetPendingForAggregateAsync(
        string aggregateType,
        Guid aggregateId,
        string commandType,
        CancellationToken cancellationToken = default) =>
        await db.SyncOperations
            .Where(o => RetryableStates.Contains(o.State)
                        && o.AggregateType == aggregateType
                        && o.AggregateId == aggregateId
                        && o.CommandType == commandType)
            .OrderBy(o => o.CreatedAtUtc)
            .ThenBy(o => o.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<SyncOperation>> ListPendingAsync(CancellationToken cancellationToken = default) =>
        await db.SyncOperations
            .Where(o => RetryableStates.Contains(o.State))
            .OrderBy(o => o.CreatedAtUtc)
            .ThenBy(o => o.Id)
            .ToListAsync(cancellationToken);

    public Task AddAsync(SyncOperation operation, CancellationToken cancellationToken = default) =>
        db.SyncOperations.AddAsync(operation, cancellationToken).AsTask();

    public Task UpdateAsync(SyncOperation operation, CancellationToken cancellationToken = default)
    {
        // Tracked by the shared context; the unit of work commits it atomically.
        return Task.CompletedTask;
    }
}