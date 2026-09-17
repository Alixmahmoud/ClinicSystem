# API / Contract Baseline — Frozen Semantics

## Status
API / Contract is COMPLETE / FROZEN semantically through Parts 1–10.

Exact routes, wire format, transport/status mapping, exact authentication mechanism, compatibility numbering/support window, and other explicitly open wire/technology choices remain open.

## Business Commands
### Patient
- CreatePatient
- UpdatePatient
- Archive/Deactivate

### Appointment / Scheduling
- CreateAppointment
- RescheduleAppointment
- CancelAppointment
- CheckInAppointment
- Review/RecordNoShow

### Walk-in / Flow / Visit
- CreateWalkInVisit
- CorrectPatientFlow
- ReorderQueuePosition
- MarkPatientLeft/DidNotWait
- StartConsultation
- Save/Update In-Progress Clinical Information
- CompleteVisit
- Correct/Addendum Completed Clinical Record

### Documents
- AddDocument
- ViewDocument

### Follow-Up
- CreateFollowUp
- RecordFollowUpOutcome
- TryAgain is an outcome/path, not a separate transport command.

### Tasks
- CreateTask
- CompleteTask
- CancelTask
- ReopenTask
- OverrideTaskPriority

### Exceptions
- CreateOperationalException
- ResolveException
- ReopenException

### Financial
- RecordPerformedService
- RecordPayment
- CorrectFinancialEvent
- Void/Reverse Financial Event
- ReconcilePayments
- ResolveFinancialDiscrepancy

### Notifications
- AcknowledgeNotification
- External patient messaging remains out of V1.

### Administration / Configuration
- user management
- role/permission changes
- service configuration
- doctor schedule/exceptions
- follow-up configuration
- publish clinical template version

### Backup / Restore
- CreateBackup
- RestoreBackup

### Migration
Staged workflow commands are supported conceptually; exact command names remain implementation detail unless separately frozen.

## Query Categories
- Patient workspace/history/appointments/follow-ups/financial context
- Doctor/clinical daily patients/consultation/template
- Appointment details/slots/conflicts/schedules/lists
- Patient flow/queue/next-patient/walk-in recommendations
- Follow-up due/overdue/details/history
- Tasks and exceptions
- Financial state/reconciliation/discrepancies/history
- Owner operational overviews and reports
- Admin configuration and backup status
- Audit search/details
- Connectivity and operation capability
- Synchronization status/pending/conflicts/result by OperationId
- Backup/restore status/preview/history
- Migration analysis/validation/duplicate review/preview/result

## Contract Semantics
- Commands express business intent.
- Queries are workflow/read-model oriented.
- UserId and WorkstationId are distinct request context identities.
- OperationId and BaseVersion are part of operation metadata where applicable.
- Validation layers: structural/envelope, required input, cross-field, business rule, state transition, dependency, concurrency.
- RBAC plus action/resource/context authorization.
- Authority-side authorization is authoritative.
- Default-deny/fail-closed is expected.
- View/Edit are distinct.
- Responsibility is not authorization.
- Outcomes: Accepted, Already Processed, Conflict, Rejected, Dependency Blocked, Temporarily Unavailable.
- Failure categories remain distinct.
- OperationId is exact idempotency identity.
- Unknown outcomes recover through OperationId lookup; never blind replay.
- BaseVersion is concurrency metadata, not API version.
- No universal LWW or generic rebase.
- Transport status and business outcome are distinct.
- Compatibility distinguishes Application Version, Schema Version, Aggregate/Entity Version, API Contract Version, and Synchronization Protocol Version.

## Rejected Directions
- Generic CRUD as synchronization/business contract
- ForceOverwrite
- MergePatient
- silent mutation of completed clinical records
- automatic NoShow Follow-Up
- external patient communications
- raw database synchronization commands

## Open Contract Details
- exact routes
- wire serialization
- transport/status mapping
- exact auth mechanism
- compatibility/version numbering and support window
- exact validation/authz matrices where implementation-level
- binary document transport

## Source
Master handoff pages 7–10 and 16.
