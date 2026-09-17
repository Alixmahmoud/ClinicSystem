using Clinic.Authority.Domain.Common;

namespace Clinic.Authority.Domain.Appointments;

public sealed record PatientCheckedInEvent(Guid AppointmentId, Guid DoctorId, Guid PatientId, DateTimeOffset OccurredAtUtc)
    : IDomainEvent
{
    public Guid AggregateId => AppointmentId;
}