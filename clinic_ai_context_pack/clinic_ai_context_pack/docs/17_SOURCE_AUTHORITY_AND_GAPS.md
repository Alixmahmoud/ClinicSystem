# Source Authority, Coverage, and Gaps

## What This Pack Is
This pack extracts the important current-state information present in the current master handoff and organizes it into agent-friendly Markdown files.

## What This Pack Is Not
It is not a replacement for the full detailed phase specifications. The master handoff explicitly states that detailed phase documents remain authoritative for their own frozen phase content.

## Consequence
When a task requires exact:
- requirement acceptance criteria;
- detailed workflow transitions;
- detailed UX behavior;
- complete permissions;
- detailed aggregate/domain rules;
- exact physical schema;
- exact route/DTO/wire details;
- exact security values;
- exact deployment tooling;

the agent must read the relevant detailed phase document rather than inventing missing details from this pack.

## Current Handoff Source
`CLINIC MANAGEMENT SYSTEM — COMPLETE MASTER HANDOFF — CURRENT THROUGH REPOSITORY / SOLUTION STRUCTURE`

## Source Principle
Current handoff is the current-state pointer. Older handoffs and old chat are traceability/evidence only and never override the current-state pointer.

## Coverage Map
| Area | Master Handoff Coverage | Detailed Source Still Needed? |
|---|---|---|
| Discovery | state + phase authority | Yes, for detailed discovery evidence |
| Requirements | frozen status + role in authority hierarchy | Yes |
| Product Definition | substantial V1 scope/principles | Detailed document may still be required for full positioning/rationale |
| UX | established areas/status | Yes |
| Architecture | substantial/frozen baseline | Detailed architecture docs remain authoritative |
| Data | substantial/frozen baseline | Yes for exact data semantics not reproduced here |
| API/Contract | substantial/frozen semantic baseline | Yes for exact parts and detailed contract definitions |
| Synchronization | substantial/frozen semantic baseline | Yes for exact implementation details and detailed semantics |
| Security | substantial/frozen baseline | Yes for detailed 18-part specification |
| Deployment | substantial/frozen baseline | Yes for detailed deployment procedures |
| Repository | implementation-ready structure | Current handoff is sufficient for the summarized structure |
| First Vertical Slice | boundary and completion gate | Workflow-specific source material is still required for selection |

## Agent Rule
Never compensate for a missing detailed authoritative source by inventing requirements.

## Source
Master handoff pages 1–3, 16–18.
