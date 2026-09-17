CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "appointments" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_appointments" PRIMARY KEY,
    "PatientId" TEXT NOT NULL,
    "DoctorId" TEXT NOT NULL,
    "ScheduledAtUtc" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL,
    "Version" INTEGER NOT NULL,
    "ConfirmedVersion" INTEGER NULL
);

CREATE TABLE "patient_flow_entries" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_patient_flow_entries" PRIMARY KEY,
    "AppointmentId" TEXT NOT NULL,
    "PatientId" TEXT NOT NULL,
    "DoctorId" TEXT NOT NULL,
    "QueuedAtUtc" INTEGER NOT NULL
);

CREATE TABLE "sync_operations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_sync_operations" PRIMARY KEY AUTOINCREMENT,
    "OperationId" TEXT NOT NULL,
    "OriginWorkstationId" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    "AggregateType" TEXT NOT NULL,
    "AggregateId" TEXT NOT NULL,
    "CommandType" TEXT NOT NULL,
    "BaseVersion" INTEGER NULL,
    "CommandPayloadJson" TEXT NOT NULL,
    "ProtocolVersion" TEXT NOT NULL,
    "CreatedAtUtc" INTEGER NOT NULL,
    "State" INTEGER NOT NULL,
    "ResultOutcome" INTEGER NULL,
    "ResultNewVersion" INTEGER NULL,
    "FailureCategory" INTEGER NULL,
    "ResultMessage" TEXT NULL,
    "AttemptCount" INTEGER NOT NULL,
    "UpdatedAtUtc" INTEGER NOT NULL
);

CREATE TABLE "workstation_settings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_workstation_settings" PRIMARY KEY,
    "WorkstationId" TEXT NOT NULL,
    "DisplayName" TEXT NOT NULL
);

CREATE INDEX "IX_patient_flow_entries_DoctorId_QueuedAtUtc" ON "patient_flow_entries" ("DoctorId", "QueuedAtUtc");

CREATE UNIQUE INDEX "IX_sync_operations_OperationId" ON "sync_operations" ("OperationId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260917125654_InitialWorkstationCreate', '10.0.4');

COMMIT;

