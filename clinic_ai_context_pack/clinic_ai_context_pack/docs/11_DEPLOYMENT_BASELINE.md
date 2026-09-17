# Deployment Baseline — Frozen

## Status
Deployment is COMPLETE / FROZEN.

## Frozen Deployment Lifecycle
`Authority install → PostgreSQL configuration → Authority initialization → Clinic initialization → Initial admin → Clinic configuration → Backup configuration → Workstation install → Discover / Manual fallback → Enrollment → Trust → Initial snapshot → Incremental sync → Ready → Normal operation → Updates / migrations → Rollback / recovery → Workstation replacement / re-enrollment → Clinic recovery`

## Frozen Deployment Rules
- Installation is distinct from enrollment/trust.
- LAN discovery is supported with manual fallback.
- Updates are verified/staged with compatibility and health checks and rollback/recovery behavior.
- Application Version, Schema Version, Aggregate/Entity Version, API Contract Version and Sync Protocol Version remain distinct.
- Backup/restore is security-aware and synchronization-aware.
- Deployment covers topology, Authority/PostgreSQL placement, workstation deployment, WPF/SQLite initialization, LAN expectations, enrollment, application/environment configuration, database init/migrations, backup/restore, updates, compatibility, package verification, diagnostics, operator/admin procedures and deployment verification.

## Open Deployment Details
- exact installer/package technology
- exact discovery mechanism
- exact TLS/certificate deployment mechanism
- exact enrollment automation
- exact update delivery mechanism
- exact rollback mechanism
- exact mixed-version support matrix
- exact monitoring/diagnostic tooling
- exact environment configuration

## Source
Master handoff pages 11–12 and 16.
