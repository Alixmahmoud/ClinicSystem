# Architecture Baseline — Frozen

## Status
Architecture is COMPLETE / FROZEN through Part 28.

## Runtime Topology
```text
WPF Workstation
    ↓
Local SQLite
    ↓
Business-Aware Synchronization Boundary
    ↓
Clinic Authority
    ↓
PostgreSQL
```

## Ownership and Boundaries
- Clinic Authority owns authoritative shared clinic state.
- PostgreSQL is authoritative shared persistence behind Authority.
- Workstations never directly access PostgreSQL.
- SQLite is workstation-scoped durable local operational persistence.
- SQLite is not Authority and not a full PostgreSQL mirror.
- Synchronization is business-operation / aggregate oriented, not raw row replication.
- Authority is a modular monolith / single Authority runtime direction for V1.
- API contracts are business-oriented and do not expose persistence entities.
- Read models are derived/non-authoritative.
- Background workers cannot bypass domain, authorization, audit, or application boundaries.

## Selected Technology Direction
- WPF on modern .NET for workstation UI.
- Clinic Authority as ASP.NET Core, deployable as a Windows Service for V1.
- PostgreSQL for authoritative shared persistence.
- SQLite for workstation-local persistence.
- Entity Framework Core as the persistence framework where applicable.
- Single modular Authority process for V1.
- Avoid microservices/Kubernetes/cloud-dependent core infrastructure.
- WebView2 is optional only when a concrete product need justifies it; it is not a default requirement.

## Explicitly Forbidden Architecture
- WPF → PostgreSQL direct access
- workstation raw-sync-table bypasses
- Domain → WPF/HTTP/SQLite/EF implementation coupling
- Reporting that mutates authoritative business state
- Technical logs used as business audit
- Sync engine direct raw-SQL mutation of Authority persistence
- row replication instead of operation-based synchronization
- universal Last-Write-Wins
- unrestricted offline Authority

## Source
Master handoff pages 5–6.
