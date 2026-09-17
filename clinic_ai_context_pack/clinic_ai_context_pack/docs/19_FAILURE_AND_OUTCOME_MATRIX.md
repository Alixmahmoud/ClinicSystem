# Failure and Outcome Semantics

## Business Outcomes
- Accepted
- Already Processed
- Conflict
- Rejected
- Dependency Blocked
- Temporarily Unavailable

## Failure Categories
Keep these distinct:
- Authentication failure
- Authorization failure
- Validation failure
- Malformed/unsupported operation or protocol
- Network failure
- Business rejection
- Conflict
- Temporary Authority unavailability

## Critical Distinctions
- Network failure ≠ business rejection
- Network failure ≠ conflict
- Network failure ≠ temporary Authority unavailability
- Conflict ≠ authentication failure
- Conflict ≠ authorization denial
- Temporary Authority unavailability ≠ authorization denial
- Transport status ≠ business outcome

## Unknown Outcome
When communication outcome is unknown, recover using the existing OperationId. Never generate a new OperationId merely because a communication attempt timed out or the response was lost.

## Source
Master handoff pages 9–10.
