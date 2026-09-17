using Clinic.Client.Domain.Appointments;
using Clinic.Client.Domain.Queue;
using Clinic.Client.Infrastructure.Persistence.Settings;
using Clinic.Client.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Clinic.Client.Infrastructure.Persistence;

/// <summary>
/// Workstation local persistence boundary (SQLite). Holds the local projection of
/// appointments, the local patient-flow/queue, and the durable sync intents. Only this
/// assembly touches the workstation's local store; the Authority is never queried here.
/// </summary>
public sealed class WorkstationDbContext : DbContext
{
    public WorkstationDbContext(DbContextOptions<WorkstationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<PatientFlowEntry> PatientFlowEntries => Set<PatientFlowEntry>();
    public DbSet<SyncOperation> SyncOperations => Set<SyncOperation>();
    public DbSet<WorkstationSettings> WorkstationSettings => Set<WorkstationSettings>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // SQLite cannot ORDER BY or compare DateTimeOffset natively. The binary converter stores
        // an order-preserving integer (UTC ticks dominate), enabling server-side ordering.
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToBinaryConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).ValueGeneratedNever();
            entity.Property(a => a.PatientId).IsRequired();
            entity.Property(a => a.DoctorId).IsRequired();
            entity.Property(a => a.ScheduledAtUtc).IsRequired();
            entity.Property(a => a.Status).HasConversion<int>().IsRequired();
            entity.Property(a => a.Version).IsRequired();
            entity.Property(a => a.ConfirmedVersion);
        });

        modelBuilder.Entity<PatientFlowEntry>(entity =>
        {
            entity.ToTable("patient_flow_entries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AppointmentId).IsRequired();
            entity.Property(e => e.PatientId).IsRequired();
            entity.Property(e => e.DoctorId).IsRequired();
            entity.Property(e => e.QueuedAtUtc).IsRequired();
            entity.HasIndex(e => new { e.DoctorId, e.QueuedAtUtc });
        });

        modelBuilder.Entity<SyncOperation>(entity =>
        {
            entity.ToTable("sync_operations");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).ValueGeneratedOnAdd();
            entity.Property(o => o.OperationId).IsRequired();
            entity.Property(o => o.OriginWorkstationId).IsRequired();
            entity.Property(o => o.UserId).IsRequired();
            entity.Property(o => o.AggregateType).HasMaxLength(128).IsRequired();
            entity.Property(o => o.AggregateId).IsRequired();
            entity.Property(o => o.CommandType).HasMaxLength(128).IsRequired();
            entity.Property(o => o.BaseVersion);
            entity.Property(o => o.CommandPayloadJson).IsRequired();
            entity.Property(o => o.ProtocolVersion).HasMaxLength(32).IsRequired();
            entity.Property(o => o.CreatedAtUtc).IsRequired();
            entity.Property(o => o.State).HasConversion<int>().IsRequired();
            entity.Property(o => o.ResultOutcome).HasConversion<int?>();
            entity.Property(o => o.ResultNewVersion);
            entity.Property(o => o.FailureCategory).HasConversion<int?>();
            entity.Property(o => o.ResultMessage);
            entity.Property(o => o.AttemptCount).IsRequired();
            entity.Property(o => o.UpdatedAtUtc).IsRequired();
            entity.HasIndex(o => o.OperationId).IsUnique();
        });

        modelBuilder.Entity<WorkstationSettings>(entity =>
        {
            entity.ToTable("workstation_settings");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).ValueGeneratedNever();
            entity.Property(s => s.WorkstationId).IsRequired();
            entity.Property(s => s.DisplayName).HasMaxLength(128).IsRequired();
        });
    }
}