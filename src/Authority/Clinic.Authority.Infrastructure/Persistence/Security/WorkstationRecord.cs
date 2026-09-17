using Clinic.Authority.Domain.Security;

namespace Clinic.Authority.Infrastructure.Persistence.Security;

/// <summary>
/// Store projection of a workstation for the trust boundary. The exact enrollment and
/// certificate mechanics are open security details; the trust state is the minimal
/// durable boundary.
/// </summary>
public sealed class WorkstationRecord
{
    private WorkstationRecord()
    {
    }

    public WorkstationRecord(Guid id, string displayName, WorkstationTrustState trustState)
    {
        Id = id;
        DisplayName = displayName;
        TrustState = trustState;
    }

    public Guid Id { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public WorkstationTrustState TrustState { get; private set; }
}