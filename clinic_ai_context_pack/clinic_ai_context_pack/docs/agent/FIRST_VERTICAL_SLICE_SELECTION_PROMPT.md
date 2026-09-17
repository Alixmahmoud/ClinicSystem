# OpenCode — First Vertical Slice Selection Prompt

```text
Read AGENTS.md and all authoritative documents relevant to the current project state.

We are at the First Vertical Slice boundary. Do not write application code.

Identify candidate source-backed bounded workflows that can demonstrate:
WPF → Application / Use-Case Boundary → Domain Operation → Local SQLite → Sync Operation → Clinic Authority → PostgreSQL → Authority Result → Local Sync State.

Do not invent a workflow, redesign requirements, rank product value, or silently select a winner.

For each candidate report its authoritative source, command, aggregate(s), local persistence, Authority persistence, synchronization behavior, authorization, idempotency, concurrency/BaseVersion, audit, semantic failure cases, tests, dependencies, and open implementation details.

Stop after the analysis and wait for project-owner selection.
```
