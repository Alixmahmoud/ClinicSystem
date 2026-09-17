# Clinic Management System — AI Implementation Instructions

## 1. Authority and Decision Discipline

Read the current master handoff and relevant detailed phase documents before making project decisions.

Decision classes:
- CONFIRMED / SELECTED
- PROPOSED
- REJECTED
- ASSUMPTION
- OPEN QUESTION
- IMPLEMENTATION DETAIL

Never silently promote an ASSUMPTION, PROPOSAL, OPEN QUESTION, or IMPLEMENTATION DETAIL into a frozen business decision.

When a contradiction is found:
- identify the authoritative source,
- quote or precisely identify the conflicting statements,
- do not silently reconcile them,
- classify whether it is an implementation defect, specification gap, contradiction, or upstream decision issue.

## 2. Project Status

The project is implementation-ready at the repository/solution level. The next task is the First Vertical Slice.

Do not restart Discovery, Requirements, Product Definition, UX, Architecture, Data, API/Contract, Synchronization, Security, Deployment, or Repository/Solution Structure unless a genuine source-backed contradiction or missing requirement requires phase-gate review.

## 3. Architecture

Runtime path:
`WPF → SQLite → Business-Aware Synchronization Boundary → Clinic Authority → PostgreSQL`

Rules:
- Clinic Authority owns authoritative shared clinic state.
- PostgreSQL is authoritative shared persistence behind Authority.
- Workstations never directly access PostgreSQL.
- SQLite is workstation-scoped durable local operational persistence.
- SQLite is not Authority and not a full PostgreSQL mirror.
- Synchronization is business-operation / aggregate oriented, not raw row replication.
- Authority is a V1 modular monolith / single Authority runtime direction.
- API contracts are business-oriented and do not expose persistence entities.
- Read models are derived/non-authoritative.
- Background workers must not bypass domain, authorization, audit, or application boundaries.

## 4. Explicitly Forbidden Directions

Never implement:
- WPF → PostgreSQL direct access
- workstation raw-sync-table bypasses
- Domain → WPF/HTTP/SQLite/EF implementation coupling
- reporting that mutates authoritative business state
- technical logs used as business audit
- sync engine direct raw-SQL mutation of Authority persistence
- row replication instead of operation-based sync
- universal Last-Write-Wins
- unrestricted offline Authority
- blind replay
- generic merge/rebase
- generic CRUD synchronization bypassing business operations

## 5. Domain Distinctions

The following are separate concepts and must remain separate in code, semantics, and contracts:

- Patient ≠ Appointment ≠ Visit ≠ Patient Flow
- Appointment Status ≠ Patient Flow State ≠ Visit State
- Follow-Up ≠ Task ≠ Exception
- Patient Status ≠ Follow-Up Status
- User identity ≠ Role ≠ Permission ≠ Workstation
- Responsibility ≠ Authorization
- Duplicate Detection ≠ Merge
- Service ≠ Performed Service
- Payment ≠ Payment Allocation
- Financial Encounter ≠ Reconciliation
- Payment Correction ≠ automatic Void/Reversal
- Original Financial Event ≠ Later Correction
- Expected Amount ≠ Actual Received Amount
- Completed Clinical Record ≠ ordinary editable record
- Business Data ≠ Audit History
- Business Data ≠ Technical Logs
- Business Data ≠ Synchronization Metadata
- Configuration ≠ Historical Transaction Data
- Aggregate Boundary ≠ Database Table Boundary
- Read Model ≠ Authoritative Business State
- Live report ≠ immutable historical exported report artifact
- Offline ≠ isolated
- Sync Operation ≠ Change Feed
- Conflict ≠ Network Failure
- Authentication ≠ Authorization
- RequestId / CorrelationId ≠ OperationId
- Transport status ≠ Business outcome
- BaseVersion ≠ API version

## 6. Synchronization Rules

Operation envelope includes:
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

