# OpenCode — Read-Only Review Prompt Template

```text
Perform a read-only review of the implemented First Vertical Slice.

Read:
- AGENTS.md
- approved slice specification
- relevant architecture/data/API/synchronization/security documents
- current diff and tests

Check for:
- forbidden WPF → PostgreSQL access
- raw sync-table bypasses
- Domain coupling to WPF/HTTP/EF/SQLite implementation
- aggregate-boundary violations
- missing OperationId stability/idempotency
- incorrect BaseVersion/concurrency handling
- authorization bypasses
- missing audit where required
- local persistence + pending sync intent durability
- incorrect sync-state transitions
- semantic failure collapse
- incorrect unknown-outcome handling
- missing tests
- scope creep

Classify findings as CRITICAL / IMPORTANT / MINOR / PASS. Give file/location, rule violated, evidence, and correction proposal. Do not edit files.
```
