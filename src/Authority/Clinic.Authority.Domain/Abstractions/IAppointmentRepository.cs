using Clinic.Authority.Domain.Appointments;

namespace Clinic.Authority.Domain.Abstractions;

/// <summary>
/// Repository port for the authoritative Appointment aggregate. Persistence implementation
/// lives in Authority Infrastructure (PostgreSQL via EF Core) and must not leak entities into
/// the public API contracts.
/// </summary>
public interface IAppointmentRepository
{
    Task<Appointment?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
}