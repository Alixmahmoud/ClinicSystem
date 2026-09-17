namespace Clinic.Client.Application.CheckIn;

/// <summary>
/// Local-first check-in outcomes kept distinct. These are NOT Authority outcomes and
/// never replace them; they describe the workstation-side result and sync intent state.
/// </summary>
public enum CheckInAppointmentLocalOutcome
{
    CheckedInAndQueuedForSync = 1,
    AlreadyCheckedIn = 2,
    AppointmentNotFound = 3,
    InvalidTransition = 4
}

public sealed record CheckInAppointmentResult(
    CheckInAppointmentLocalOutcome Outcome,
    Guid AppointmentId,
    Guid OperationId,
    long LocalVersion,
    bool HasPendingSync,
    string? Message);