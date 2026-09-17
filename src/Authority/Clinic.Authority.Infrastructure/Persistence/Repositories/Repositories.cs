using Clinic.Authority.Domain.Abstractions;
using Clinic.Authority.Domain.Appointments;
using Clinic.Authority.Domain.Audit;
using Clinic.Authority.Domain.Queue;
using Clinic.Authority.Domain.Sync;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Authority.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository(AuthorityDbContext db) : IAppointmentRepository
{
    public Task<Appointment?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        db.Appointments.Add(appointment);
        return Task.CompletedTask;
    }
}

public sealed class PatientFlowRepository(AuthorityDbContext db) : IPatientFlowRepository
{
    public Task AddAsync(PatientFlowEntry entry, CancellationToken cancellationToken = default)
    {
        db.PatientFlowEntries.Add(entry);
        return Task.CompletedTask;
    }
}

public sealed class ProcessedOperationRepository(AuthorityDbContext db) : IProcessedOperationRepository
{
    public Task<ProcessedOperation?> GetAsync(
        Guid workstationId,
        Guid operationId,
        CancellationToken cancellationToken = default) =>
        db.ProcessedOperations.FirstOrDefaultAsync(
            p => p.WorkstationId == workstationId && p.OperationId == operationId,
            cancellationToken);

    public Task AddAsync(ProcessedOperation processedOperation, CancellationToken cancellationToken = default)
    {
        db.ProcessedOperations.Add(processedOperation);
        return Task.CompletedTask;
    }
}

public sealed class AuditLogRepository(AuthorityDbContext db) : IAuditLogRepository
{
    public Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        db.AuditLogEntries.Add(entry);
        return Task.CompletedTask;
    }
}

public sealed class AuthoritativeChangeRepository(AuthorityDbContext db) : IAuthoritativeChangeRepository
{
    public Task AddAsync(AuthoritativeChange change, CancellationToken cancellationToken = default)
    {
        db.AuthoritativeChanges.Add(change);
        return Task.CompletedTask;
    }
}

public sealed class AuthorityUnitOfWork(AuthorityDbContext db) : IUnitOfWork
{
    /// <summary>
    /// All mutations staged on the shared context are committed atomically by the provider
    /// (single transaction), preserving the invariant that business change + idempotency
    /// record + audit + authoritative change either all persist or none do.
    /// </summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}