using Clinic.Authority.Domain.Common;

namespace Clinic.Authority.Domain.Appointments;

/// <summary>
/// Appointment aggregate root owned by the Clinic Authority.
/// Frozen rules for this slice:
///  - Scheduled → Checked In is valid.
///  - Checked In cannot become No-Show.
///  - Check-in derives doctor and queue automatically (queue = separate aggregate).
/// Appointment is NOT a Visit; Appointment Status is NOT Patient Flow State.
/// </summary>
public sealed class Appointment : IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    private Appointment()
    {
    }

    public Appointment(
        Guid id,
        Guid patientId,
        Guid doctorId,
        DateTimeOffset scheduledAtUtc,
        AppointmentStatus status,
        long version)
    {
        if (id == Guid.Empty) throw new ArgumentException("Appointment id is required.", nameof(id));
        if (patientId == Guid.Empty) throw new ArgumentException("Patient id is required.", nameof(patientId));
        if (doctorId == Guid.Empty) throw new ArgumentException("Doctor id is required.", nameof(doctorId));

        Id = id;
        PatientId = patientId;
        DoctorId = doctorId;
        ScheduledAtUtc = scheduledAtUtc;
        Status = status;
        Version = version;
    }

    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateTimeOffset ScheduledAtUtc { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public long Version { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// Check the patient in. Only a Scheduled appointment can be checked in.
    /// Checked In cannot become No-Show, and a Completed/Cancelled appointment is terminal
    /// for this path. The queue/patient-flow state is established by the orchestrator
    /// (separate aggregate) after this transition succeeds.
    /// </summary>
    public void CheckIn()
    {
        if (Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidTransitionException(
                AppointmentStatus.Scheduled,
                Status,
                "Only a Scheduled appointment can be checked in (Scheduled → Checked In is the only allowed check-in transition).");
        }

        Status = AppointmentStatus.CheckedIn;
        Version++;
        _domainEvents.Add(new PatientCheckedInEvent(Id, DoctorId, PatientId, DateTimeOffset.UtcNow));
    }

    internal void ConfirmVersion(long authoritativeVersion)
    {
        Version = authoritativeVersion;
    }
}