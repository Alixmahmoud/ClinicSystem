# Data Specification Baseline — Frozen through Part 15

## Status
Data Specification is COMPLETE / FROZEN through Part 15.

## Appointment and Visit Semantics
- Appointment lifecycle preserves Scheduled, Checked In, Completed, Cancelled, Potential No-Show / Needs Review, and No-Show semantics according to the defined transition model.
- No-show requires reception review and does not automatically create a Follow-Up.
- Missing an appointment does not make a patient inactive by itself.
- Visit is separate from Appointment.
- Walk-in is supported.
- Visit completion is independent from Appointment completion.

## Clinical History
- Completed clinical records are protected from ordinary edits.
- Corrections/addenda preserve the original record.
- Published Clinical Template versions are immutable.
- Clinical records reference the version used.

## Follow-Up / Task / Exception
Follow-Up, Task, and Exception have distinct lifecycle semantics and must remain separate in implementation and contracts.

## Financial Model
The financial model distinguishes:
- FinancialEncounter
- PerformedService
- historical expected amount
- Payment
- PaymentAllocation
- Balance
- Reconciliation
- Financial Exception

Financial rules in the master handoff:
- partial payments are supported;
- one payment can cover multiple services;
- allocation is explicit;
- historical prices/original financial values are preserved;
- later configuration changes do not rewrite history;
- overpayment handling is explicit;
- financial correction is distinct from void/reversal;
- daily reconciliation compares expected, recorded and actual received;
- discrepancies create exceptions.

## Migration
- staged/reviewable workflow;
- Excel/CSV supported;
- duplicate detection without automatic merge;
- failed batches can roll back.

## Backup / Restore
Backup/restore is manual or scheduled, verified, retention-aware, previewed/explicitly confirmed, backed up before restore, audited, and synchronization-aware.

## Open Data Details
Do not guess:
- exact physical schema;
- exact index choices;
- exact clinical field catalog where not frozen;
- clinical template implementation;
- follow-up catalogs/templates;
- escalation rules;
- migration taxonomy/algorithm/mapping/repeat-import/provenance;
- remaining backup/restore technical handling.

## Source
Master handoff pages 6–7 and 16.
