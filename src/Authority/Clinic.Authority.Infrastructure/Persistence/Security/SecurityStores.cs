using Clinic.Authority.Domain.Security;
using Clinic.Authority.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Authority.Infrastructure.Persistence.Security;

public sealed class UserAccountStore(AuthorityDbContext db) : IUserAccountStore
{
    public async Task<UserAccount?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var record = await db.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        return ToAccount(record);
    }

    public async Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var record = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return ToAccount(record);
    }

    private static UserAccount? ToAccount(UserAccountRecord? record)
    {
        // Fail closed: inactive accounts cannot authenticate.
        if (record is null || !record.IsActive)
        {
            return null;
        }

        return record.ToUserAccount();
    }
}

public sealed class WorkstationTrustStore(AuthorityDbContext db) : IWorkstationTrustStore
{
    public async Task<bool> IsTrustedAsync(Guid workstationId, CancellationToken cancellationToken = default)
    {
        var workstation = await db.Workstations.FirstOrDefaultAsync(w => w.Id == workstationId, cancellationToken);
        if (workstation is null)
        {
            return false; // fail closed: unknown workstation is not trusted
        }

        return workstation.TrustState == WorkstationTrustState.Trusted
               || workstation.TrustState == WorkstationTrustState.Authorized;
    }
}

public sealed class AuthorizationService(AuthorityDbContext db) : IAuthorizationService
{
    /// <summary>
    /// Fail-closed RBAC default for the slice: the CheckInAppointment Edit capability is
    /// granted to Reception and Administrator. View and Edit remain distinct capabilities.
    /// The exact role-permission matrix remains an open security detail.
    /// </summary>
    public async Task<bool> CanCheckInAppointmentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return false;
        }

        return user.Role is UserRole.Reception or UserRole.Administrator;
    }
}