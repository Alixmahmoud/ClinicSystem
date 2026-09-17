namespace Clinic.Client.Domain.Appointments;

/// <summary>
/// Frozen: Appointment Status is a distinct concept from Patient Flow State and from
/// Visit State. The workstation local projection mirrors the Authority lifecycle.
/// </summary>
public enum AppointmentStatus
{
    Scheduled = 1,
    CheckedIn = 2,
    Completed = 3,
    Cancelled = 4,
    PotentialNoShow = 5,
    NoShow = 6
}