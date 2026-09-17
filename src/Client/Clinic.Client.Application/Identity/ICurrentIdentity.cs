namespace Clinic.Client.Application.Identity;

/// <summary>
/// The "who" and "where" of a workstation user action. The workstation trusts its local
/// operator for enabling the action, while the Clinic Authority remains authoritative for
/// authentication and authorization of the submitted operation.
/// </summary>
public interface ICurrentIdentity
{
    Guid UserId { get; }
    string UserName { get; }
    Guid WorkstationId { get; }
    string WorkstationName { get; }
}

public sealed class CurrentIdentity : ICurrentIdentity
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid WorkstationId { get; set; }
    public string WorkstationName { get; set; } = string.Empty;
}