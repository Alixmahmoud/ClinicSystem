namespace Clinic.Authority.Domain.Security;

/// <summary>
/// LAN presence is not proof of trust; workstation enrollment/trust is a security boundary.
/// The workstation trust lifecycle is Unenrolled → Enrollment Requested → Authorized →
/// Trusted / Active → Suspended / Revoked. The exact enrollment mechanism is an open detail.
/// </summary>
public interface IWorkstationTrustStore
{
    Task<bool> IsTrustedAsync(Guid workstationId, CancellationToken cancellationToken = default);
}