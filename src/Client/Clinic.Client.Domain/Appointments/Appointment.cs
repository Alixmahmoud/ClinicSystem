using Clinic.Client.Domain.Common;

namespace Clinic.Client.Domain.Appointments;

/// <summary>
/// Workstation-local projection of the Appointment aggregate. Local-first: the workstation
/// applies the business change immediately and records a durable sync intent. The local
/// projection is eventually reconcilable with the Authority-confirmed version.
/// </summary>
public sealed class Appointment
{
    private Appointment()
    {
    }

    public Appointment(
        Guid id,
        Guid patientId,
        Guid doctorId,
        DateTimeOffset scheduledAtUtc,
        AppointmentStatus status,
        long version,
        long? confirmedVersion)
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
        ConfirmedVersion = confirmedVersion;
    }

    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateTimeOffset ScheduledAtUtc { get; private set; }
    public AppointmentStatus Status { get; private set; }

    /// <summary>Local optimistic-concurrency version (increments on every local transition).</summary>
    public long Version { get; private set; }

    /// <summary>Highest version confirmed by the Clinic Authority for this appointment, if any.</summary>
    public long? ConfirmedVersion { get; private set; }

    /// <summary>
    /// Check the patient in locally. Only a Scheduled appointment can be checked in.
    /// The queue/patient-flow entry is established by the orchestrator (separate aggregate)
    /// after this transition succeeds, and the durable sync intent is created atomically.
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
    }

    /// <summary>Reconcile the local projection with an Authority-confirmed version.</summary>
    public void ConfirmVersion(long authoritativeVersion)
    {
        if (authoritativeVersion <= 0) throw new ArgumentOutOfRangeException(nameof(authoritativeVersion));
        ConfirmedVersion = authoritativeVersion;
        if (authoritativeVersion > Version)
        {
            // The Authority reserved a higher version (e.g. a concurrent transition won).
            // Keep the local projection consistent with the authoritative ordering.
            Version = authoritativeVersion;
        }
    }
}