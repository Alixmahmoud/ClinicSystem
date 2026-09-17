using Clinic.Authority.Application.CheckIn;
using Clinic.Authority.Domain.Appointments;
using Clinic.Authority.Domain.Sync;
using Clinic.Authority.Infrastructure.Persistence;
using Clinic.Authority.Infrastructure.Persistence.Repositories;
using Clinic.Authority.Infrastructure.Persistence.Security;
using Clinic.Authority.Infrastructure.Seeding;
using Clinic.Contracts.Commands;
using Clinic.Contracts.Operations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Clinic.Authority.Infrastructure.Tests;

public sealed class AuthorityPersistenceTests
{
    [Fact]
    public async Task MigrateAndSeed_CreateAuthorityFixture()
    {
        await using var database = await AuthorityTestDatabase.CreateAsync();

        await database.SeedAsync();

        await using var db = database.CreateContext();
        Assert.Contains(db.Users, u => u.Id == AuthoritySeedConstants.ReceptionUserId);
        Assert.Contains(db.Workstations, w => w.Id == AuthoritySeedConstants.WorkstationId);
        Assert.Contains(db.Appointments, a => a.Id == AuthoritySeedConstants.AppointmentId);
    }

    [Fact]
    public async Task ProcessedOperation_IsUniquePerWorkstationAndOperationId()
    {
        await using var database = await AuthorityTestDatabase.CreateAsync();

        await using var db = database.CreateContext();
        var workstationId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var processedAt = DateTimeOffset.UtcNow;

        db.ProcessedOperations.Add(new ProcessedOperation(
            workstationId, operationId, "CheckInAppointment", "Appointment", Guid.NewGuid(), 2, processedAt));
        db.ProcessedOperations.Add(new ProcessedOperation(
            workstationId, operationId, "CheckInAppointment", "Appointment", Guid.NewGuid(), 2, processedAt));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Processor_OnRealPostgres_ProducesSingleEffect_AndDeduplicatesTheOperationId()
    {
        await using var database = await AuthorityTestDatabase.CreateAsync();
        await database.SeedAsync();

        await using var db = database.CreateContext();
        var processor = new CheckInAppointmentProcessor(
            new UserAccountStore(db),
            new WorkstationTrustStore(db),
            new AuthorizationService(db),
            new AppointmentRepository(db),
            new PatientFlowRepository(db),
            new ProcessedOperationRepository(db),
            new AuditLogRepository(db),
            new AuthoritativeChangeRepository(db),
            new AuthorityUnitOfWork(db));

        var appointmentId = AuthoritySeedConstants.AppointmentId;
        var operationId = Guid.NewGuid();
        var envelope = new OperationEnvelope(
            operationId,
            AuthoritySeedConstants.WorkstationId,
            AuthoritySeedConstants.ReceptionUserId,
            AppointmentAggregateType.Value,
            appointmentId,
            CheckInAppointmentCommandType.Value,
            1,
            DateTimeOffset.UtcNow,
            "1.0",
            CheckInAppointmentCommandCodec.Encode(new CheckInAppointmentCommand(appointmentId)));

        var first = await processor.ProcessAsync(envelope);
        var second = await processor.ProcessAsync(envelope);

        Assert.Equal(OperationOutcome.Accepted, first.Outcome);
        Assert.Equal(OperationOutcome.AlreadyProcessed, second.Outcome);

        await using var verification = database.CreateContext();
        var appointment = await verification.Appointments.SingleAsync(a => a.Id == appointmentId);
        Assert.Equal(AppointmentStatus.CheckedIn, appointment.Status);
        Assert.Equal(2, appointment.Version);
        Assert.Equal(1, await verification.ProcessedOperations.CountAsync(p => p.OperationId == operationId));
        Assert.Equal(1, await verification.PatientFlowEntries.CountAsync(e => e.AppointmentId == appointmentId));
        Assert.Equal(1, await verification.AuditLogEntries.CountAsync(a => a.OperationId == operationId));
        Assert.Equal(1, await verification.AuthoritativeChanges.CountAsync(c => c.OperationId == operationId));
    }
}