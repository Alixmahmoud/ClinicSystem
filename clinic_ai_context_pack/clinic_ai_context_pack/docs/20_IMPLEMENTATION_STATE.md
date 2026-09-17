# Implementation State

This file is the current implementation-state pointer. It records what was actually
implemented against the frozen specifications, how it was verified, and what remains.
It does not reopen or redesign any frozen phase content.

## Current Project Phase

| Phase | Status |
|---|---|
| Discovery | COMPLETE |
| Requirements | COMPLETE / FROZEN |
| Product Definition | COMPLETE |
| UX | COMPLETE |
| Architecture | COMPLETE / FROZEN |
| Data Specification | COMPLETE / FROZEN |
| API / Contract | COMPLETE / FROZEN (semantic) |
| Synchronization | COMPLETE / SEMANTIC BASELINE FROZEN |
| Security | COMPLETE / FROZEN |
| Deployment | COMPLETE / FROZEN |
| Repository / Solution Structure | COMPLETE / IMPLEMENTATION-READY |
| **First Vertical Slice** | **COMPLETE / IMPLEMENTED + TESTED + CHECKPOINTED + PUSHED** |
| **Incremental Workflow Implementation** | **NEXT / SOURCE-BACKED SELECTION REQUIRED** |
| Integration + Failure Testing | AFTER MAJOR WORKFLOW IMPLEMENTATION |
| Pilot / Release Hardening | FINAL PRE-RELEASE STAGE |

## First Vertical Slice — Actual Scope

- Business command: `CheckInAppointment`.
- Workflow exercised: Reception checks in an existing Scheduled appointment → the
  appointment becomes Checked In (CheckedIn) → doctor-specific patient-flow/queue
  participation is established.
- The slice was selected from the authoritative command catalog
  (`18_COMMAND_CATALOG.md`, Appointment / Scheduling) and the open business-workflow
  selection rule in `13_FIRST_VERTICAL_SLICE.md`.

## Actual Implementation Path (CONFIRMED IMPLEMENTED)

```text
WPF (Clinic.Client.Wpf, MainViewModel)
→ Client Application / Use-Case Boundary (CheckInAppointmentUseCase)
→ Client Domain (Appointment.CheckIn, Version++ / ConfirmVersion)
→ Local SQLite (Clinic.Client.Infrastructure, WorkstationDbContext)
→ Durable Sync Operation (Clinic.Client.Sync, SyncOperation + SyncEngine)
→ Authority API / Application (POST /api/operations/submit → CheckInAppointmentProcessor)
→ Authority Domain (Appointment.CheckIn, PatientCheckedInEvent)
→ PostgreSQL (clinic_authority: appointments, patient_flow_entries, processed_operations, audit_log_entries, authoritative_changes)
→ Authority Result (definitive OperationResult / recovery via GET /api/operations/{workstationId}/{operationId})
→ Local Sync State (MarkCompleted / MarkRejected / MarkConflict / MarkUnknownOutcome + appointment.ConfirmVersion)
```

All layers exist in `src/` and are exercised by the test suite. There is no direct
workstation → PostgreSQL access; Authority remains the final authority side boundary.

## Cross-Cutting Foundations (CONFIRMED IMPLEMENTED)

- individual identity: seeded reception user, carried in the operation envelope.
- workstation identity: seeded workstation, carried as `OriginWorkstationId`.
- authentication: envelope identity resolved to a user account Authority-side
  (placeholder mechanism, no session/credentials yet — see Known Limitations).
- authorization: RBAC role check for the command Authority-side (fail-closed default).
- business command: `CheckInAppointment` codec at Contracts/Client Application/Authority.
- validation: envelope/command validation before domain execution.
- aggregate ownership: `Appointment` aggregate checked in by its own aggregate method.
- domain invariants: Scheduled → CheckedIn single valid transition; Version increment.
- local transaction/persistence: local change + pending sync intent in one
  `SaveChangesAsync` (atomic).
