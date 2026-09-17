namespace Clinic.Client.Sync;

/// <summary>
/// Frozen: the durable sync intent lifecycle. "Created → Queued → Submitting → outcome"
/// is preserved; any submission whose authoritative reply was lost is recovered by exact
/// OperationId lookup, never by blind replay and never by regenerating the OperationId.
/// Terminal states are never retried.
/// </summary>
public enum SyncOperationState
{
    Queued = 1,
    Submitting = 2,
    Completed = 3,
    Rejected = 4,
    Conflict = 5,
    TemporarilyUnavailable = 6,
    UnknownOutcome = 7
}