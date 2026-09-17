using Clinic.Client.Domain.Appointments;

namespace Clinic.Client.Domain.Abstractions;

/// <summary>
/// Repository port for the workstation-local Appointment projection.
/// Implementation lives in Client Infrastructure (SQLite via EF Core).
/// </summary>
public interface IAppointmentRepository
{
    Task<Appointment?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
}