# OpenCode — First Vertical Slice Build Prompt Template

```text
Read AGENTS.md and the approved First Vertical Slice specification.

Implement ONLY the approved slice.

Preserve all frozen architecture, data, API/contract, synchronization, security, and deployment semantics.

The implementation must preserve:
- WPF/application/domain boundaries
- local SQLite persistence
- durable pending synchronization intent
- stable OperationId
- BaseVersion/concurrency semantics where applicable
- Authority-side authentication/authorization
- Domain invariant enforcement
- Authority transaction and audit boundaries
- explicit semantic outcomes
- unknown-outcome OperationId recovery

Do not invent missing business rules. Stop and report genuine specification gaps.

After implementation:
1. build;
2. run relevant unit/integration/contract/end-to-end tests;
3. inspect the diff;
4. report changed files;
5. report tests and results;
6. report open items;
7. report any deviation from the approved slice plan.

Do not commit or push unless explicitly requested.
```
