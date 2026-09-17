# Synchronization Baseline — Semantic Freeze

## Status
Synchronization is COMPLETE / SEMANTIC BASELINE FROZEN.

## Operation Envelope
Every synchronized business operation conceptually carries:
- OperationId
- OriginWorkstationId
- UserId
- AggregateType
- AggregateId
- CommandType
- BaseVersion
- CommandPayload
- CreatedAt
- ProtocolVersion / sync metadata

## Lifecycle
`Created → Queued → Ready → Submitting → definitive result`

Unknown outcome path:
`Unknown Outcome → OperationId Result Recovery`

## Core Invariants
1. One OperationId can create at most one authoritative business effect.
2. Where a local business change is synchronization-bound, business change + pending synchronization intent are atomic and durable.
3. Unknown outcomes are recoverable via OperationId lookup.
4. Authority is the shared-truth owner.
5. Optimistic concurrency uses Version + BaseVersion + OperationId.
6. Conflict is explicit and domain-aware; no universal LWW or generic merge/rebase.
7. Dependencies are respected; blocked operations wait for prerequisites.
8. Incoming authoritative change + cursor advancement are atomic.
9. Initial sync uses authoritative snapshot then incremental change feed.
10. Expired cursor recovery uses full authoritative snapshot refresh, then incremental continuation.
11. Offline capability is operation-specific and bounded.
12. Restore is synchronization-aware and must not duplicate effects or corrupt sync cursors.

## Authority Processing Pipeline
`Receive → Validate Envelope → Authenticate → Authorize → Check Idempotency → Validate Dependencies → Validate BaseVersion / Concurrency → Execute Domain Operation → Persist Transaction → Create Audit → Publish Authoritative Change → Return Result`

## Semantic Outcomes
- Accepted
- Already Processed
- Conflict
- Rejected
- Dependency Blocked
- Temporarily Unavailable

## Failure Distinctions
Do not collapse:
- network failure
- business rejection
- conflict
- temporary Authority unavailability
- authentication failure
- authorization failure
- validation failure
- malformed/unsupported operation or protocol failure

## Explicitly Rejected Synchronization Designs
- raw database replication
- workstation PostgreSQL direct access
- universal Last-Write-Wins
- blind replay
- generic merge/rebase
- OperationId regeneration during retry
- one generic SyncError collapsing semantic outcomes
- unlimited offline Authority
- advancing cursor without corresponding durable state application
- generic CRUD synchronization bypassing business operations
- technical sync path bypassing Authority authz/audit/domain rules

## Open Synchronization Details
- exact change-feed implementation
- exact snapshot consistency mechanism
- exact retry/backoff numbers
- exact conflict-retention implementation
- exact offline authorization capability/grace mechanics
- exact post-restore synchronization recovery algorithm
- exact monitoring/diagnostic tooling
- remaining wire-level sync details

## Source
Master handoff pages 8–10 and 16.