Lifecycle:
`Created → Queued → Ready → Submitting → definitive result`
with `Unknown Outcome → OperationId Result Recovery`.

Invariants:
- one OperationId can create at most one authoritative business effect;
- local synchronization-bound business change + pending sync intent are atomic and durable;
- unknown outcomes are recoverable via OperationId lookup;
- Authority owns shared truth;
- optimistic concurrency uses Version + BaseVersion + OperationId;
- conflicts are explicit and domain-aware;
- dependencies must be respected;
- incoming authoritative change + cursor advancement are atomic;
- initial sync uses authoritative snapshot then incremental change feed;
- expired cursors require authoritative snapshot refresh then incremental continuation;
- offline capability is operation-specific and bounded;
- restore must be synchronization-aware.

Authority processing:
`Receive → Validate Envelope → Authenticate → Authorize → Check Idempotency → Validate Dependencies → Validate BaseVersion / Concurrency → Execute Domain Operation → Persist Transaction → Create Audit → Publish Authoritative Change → Return Result`

Semantic outcomes:
- Accepted
- Already Processed
- Conflict
- Rejected
- Dependency Blocked
- Temporarily Unavailable

Failure distinctions must remain explicit:
- network failure
- business rejection
- conflict
- temporary Authority unavailability
- authentication failure
- authorization failure
- validation failure
- malformed/unsupported operation or protocol

Unknown outcomes use OperationId recovery; never blindly replay.

## 7. Security Rules

- Human identity ≠ workstation identity.
- Authentication ≠ authorization.
- RBAC is the base model, refined by action/resource/context authorization.
- Authority-side authorization is authoritative.
- Default-deny / fail-closed is required.
- View and Edit are distinct capabilities.
- Responsibility is not authorization.
- LAN presence is not proof of trust.
- Workstation enrollment/trust is a security boundary.
- Offline Authority is bounded.
- High-risk operations receive stronger controls and may require recent/step-up security context.
- Business Audit, Security Events, Technical Logs, and Synchronization Diagnostics are separate categories.
- Restore is privileged, explicit, controlled, security-sensitive, and synchronization-aware.
- Update/package verification failures are security-relevant and must fail safely.
- Security failures are not blindly retried.

Do not invent unresolved security values or mechanisms.

## 8. Scope Discipline

Implement one bounded workflow at a time.

Do not implement the whole system at once.

Do not add speculative features.

Do not reintroduce V1 exclusions:
- cloud infrastructure/dependency for core operation
- external WhatsApp/SMS/email patient communication
- purchasing
- advanced inventory
- equipment management
- full HR/payroll
- advanced accounting
- multi-branch operation
- advanced analytics/BI
- AI infrastructure as a V1 architectural dependency

## 9. Database Rules

- Authority PostgreSQL migrations and workstation SQLite migrations remain separate.
- Only Authority persistence accesses PostgreSQL.
- SQLite is not a shared LAN database.
- Database tables must not be assumed to equal aggregate boundaries.

## 10. Testing

Use the repository test boundaries:
- Unit
- Integration
- Contract
- EndToEnd

A feature is not complete merely because it compiles. Test the semantic and cross-system behavior relevant to the current slice, including persistence, authorization, idempotency, concurrency, sync outcomes, and failure distinctions.

## 11. Git Safety

Do not push, destructively reset, delete unrelated work, overwrite unrelated changes, or rewrite history.
Do not commit unless explicitly instructed.

## 12. AI Workflow

Before implementation:
1. Read `AGENTS.md`.
2. Read relevant authoritative documents.
3. Inspect repository state.
4. Identify frozen rules.
5. Identify open details.
6. Identify contradictions.
7. Produce a bounded plan.

During implementation:
- implement only approved scope;
- do not guess business semantics;
- stop on genuine specification gaps;
- report deviations.

After implementation:
- build;
- run relevant tests;
- inspect diff;
- report changed files;
- report tests;
- report open items;
- report deviations from approved plan.
