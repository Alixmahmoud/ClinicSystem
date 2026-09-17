# First Vertical Slice — Implementation Boundary

## Status
COMPLETE / IMPLEMENTED + TESTED + CHECKPOINTED + PUSHED.

This boundary document preserves the original plan. The actual execution record
(scope, decisions, tests, deviations, git reference) is in
`../20_IMPLEMENTATION_STATE.md`; a completion summary is also in
`../13_FIRST_VERTICAL_SLICE.md`.

## Purpose
The First Vertical Slice is not another architecture phase. It is the first practical implementation milestone whose purpose is to prove that the already-specified system works as one coherent path.

## Required End-to-End Path
```text
WPF
→ Application / Use-Case Boundary
→ Domain Operation
→ Local SQLite
→ Sync Operation
→ Clinic Authority
→ PostgreSQL
→ Authority Result
→ Local Sync State
```

## Cross-Cutting Foundations the Slice Must Exercise
- individual identity
- workstation identity
- authentication
- authorization
- business command
- validation
- aggregate ownership
- domain invariant enforcement
- transaction boundary
- local persistence
- durable pending synchronization operation
- Authority processing
- idempotency
- optimistic concurrency
- audit
- synchronization result handling
- semantic failure handling
- local authoritative-result/state update

## Completion Gate
The selected slice must demonstrate:
- an actual WPF-to-local-to-sync-to-Authority-to-PostgreSQL-to-result path;
- the same OperationId surviving local queue, submission and unknown-outcome recovery;
- Authority remaining the final shared-state security/business boundary;
- local persistence and pending sync intent surviving restart;
- major semantic failure categories remaining distinguishable.

## Business Workflow Selection Rule
The current handoff intentionally does NOT choose a specific business command. The first slice must be selected from authoritative Requirements / API / Domain material and current V1 priorities. Do not choose a workflow purely by implementation convenience.

## Implementation Governance
The first implementation is not permission to redesign frozen architecture/data/API/sync/security decisions.

Any defect must first be classified as:
- implementation defect;
- specification gap;
- contradiction;
- true upstream decision issue.

Only then may upstream frozen content be considered for change through an explicit phase-gate process.

## Source
Master handoff pages 15–18.
