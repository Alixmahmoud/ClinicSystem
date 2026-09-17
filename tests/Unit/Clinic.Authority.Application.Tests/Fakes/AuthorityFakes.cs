using Clinic.Authority.Domain.Abstractions;
using Clinic.Authority.Domain.Appointments;
using Clinic.Authority.Domain.Audit;
using Clinic.Authority.Domain.Queue;
using Clinic.Authority.Domain.Security;
using Clinic.Authority.Domain.Sync;

namespace Clinic.Authority.Application.Tests.Fakes;

internal sealed class InMemoryUserAccountStore : IUserAccountStore
{
    public Dictionary<Guid, UserAccount> ById { get; } = new();

    public Task<UserAccount?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        Task.FromResult(ById.Values.FirstOrDefault(a => a.DisplayName == username));

    public Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(ById.TryGetValue(userId, out var account) ? account : null);
}

internal sealed class InMemoryWorkstationTrustStore : IWorkstationTrustStore
{
    public HashSet<Guid> Trusted { get; } = new();

    public Task<bool> IsTrustedAsync(Guid workstationId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Trusted.Contains(workstationId));
}

internal sealed class StubAuthorizationService : IAuthorizationService
{
    public bool AllowCheckIn { get; set; } = true;

    public Task<bool> CanCheckInAppointmentAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(AllowCheckIn);
}

internal sealed class InMemoryAppointmentRepository : IAppointmentRepository
{
    public Dictionary<Guid, Appointment> Items { get; } = new();

    public Task<Appointment?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.TryGetValue(id, out var appointment) ? appointment : null);

    public Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        Items[appointment.Id] = appointment;
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryPatientFlowRepository : IPatientFlowRepository
{
    public List<PatientFlowEntry> Items { get; } = new();

    public Task AddAsync(PatientFlowEntry entry, CancellationToken cancellationToken = default)
    {
        Items.Add(entry);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryProcessedOperationRepository : IProcessedOperationRepository
{
    public Dictionary<(Guid WorkstationId, Guid OperationId), ProcessedOperation> Items { get; } = new();

    public Task<ProcessedOperation?> GetAsync(Guid workstationId, Guid operationId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.TryGetValue((workstationId, operationId), out var operation) ? operation : null);

    public Task AddAsync(ProcessedOperation processedOperation, CancellationToken cancellationToken = default)
    {
        Items[(processedOperation.WorkstationId, processedOperation.OperationId)] = processedOperation;
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryAuditLogRepository : IAuditLogRepository
{
    public List<AuditLogEntry> Items { get; } = new();

    public Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        Items.Add(entry);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryAuthoritativeChangeRepository : IAuthoritativeChangeRepository
{
    public List<AuthoritativeChange> Items { get; } = new();

    public Task AddAsync(AuthoritativeChange change, CancellationToken cancellationToken = default)
    {
        Items.Add(change);
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