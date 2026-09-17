namespace Clinic.Contracts.Operations;

/// <summary>
/// Frozen failure categories kept distinct. These are NOT business outcomes and must
/// never be collapsed into a single generic error. Corresponds to the frozen
/// Failure and Outcome Matrix. "TemporarilyUnavailable" also exists as a business outcome.
/// </summary>
public enum OperationFailureCategory
{
    Authentication = 1,
    Authorization = 2,
    Validation = 3,
    MalformedProtocol = 4,
    Network = 5,
    TemporarilyUnavailable = 6
}