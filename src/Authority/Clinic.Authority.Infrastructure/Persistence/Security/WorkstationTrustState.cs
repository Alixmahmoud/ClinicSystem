namespace Clinic.Authority.Infrastructure.Persistence.Security;

/// <summary>
/// Workstation trust lifecycle subset used by the slice: Unenrolled → Authorized →
/// Trusted / Active → Suspended / Revoked. Exact enrollment is an open detail.
/// </summary>
public enum WorkstationTrustState
{
    Unenrolled = 0,
    Authorized = 1,
    Trusted = 2,
    Suspended = 3,
    Revoked = 4
}