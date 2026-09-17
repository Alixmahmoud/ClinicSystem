# Open / Implementation-Detail Register

These items are explicitly unresolved or implementation-specific. They are not gaps to guess.

## Resolved During the First Vertical Slice (IMPLEMENTATION DETAIL)
Resolved at the implementation boundary, consistent with frozen semantics; these
resolutions are recorded for traceability and are NOT frozen specification changes.

- exact API routes for submit/recovery: `POST /api/operations/submit`,
  `GET /api/operations/{workstationId:guid}/{operationId:guid}`.
- transport/status mapping: HTTP 200 body `OperationResult` with a business `Outcome`
  for all 6 semantic outcomes; HTTP 401/403/422/400/502/503 map to the 5 failure
  categories; recovery GET 200 (result) or 404 (definitive "no record", safe re-submit
  of the same OperationId); other 5xx/network/timeout → UnknownOutcome.
- wire serialization: JSON (`System.Net.Http.Json`), envelope ProtocolVersion "1.0".
- seed/bootstrap mechanism: `Clinic.Contracts.Development.DevelopmentSeed` (single
  source), re-exported via `AuthoritySeedConstants`; used by both workstation and
  authority seeders (not a CreateAppointment bypass).
- package/build conventions: central package management, EF packages 10.0.4 family,
  Npgsql provider 10.0.3, SQLitePCLRaw.bundle_e_sqlite3 2.1.13;
  repo-local `dotnet-ef` tool manifest at repo root.
- identity/auth placeholder mechanism: fixed seeded reception user/workstation carried
  in the operation envelope; real session/token mechanism remains open.

## Security
- exact role-permission matrix where not frozen
- password/session/lockout values
- step-up values/mechanism
- offline grace/capability
- workstation certificate/enrollment mechanism
- SQLite encryption/key management
- audit retention/tamper evidence
- monitoring/tooling

## Data
- exact physical schema/indexes
- exact clinical field catalog where not frozen
- clinical-template implementation
- follow-up catalog/templates
- escalation rules
- migration taxonomy/algorithm/mapping/repeat-import/provenance
- remaining backup/restore technical handling

## API
- exact routes
- wire serialization
- transport/status mapping
- exact auth mechanism
- compatibility/version numbering and support window
- exact validation/authz matrices where still implementation-level
- binary document transport

## Synchronization
- exact change-feed mechanism
- snapshot consistency
- retry/backoff numbers
- conflict-retention implementation
- offline capability mechanics
- post-restore recovery algorithm
- monitoring/tooling
- remaining wire details

## Deployment
- installer/package tooling
- discovery mechanism
- certificate/TLS deployment
- enrollment automation
- update delivery
- rollback tooling
- mixed-version matrix
- diagnostics tooling
- exact environment configuration

## Repository
- implementation names/namespaces may evolve where they do not alter frozen boundaries;
- exact package references/build conventions may be refined within the established structure.

## Rule
Never invent an open item merely because an implementation feels incomplete. Resolve open details at the appropriate implementation/specification boundary and preserve frozen semantics.

## Source
Master handoff pages 16–18.
