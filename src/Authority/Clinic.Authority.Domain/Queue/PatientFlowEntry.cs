namespace Clinic.Authority.Domain.Queue;

/// <summary>
/// Frozen: Patient Flow State is a distinct concept from Appointment Status and from
/// Visit State. This slice establishes a doctor-specific patient-flow/queue entry when
/// an appointment is checked in. The queue position is derived from entry order, not
/// stored as mutable ordering state here.
/// </summary>
public sealed class PatientFlowEntry
{
    private PatientFlowEntry()
    {
    }

    public PatientFlowEntry(
        Guid id,
        Guid appointmentId,
        Guid patientId,
        Guid doctorId,
        DateTimeOffset queuedAtUtc)
    {
        if (id == Guid.Empty) throw new ArgumentException("Entry id is required.", nameof(id));
        if (appointmentId == Guid.Empty) throw new ArgumentException("Appointment id is required.", nameof(appointmentId));
        if (patientId == Guid.Empty) throw new ArgumentException("Patient id is required.", nameof(patientId));
        if (doctorId == Guid.Empty) throw new ArgumentException("Doctor id is required.", nameof(doctorId));

        Id = id;
        AppointmentId = appointmentId;
        PatientId = patientId;
        DoctorId = doctorId;
        QueuedAtUtc = queuedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid AppointmentId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateTimeOffset QueuedAtUtc { get; private set; }
}