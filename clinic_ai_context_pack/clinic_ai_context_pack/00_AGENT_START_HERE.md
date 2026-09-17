# Clinic Management System — AI Agent Start Here

## Purpose
This file is the entry point for any AI coding/analysis agent working on the Clinic Management System.

## Current Project Position
The project is stopped at the end of the Repository / Solution Structure phase. The next legitimate phase is the First Vertical Slice. No First Vertical Slice implementation has been executed in the source handoff.

## Non-Negotiable Startup Rules
1. Read this file and `AGENTS.md` before doing project work.
2. Read the authoritative phase files in `docs/` relevant to the task.
3. Treat the current master handoff as the current-state pointer.
4. Detailed phase documents remain authoritative for their own frozen content.
5. Do not restart completed phases.
6. Do not rediscover settled workflows unless a genuine source-backed contradiction exists.
7. Do not silently turn assumptions, proposals, open questions, or implementation details into frozen requirements.
8. Use only these decision classes: `CONFIRMED / SELECTED`, `PROPOSED`, `REJECTED`, `ASSUMPTION`, `OPEN QUESTION`, `IMPLEMENTATION DETAIL`.
9. If a contradiction is found, report the authoritative sources and exact conflicting statements; do not silently reconcile them.
10. Protect project integrity over conversational convenience.

## Current Phase Status
- Discovery: COMPLETE
- Requirements: COMPLETE / FROZEN
- Product Definition: COMPLETE
- UX: COMPLETE
- Architecture: COMPLETE / FROZEN through Part 28
- Data Specification: COMPLETE / FROZEN through Part 15
- API / Contract: COMPLETE / FROZEN semantically through Parts 1–10; exact wire/technology details left open where marked
- Synchronization: COMPLETE / SEMANTIC BASELINE FROZEN
- Security: COMPLETE / FROZEN 18/18; Part 17 integrity audit PASS; Part 18 final freeze complete
- Deployment: COMPLETE / FROZEN
- Repository / Solution Structure: COMPLETE / IMPLEMENTATION-READY
- First Vertical Slice: NEXT / PRACTICAL IMPLEMENTATION START
- Incremental Workflow Implementation: AFTER FIRST VERTICAL SLICE
- Integration + Failure Testing: AFTER MAJOR WORKFLOW IMPLEMENTATION
- Pilot / Release Hardening: FINAL PRE-RELEASE STAGE

## Runtime Architecture
`WPF Workstation → Local SQLite → Business-Aware Synchronization Boundary → Clinic Authority → PostgreSQL`

Clinic Authority owns authoritative shared clinic state. PostgreSQL is authoritative shared persistence behind Authority. Workstations never directly access PostgreSQL. SQLite is workstation-scoped durable local operational persistence and is not Authority or a full PostgreSQL mirror.

## First Vertical Slice Goal
The slice must prove:
`WPF → Application / Use-Case Boundary → Domain Operation → Local SQLite → Sync Operation → Clinic Authority → PostgreSQL → Authority Result → Local Sync State`

The current handoff does **not** choose the first business command. Selection must come from authoritative Requirements / API / Domain material, not from implementation convenience.

## Forbidden Shortcuts
- WPF → PostgreSQL direct access
- workstation raw-sync-table bypasses
- raw database replication
- generic CRUD synchronization/business contracts
- universal Last-Write-Wins
- blind replay
- generic merge/rebase
- unlimited offline Authority
- regenerating OperationId on retry
- generic error collapsing semantic outcomes
- sync paths that bypass Authority authorization/domain/audit
- reintroducing explicitly excluded V1 scope

## Important Reference Files
- `docs/01_CURRENT_STATE.md`
- `docs/02_AUTHORITY_HIERARCHY.md`
- `docs/03_PRODUCT_V1_SCOPE.md`
- `docs/04_REQUIREMENTS_STATUS.md`
- `docs/05_UX_BASELINE.md`
- `docs/06_ARCHITECTURE_BASELINE.md`
- `docs/07_DATA_BASELINE.md`
- `docs/08_API_CONTRACT_BASELINE.md`
- `docs/09_SYNCHRONIZATION_BASELINE.md`
- `docs/10_SECURITY_BASELINE.md`
- `docs/11_DEPLOYMENT_BASELINE.md`
- `docs/12_REPOSITORY_STRUCTURE.md`
- `docs/13_FIRST_VERTICAL_SLICE.md`
- `docs/14_OPEN_ITEM_REGISTER.md`
- `docs/15_CHANGE_GOVERNANCE.md`
- `docs/16_CONTEXT_INTEGRITY_CHECKLIST.md`
- `docs/17_SOURCE_AUTHORITY_AND_GAPS.md`

## Source Limitation
This pack is an extraction of the current master handoff. The handoff itself explicitly says that detailed phase documents remain authoritative for their own frozen phase content. Therefore, a phase file in this pack must not be treated as a substitute for a missing detailed phase document when exact requirements are needed.

## Source
Extracted from: `CLINIC MANAGEMENT SYSTEM — COMPLETE MASTER HANDOFF — CURRENT THROUGH REPOSITORY / SOLUTION STRUCTURE`, 18 pages.
