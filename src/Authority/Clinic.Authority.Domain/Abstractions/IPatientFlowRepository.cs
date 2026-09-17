using Clinic.Authority.Domain.Queue;

namespace Clinic.Authority.Domain.Abstractions;

public interface IPatientFlowRepository
{
    Task AddAsync(PatientFlowEntry entry, CancellationToken cancellationToken = default);
}