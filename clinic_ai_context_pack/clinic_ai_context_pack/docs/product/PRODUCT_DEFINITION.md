# Product Definition — V1 Scope and Non-Goals

## Product
Desktop clinic-management system for medium multi-doctor clinics in Egypt, initially general specialty, designed around real clinic workflows and local-first continuity.

## Core Principles
- Workflow efficiency: fewer clicks, less typing, less navigation, less manual coordination.
- Visible state: users should see what matters now, relevant context, and the next meaningful action.
- Human control: consequential actions remain human-controlled.
- Deterministic before intelligent: predictable/explainable behavior before speculative AI.
- Local-first operation: ordinary core clinic work must not depend on internet availability.
- Exception-first operation: unresolved meaningful problems must be visible and actionable.
- Role-specific experience: Doctor, Reception, Cashier, Owner/Manager, and Administrator have distinct needs.
- Patient context should be coherent in UX without turning Patient into one giant technical aggregate.

## V1 Priorities
1. Doctor Workspace & Administrative Efficiency
2. Follow-Up & Retention
3. Payment & Daily Reconciliation
4. Patient Flow
5. Tasks & Exceptions

## Explicit V1 Exclusions
- Cloud infrastructure/dependency for core operation
- External WhatsApp/SMS/email patient communication
- Purchasing
- Advanced inventory
- Equipment management
- Full HR/payroll
- Advanced accounting
- Multi-branch operation
- Advanced analytics/BI
- AI infrastructure as a V1 architectural dependency

## Scope Rule
Do not reintroduce excluded scope during implementation unless explicitly revisited through an authoritative phase-gate decision.

## Source
Master handoff pages 4–5 and 17.
