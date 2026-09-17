namespace Clinic.Authority.Domain.Security;

/// <summary>
/// Minimal role set for the slice. The exact role-permission matrix is an explicitly open
/// security detail; these values implement a fail-closed default-deny base with the
/// frozen RBAC model. Not elevated to a frozen requirement without a phase-gate decision.
/// </summary>
public enum UserRole
{
    Administrator = 1,
    Reception = 2,
    Doctor = 3,
    Cashier = 4
}