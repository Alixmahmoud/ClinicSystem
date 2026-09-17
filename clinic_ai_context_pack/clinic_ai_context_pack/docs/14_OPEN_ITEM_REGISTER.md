# Open / Implementation-Detail Register

These items are explicitly unresolved or implementation-specific. They are not gaps to guess.

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
