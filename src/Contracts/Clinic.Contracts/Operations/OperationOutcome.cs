namespace Clinic.Contracts.Operations;

/// <summary>
/// Frozen semantic outcomes of Authority processing. These are business outcomes,
/// distinct from transport status and from non-business failure categories.
/// </summary>
public enum OperationOutcome
{
    Accepted = 1,
    AlreadyProcessed = 2,
    Conflict = 3,
    Rejected = 4,
    DependencyBlocked = 5,
    TemporarilyUnavailable = 6
}