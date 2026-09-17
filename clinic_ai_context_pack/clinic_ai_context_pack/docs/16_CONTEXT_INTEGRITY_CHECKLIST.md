# Context Integrity Checklist for AI Agents

Before starting a new task, confirm all of the following:

- Current phase pointer is **Incremental Workflow Implementation**, after the completed **First Vertical Slice** (implemented, tested, checkpointed, pushed — `docs/20_IMPLEMENTATION_STATE.md`).
- The next bounded vertical slice is **not selected**; selection is a separate, governed, source-backed task.
- Security is 18/18 complete and frozen; Part 17 audit PASS; Part 18 final freeze complete.
- Deployment is complete/frozen.
- Repository / Solution Structure is complete/implementation-ready.
- Architecture remains `WPF → SQLite → Sync Boundary → Authority → PostgreSQL`.
- Authority owns shared truth.
- Workstations never directly access PostgreSQL.
- Sync remains operation-based with stable OperationId, durable queue, explicit outcomes, bounded offline authority, and atomic cursor application.
- API semantic freeze remains intact; exact transport/wire/tooling details remain open where explicitly marked.
- High-risk, audit, restore, update, and workstation-trust boundaries remain security-sensitive.
- Deferred V1 scope is not reintroduced.
- Settled doctor workflow, appointment/scheduling, queue, patient workspace, follow-up, payment/reconciliation, and exception decisions are not re-asked without a genuine contradiction.
- No implementation proposal is treated as frozen merely because it appears in the repository structure.

## Startup Stop Condition
If any of these checks fail, stop and inspect the authoritative documents before implementing.

## Source
Master handoff page 17.
