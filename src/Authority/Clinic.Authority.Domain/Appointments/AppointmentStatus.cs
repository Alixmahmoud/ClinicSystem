namespace Clinic.Authority.Domain.Appointments;

/// <summary>
/// Frozen: Appointment Status is a distinct concept from Patient Flow State and from
/// Visit State. The lifecycle preserves Scheduled, Checked In, Completed, Cancelled,
/// Potential No-Show / Needs Review, and No-Show semantics.
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