- durable pending synchronization operation: `SyncOperation` row keyed by OperationId.
- Authority processing: pipeline in `CheckInAppointmentProcessor`
  (Receive → Validate Envelope → Decode Command → Payload/AggregateId match →
  BaseVersion present → Authenticate → Workstation trust (fail closed) → Authorize →
  Check Idempotency → Dependencies → Concurrency → Execute Domain → Persist →
  Audit → Publish Change → Return Result).
- idempotency: `ProcessedOperation` row written in the same single `SaveChangesAsync`
  as the business effect, audit, and authoritative-change outbox row.
- optimistic concurrency: `Version` / `BaseVersion` check with explicit Conflict.
- audit: `AuditLogEntry` written transactionally.
- semantic failure handling: 6 business outcomes and 5 failure categories remain
  distinct at the contract boundary (`19_FAILURE_AND_OUTCOME_MATRIX.md`).
- authoritative result handling: local state updates only from a definitive
  Authority `OperationResult` (Accepted/AlreadyProcessed/Conflict etc. or a terminal
  failure-category rejection).
- restart/recovery behavior: unknown outcomes recover by GET using the SAME
  OperationId (no blind replay, no OperationId regeneration); 404 is a definitive
  "no record" → safe re-submit of the same OperationId.

## Implementation Decisions Made During the Slice

These are IMPLEMENTATION DETAIL / ASSUMPTION-level resolutions consistent with the
frozen semantic baselines. They were deliberately left open by the frozen specs:

- Transport mapping (open frozen detail): POST `/api/operations/submit` returns
  HTTP 200 for all 6 business outcomes (definitive business answers, body = `OperationResult`
  with `Outcome`). HTTP 401/403/422/400/502/503 map to
  Authentication/Authorization/Validation/MalformedProtocol/Network/TemporarilyUnavailable
  `OperationResult` bodies. Recovery GET returns 200 + result, or 404 (definitive
  "no record"). Other 5xx/network timeout → UnknownOutcome (retryable via OperationId).
- Seed/bootstrap is NOT a CreateAppointment bypass: the single source is
  `Clinic.Contracts.Development.DevelopmentSeed` (seeded reception user, workstation,
  doctor, patient, appointment + `reception`/`FRONT-DESK-1`), re-exported by
  `AuthoritySeedConstants`.
- Client uses a hand-rolled composition root (no DI package). Authority base URL
  overridable via `CLINIC_AUTHORITY_BASE_URL`; SQLite connection via
  `CLINIC_WORKSTATION_CONNECTION`.
- Package/EF version graph unified (EF packages 10.0.4 family, Npgsql provider 10.0.3,
  SQLitePCLRaw.bundle_e_sqlite3 2.1.13) via `Directory.Packages.props` (CPM).
- `dotnet-ef` repo-local tool manifest is at the repo root (`dotnet-tools.json`).

## Tests Performed (VERIFIED)

- `dotnet build ClinicManagementSystem.sln`: build succeeds. NOTE: a clean rebuild
  surfaces one pre-existing nullable-annotation warning (CS8605) in test-only code
  (`tests/Unit/Clinic.Client.Domain.Tests/AppointmentTests.cs` line 39). It predates
  this documentation, is treated as a finding (see Known Limitations / Technical Debt),
  and was not modified as part of this context update.
- `dotnet test`: 53/53 passed across 10 projects:
  - Unit: Client.Domain (4), Authority.Domain (3), Client.Application (4), Authority.Application (9)
  - Integration: Client.Infrastructure/SQLite (5), Authority.Infrastructure/PostgreSQL (3),
    Persistence.Integration (3, incl. rollback-on-duplicate-OperationId atomicity proof),
    Sync.Integration (7)
  - Contract: Contracts (11)
  - EndToEnd: 4 (full path; resubmit idempotency; authority-unavailable→recovery;
    unknown-outcome recovery without duplicate effect)
- Integration tests run against real PostgreSQL (18, local) and real SQLite.
- E2E tests run the Authority API in-process with real PostgreSQL and real SQLite
  (test-created databases).

