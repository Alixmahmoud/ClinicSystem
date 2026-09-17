using Clinic.Authority.Domain.Appointments;
using Clinic.Authority.Domain.Audit;
using Clinic.Authority.Domain.Queue;
using Clinic.Authority.Domain.Security;
using Clinic.Authority.Domain.Sync;
using Clinic.Authority.Infrastructure.Persistence.Security;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Authority.Infrastructure.Persistence;

/// <summary>
/// Authority persistence boundary. Only Authority persistence accesses PostgreSQL;
/// workstations never do. Contracts do not expose these entities.
/// </summary>
public sealed class AuthorityDbContext : DbContext
{
    public AuthorityDbContext(DbContextOptions<AuthorityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<PatientFlowEntry> PatientFlowEntries => Set<PatientFlowEntry>();
    public DbSet<ProcessedOperation> ProcessedOperations => Set<ProcessedOperation>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();
    public DbSet<AuthoritativeChange> AuthoritativeChanges => Set<AuthoritativeChange>();
    public DbSet<WorkstationRecord> Workstations => Set<WorkstationRecord>();
    public DbSet<UserAccountRecord> Users => Set<UserAccountRecord>();

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

        modelBuilder.Entity<ProcessedOperation>(entity =>
        {
            entity.ToTable("processed_operations");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.WorkstationId).IsRequired();
            entity.Property(p => p.OperationId).IsRequired();
            entity.Property(p => p.CommandType).HasMaxLength(128).IsRequired();
            entity.Property(p => p.AggregateType).HasMaxLength(128).IsRequired();
            entity.Property(p => p.AggregateId).IsRequired();
            entity.Property(p => p.ResultingVersion);
            entity.Property(p => p.ProcessedAtUtc).IsRequired();
            entity.HasIndex(p => new { p.WorkstationId, p.OperationId }).IsUnique();
        });

        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.ToTable("audit_log_entries");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).ValueGeneratedOnAdd();
            entity.Property(a => a.OperationId).IsRequired();
            entity.Property(a => a.UserId).IsRequired();
            entity.Property(a => a.WorkstationId).IsRequired();
            entity.Property(a => a.AggregateType).HasMaxLength(128).IsRequired();
            entity.Property(a => a.AggregateId).IsRequired();
            entity.Property(a => a.ActionName).HasMaxLength(128).IsRequired();
            entity.Property(a => a.DetailJson).IsRequired();
            entity.Property(a => a.AtUtc).IsRequired();
            entity.HasIndex(a => a.OperationId);
        });

        modelBuilder.Entity<AuthoritativeChange>(entity =>
        {
            entity.ToTable("authoritative_changes");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
            entity.Property(c => c.OperationId).IsRequired();
            entity.Property(c => c.AggregateType).HasMaxLength(128).IsRequired();
            entity.Property(c => c.AggregateId).IsRequired();
            entity.Property(c => c.Version).IsRequired();
            entity.Property(c => c.CommandType).HasMaxLength(128).IsRequired();
            entity.Property(c => c.ChangeJson).IsRequired();
            entity.Property(c => c.OccurredAtUtc).IsRequired();
            entity.Property(c => c.Published).IsRequired();
            entity.HasIndex(c => new { c.AggregateType, c.AggregateId });
        });

        modelBuilder.Entity<WorkstationRecord>(entity =>
        {
            entity.ToTable("workstations");
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Id).ValueGeneratedNever();
            entity.Property(w => w.DisplayName).HasMaxLength(128).IsRequired();
            entity.Property(w => w.TrustState).HasConversion<int>().IsRequired();
        });

        modelBuilder.Entity<UserAccountRecord>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedNever();
            entity.Property(u => u.Username).HasMaxLength(128).IsRequired();
            entity.Property(u => u.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(u => u.Role).HasConversion<int>().IsRequired();
            entity.Property(u => u.IsActive).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
        });
    }
}