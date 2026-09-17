# OpenCode Implementation Protocol — Derived Agent Workflow

> STATUS: RECOMMENDED AGENT WORKFLOW. This is an operational template derived from the frozen master handoff; it is not itself a business requirement or architecture decision.

## Phase 1 — Orientation
Read the root `AGENTS.md`, current handoff, and relevant phase documents. Produce an implementation-readiness report. Do not edit feature code.

## Phase 2 — First Vertical Slice Selection
Use authoritative Requirements / API / Domain material to identify candidate source-backed bounded workflows. Do not invent or silently select a workflow.

## Phase 3 — Slice Plan
After a workflow is selected by the project owner, produce a detailed implementation plan covering WPF → Application → Domain → SQLite → Sync → Authority → PostgreSQL → Result → local sync state.

## Phase 4 — Build
Implement only the approved slice. Preserve OperationId, BaseVersion/concurrency, authorization, transactions, audit, durable local sync intent, semantic outcomes, and unknown-outcome recovery.

## Phase 5 — Verification
Build, run relevant tests, inspect the diff, verify dependency boundaries, and check semantic failure handling.

## Phase 6 — Review
Use a read-only reviewer to compare implementation with `AGENTS.md`, the slice plan, and frozen phase documents.

## Phase 7 — Git Checkpoint
Commit only after the owner reviews the result and explicitly authorizes the checkpoint.

## Stop Conditions
Stop rather than guessing when: the required business rule is not authoritative, a document contradiction exists, a dependency boundary would need to be violated, or an open implementation detail materially affects correctness.
