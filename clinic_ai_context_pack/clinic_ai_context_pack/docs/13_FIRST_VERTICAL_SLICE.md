# First Vertical Slice — Implementation Boundary

## Status
COMPLETE / IMPLEMENTED + TESTED + CHECKPOINTED + PUSHED.

The slice boundary below is the original plan (preserved). The actual execution record
is in `20_IMPLEMENTATION_STATE.md`; see the "Actual Implementation Result" section
at the end of this file for a summary.

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

### Completion Gate Result (VERIFIED)
All completion-gate items were verified and passed. See `20_IMPLEMENTATION_STATE.md`
(Tests / Verification / Review Result).

## Actual Implementation Result — Summary

- Business command: `CheckInAppointment` (Reception checks in a Scheduled appointment
  to CheckedIn; doctor-specific patient-flow/queue participation established).
- Full verified path: WPF → Client Application → Client Domain → SQLite → durable Sync
  Operation → Authority API/Application → Authority Domain → PostgreSQL → Authority
  Result → local sync state.
- Verification: build succeeds (clean rebuild surfaces one pre-existing nullable-annotation
  warning, CS8605, in test-only code `tests/Unit/Clinic.Client.Domain.Tests/AppointmentTests.cs`);
  53/53 tests across 10 projects (unit, integration on real PostgreSQL + SQLite, contract,
  4 end-to-end).
- Important implementation decisions (transport mapping, seed mechanism, composition
  root, package pins): see `20_IMPLEMENTATION_STATE.md` — all marked as implementation
  details, not frozen changes.
- Deviation from plan: none in scope. Repo restructuring (legacy `ClinicSystem/`
  relocated to `src/Client/Clinic.Client.Wpf/`, solution renamed) did not alter frozen
  boundaries.
- Git: FVS implementation commit `325c410`; current `main`/`origin/main` = `e8544d0`
  (pushed).
- Remaining: next bounded slice is NOT selected — selection is a separate governed,
  source-backed task.

## Business Workflow Selection Rule
The current handoff intentionally does NOT choose a specific business command. The first slice must be selected from authoritative Requirements / API / Domain material and current V1 priorities. Do not choose a workflow purely by implementation convenience.

### Actual Selection
`CheckInAppointment` (Appointment / Scheduling catalog, `18_COMMAND_CATALOG.md`),
selected as a source-backed bounded workflow for this proof path.

## Implementation Governance
The first implementation is not permission to redesign frozen architecture/data/API/sync/security decisions.

Any defect must first be classified as:
- implementation defect;
- specification gap;
- contradiction;
- true upstream decision issue.

Only then may upstream frozen content be considered for change through an explicit phase-gate process.

## Source
Master handoff pages 15–18; execution record `20_IMPLEMENTATION_STATE.md`.
