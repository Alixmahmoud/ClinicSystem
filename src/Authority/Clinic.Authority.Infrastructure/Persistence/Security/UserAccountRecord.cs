using Clinic.Authority.Domain.Security;

namespace Clinic.Authority.Infrastructure.Persistence.Security;

/// <summary>
/// Store projection of a user account for authentication. This is not a public API contract
/// and not a domain aggregate.
/// </summary>
public sealed class UserAccountRecord
{
    private UserAccountRecord()
    {
    }

    public UserAccountRecord(Guid id, string username, string displayName, UserRole role, bool isActive)
    {
        Id = id;
        Username = username;
        DisplayName = displayName;
        Role = role;
        IsActive = isActive;
    }

    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    public UserAccount ToUserAccount() => new(Id, Role, DisplayName);
}