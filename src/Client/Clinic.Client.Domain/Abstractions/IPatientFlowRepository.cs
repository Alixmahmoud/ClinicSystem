using Clinic.Client.Domain.Queue;

namespace Clinic.Client.Domain.Abstractions;

public interface IPatientFlowRepository
{
    Task AddAsync(PatientFlowEntry entry, CancellationToken cancellationToken = default);
    Task<bool> ExistsForAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}