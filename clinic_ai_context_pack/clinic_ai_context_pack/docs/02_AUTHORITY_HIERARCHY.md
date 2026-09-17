# Authority Hierarchy and Decision Discipline

## Source Priority
1. Frozen Requirements — required behavior, states, rules, permissions, NFRs, acceptance criteria, V1 scope/exclusions.
2. Product Definition — product direction, V1 priorities, principles, non-goals.
3. UX — established workflow and experience behavior.
4. Architecture through Part 28 — frozen system architecture and ownership boundaries.
5. Data Specification through Part 15 — frozen data semantics and protection boundaries.
6. API / Contract Parts 1–10 — frozen semantic contract baseline; exact wire/technology details explicitly left open remain open.
7. Synchronization Specification — frozen operation-based / Authority-centric semantics and open-item boundary.
8. Security 18/18 — frozen security semantics and completion gate.
9. Deployment — frozen deployment and recovery/update boundaries.
10. Repository / Solution Structure — current implementation structure and dependency constraints.
11. Historical handoffs / old chat — traceability/evidence only; never override current state.

## Decision Classes
- CONFIRMED / SELECTED — preserve unless a later authoritative phase explicitly changes it.
- PROPOSED — working direction, not frozen.
- REJECTED — explicitly ruled out; do not reintroduce without explicit revisit decision.
- ASSUMPTION — temporary reasoning only; never silently promote.
- OPEN QUESTION — unresolved; do not guess.
- IMPLEMENTATION DETAIL — concrete implementation/tooling choice that must preserve frozen semantics unless explicitly elevated.

## Contradiction Handling
When a contradiction is found, identify the authoritative source and exact conflicting statements. Do not silently reconcile.

## Change Principle
A later implementation problem does not automatically reopen an earlier frozen phase.