## Verification / Review Result (VERIFIED)

- Completion gate of `13_FIRST_VERTICAL_SLICE.md` satisfied:
  - real WPF→local→sync→Authority→PostgreSQL→result path exists and is exercised;
  - the same OperationId survives local queue, submission and unknown-outcome recovery;
  - Authority remains the final shared-state security/business boundary;
  - local persistence and pending sync intent survive restart;
  - semantic failure categories remain distinguishable.

## Known Limitations / Technical Debt

- FINDING (pre-existing, not fixed in this task): `CS8605` nullable-annotation warning
  in `tests/Unit/Clinic.Client.Domain.Tests/AppointmentTests.cs` line 39 (unboxing
  `object?` state on `InvalidTransitionException`). Test-only; does not affect build
  success or test results. Reported separately; recommend fixing with the next
  maintenance touch of that file.
- KNOWN LIMITATION: Identity is a placeholder — fixed seeded reception user/workstation;
  no password/session/token mechanism, no step-up. Authorization checks still run.
- KNOWN LIMITATION: Workstation trust is determined from the seeded workstation record
  (fail-closed), not a certificate/enrollment flow.
- TECHNICAL DEBT: EF package versions are pinned to a unified graph for build health;
  re-evaluate on future package upgrades.
- TECHNICAL DEBT: `src/Authority/Clinic.Authority.Worker/` is an empty scaffold
  (csproj + refs only).
- KNOWN LIMITATION: Local workstations do not yet subscribe to the synchronized
  change-feed; the slice implements submit + OperationId recovery only.
- KNOWN LIMITATION: No SQLite-at-rest encryption in this slice.

## Open Items / Future Work

- OPEN ITEM: Next vertical slice is NOT selected. Selection must come from authoritative
  Requirements / API / Domain material and current V1 priorities, as a separate
  bounded, governed task (per `13_FIRST_VERTICAL_SLICE.md` selection rule).
- OPEN ITEM: Remaining unresolved items in `14_OPEN_ITEM_REGISTER.md` stay open until
  resolved at the appropriate boundary. Nothing here is a silent promotion to frozen.
- FUTURE WORK: real client authentication/session management, workstation enrollment,
  change-feed subscription (incremental + snapshot-continuation semantics), remaining
  V1 business commands, integration + failure testing phase, pilot/release hardening.

## Deviations from the Original Slice Plan

- None in scope. No architecture, requirements, data, API/contract semantics,
  synchronization semantics, security semantics, or V1 scope were changed by the slice.
- Repository restructuring (legacy `ClinicSystem/` project relocated to
  `src/Client/Clinic.Client.Wpf/`; solution renamed to `ClinicManagementSystem.sln`)
  was performed as part of the slice; frozen boundaries were not altered.

## Git Checkpoint / Push Status

- FVS implementation commit: `325c410` "feat: complete first vertical slice".
- Repository integration merge: `e8544d0` "Merge origin/main: drop legacy ClinicSystem
  and IDE artifacts" (integrates origin deletions of legacy artifacts; no code changes).
- Both pushed: `origin/main` == `e8544d0` (verified).

## Next Legitimate Action

- SOURCE-BACKED SELECTION AND PLANNING OF THE NEXT BOUNDED VERTICAL SLICE
  (separate governed task). Do not select or implement it from this document.

## Decision Classifications Used

- CONFIRMED IMPLEMENTED / VERIFIED: everything above explicitly tagged.
- IMPLEMENTATION DETAIL: transport mapping, seed mechanism, composition root,
  package pins, tooling location.
- ASSUMPTION / OPEN ITEM / FUTURE WORK: as tagged above; nothing treated as frozen.

## Source

Repository state, Git history (`git log`, `git status`), the test suite, and the
First Vertical Slice execution record referenced in `13_FIRST_VERTICAL_SLICE.md`
and `docs/implementation/FIRST_VERTICAL_SLICE.md`.