# Security Baseline — Frozen 18/18

## Status
Security Specification is COMPLETE / FROZEN 18/18.
Part 17 integrity audit: PASS.
Part 18 final freeze: COMPLETE.

## Frozen Security Principles
- Human identity ≠ workstation identity.
- Authentication ≠ authorization.
- RBAC is the base model, refined by action/resource/context authorization.
- Authority-side authorization is authoritative; client-side checks are UX support only.
- Default-deny / fail-closed is required for security boundaries.
- View and Edit are distinct capabilities.
- Responsibility is not authorization.
- LAN presence is not proof of trust.
- Workstation enrollment/trust is a security boundary.
- Offline Authority is bounded; unlimited offline Authority is rejected.
- High-risk operations receive stronger controls and may require recent/step-up security context.
- Business Audit ≠ Security Events ≠ Technical Logs ≠ Synchronization Diagnostics.
- Restore is privileged, explicit, controlled, security-sensitive and synchronization-aware.
- Update/package verification failures are security-relevant and must fail safely.
- Unknown outcomes use OperationId recovery; security failures are not blindly retried.
- Conflict is not a security failure.
- Temporary Authority unavailability is not authorization denial.
- Backups, local SQLite data, secrets and secure LAN communication require protected handling.

## High-Risk Categories
- financial corrections/voids/reversals/discrepancies/historical financial changes
- sensitive clinical edits
- completed-visit correction/addendum
- appointment/queue overrides
- restore
- audit access

## Workstation Trust Lifecycle
`Unenrolled → Enrollment Requested → Authorized → Trusted / Active → Suspended / Revoked → Re-enrolled`

## Explicitly Open Security Details
Do not guess:
- exact role-permission matrix where not frozen
- password policy values
- session timeout / lockout values
- step-up mechanism/thresholds
- offline grace/capability mechanics
- workstation certificate/enrollment implementation
- SQLite encryption and key management
- audit retention/tamper-evidence implementation
- monitoring technology/tooling
- other explicit security technology choices

## Source
Master handoff pages 10–12 and 16.
