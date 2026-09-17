# Business Command Catalog Extract

This file isolates the business-command catalog from the master handoff for agent retrieval.

## Patient
- CreatePatient
- UpdatePatient
- Archive/Deactivate

## Appointment / Scheduling
- CreateAppointment
- RescheduleAppointment
- CancelAppointment
- CheckInAppointment
- Review/RecordNoShow

## Walk-in / Flow / Visit
- CreateWalkInVisit
- CorrectPatientFlow
- ReorderQueuePosition
- MarkPatientLeft/DidNotWait
- StartConsultation
- Save/Update In-Progress Clinical Information
- CompleteVisit
- Correct/Addendum Completed Clinical Record

## Documents
- AddDocument
- ViewDocument

## Follow-Up
- CreateFollowUp
- RecordFollowUpOutcome
- TryAgain is an outcome/path, not a separate transport command.

## Tasks
- CreateTask
- CompleteTask
- CancelTask
- ReopenTask
- OverrideTaskPriority

## Exceptions
- CreateOperationalException
- ResolveException
- ReopenException

## Financial
- RecordPerformedService
- RecordPayment
- CorrectFinancialEvent
- Void/Reverse Financial Event
- ReconcilePayments
- ResolveFinancialDiscrepancy

## Notifications
- AcknowledgeNotification
- External patient messaging is out of V1.

## Administration / Configuration
- user management
- role/permission changes
- service configuration
- doctor schedule/exceptions
- follow-up configuration
- publish clinical template version

## Backup / Restore
- CreateBackup
- RestoreBackup

## Migration
Staged workflow commands are supported conceptually; exact names remain implementation detail unless explicitly frozen.

## Rejected Directions
- Generic CRUD business/sync contract
- ForceOverwrite
- MergePatient
- Silent mutation of completed clinical records
- Automatic NoShow Follow-Up
- External patient communications
- Raw database synchronization commands

## Source
Master handoff pages 7–8.
