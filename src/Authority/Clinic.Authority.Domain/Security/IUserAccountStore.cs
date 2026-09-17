namespace Clinic.Authority.Domain.Security;

/// <summary>
/// Human identity is distinct from workstation identity, and authentication is distinct
/// from authorization. The exact authentication mechanism is an open detail; this store
/// implements a fail-closed default-deny boundary for the slice.
/// </summary>
public interface IUserAccountStore
{
    Task<UserAccount?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed record UserAccount(Guid Id, UserRole Role, string DisplayName);