namespace Clinic.Authority.Domain.Security;

/// <summary>
/// Authority-side authorization is authoritative. View and Edit are distinct capabilities;
/// responsibility is not authorization. The exact permission matrix is an open detail;
/// this is the fail-closed default for the check-in Edit capability in the slice.
/// </summary>
public interface IAuthorizationService
{
    Task<bool> CanCheckInAppointmentAsync(Guid userId, CancellationToken cancellationToken = default);
}