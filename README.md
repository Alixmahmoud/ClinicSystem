# Clinic Management System

Desktop clinic-management system for medium multi-doctor clinics, designed around real clinic
workflows and local-first continuity.

## Authoritative Context

The project context package (master handoff extraction, phase documents, decision discipline rules)
is preserved at `clinic_ai_context_pack/`. Start with
`clinic_ai_context_pack/clinic_ai_context_pack/AGENTS.md` and
`clinic_ai_context_pack/clinic_ai_context_pack/00_AGENT_START_HERE.md`.

## Current Status

The **First Vertical Slice** (`CheckInAppointment`) is **COMPLETE**: implemented, tested
(53/53 tests across 10 projects), checkpointed, and pushed to GitHub. The project is now
in **Incremental Workflow Implementation**; the next bounded slice is not yet selected
(selection is a separate, source-backed, governed task). See
`clinic_ai_context_pack/clinic_ai_context_pack/docs/20_IMPLEMENTATION_STATE.md` for the
current implementation record.

Frozen runtime path:

```
WPF Workstation
    -> Local SQLite
    -> Business-Aware Synchronization Boundary
    -> Clinic Authority
    -> PostgreSQL
```

## Repository Structure

```
src/
  Client/       (WPF, Client.Application, Client.Domain, Client.Infrastructure, Client.Sync)
  Authority/    (Authority.Api, Authority.Application, Authority.Domain, Authority.Infrastructure, Authority.Worker)
  Contracts/    (Clinic.Contracts - boundary contracts only)
tests/
  Unit/         (4 projects)
  Integration/  (4 projects)
  Contract/     (1 project)
  EndToEnd/     (1 project)
database/
  authority/migrations/     (PostgreSQL migrations - separate from workstation)
  workstation/migrations/   (SQLite migrations - separate from authority)
deployment/     (authority, workstation, packages, scripts, configuration)
tools/          (development, migration, diagnostics)
docs/           (architecture, requirements, api, synchronization, security, deployment)
```

## Build

```
dotnet restore
dotnet build
```

Requires the .NET 10 SDK (see `global.json`).

## Scope Discipline

V1 exclusions (cloud dependency, external patient messaging, purchasing, advanced inventory,
equipment management, full HR/payroll, advanced accounting, multi-branch operation, advanced
analytics/BI, AI infrastructure) must not be reintroduced.