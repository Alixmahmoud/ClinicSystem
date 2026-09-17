using Clinic.Client.Domain.Abstractions;
using Clinic.Client.Domain.Appointments;
using Clinic.Client.Sync;

namespace Clinic.Client.Application.CheckIn;

public sealed record AppointmentSummary(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    DateTimeOffset ScheduledAtUtc,
    AppointmentStatus Status,
    long Version,
    long? ConfirmedVersion,
    bool HasPendingSync,
    SyncOperationState? SyncState);

/// <summary>
/// Provides the workstation's appointment list with its durable sync status. This reads the
/// local projection; the Clinic Authority remains the source of truth for reconciled state.
/// </summary>
public interface IAppointmentListQuery
{
    Task<IReadOnlyList<AppointmentSummary>> ListAsync(CancellationToken cancellationToken = default);
}

public sealed class AppointmentListQuery : IAppointmentListQuery
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ISyncOperationRepository _syncOperationRepository;

    public AppointmentListQuery(
        IAppointmentRepository appointmentRepository,
        ISyncOperationRepository syncOperationRepository)
    {
        _appointmentRepository = appointmentRepository;
        _syncOperationRepository = syncOperationRepository;
    }

    public async Task<IReadOnlyList<AppointmentSummary>> ListAsync(CancellationToken cancellationToken = default)
    {
        var appointments = await _appointmentRepository.ListAsync(cancellationToken);
        var pending = await _syncOperationRepository.ListPendingAsync(cancellationToken);

        return appointments
            .OrderBy(a => a.ScheduledAtUtc)
            .Select(a =>
            {
                var syncOperation = pending.FirstOrDefault(o =>
                    string.Equals(o.AggregateType, AppointmentAggregateType.Value, StringComparison.Ordinal)
                    && o.AggregateId == a.Id
                    && string.Equals(o.CommandType, CheckInAppointmentCommandName.Value, StringComparison.Ordinal));
                return new AppointmentSummary(
                    a.Id,
                    a.PatientId,
                    a.DoctorId,
                    a.ScheduledAtUtc,
                    a.Status,
                    a.Version,
                    a.ConfirmedVersion,
                    syncOperation is not null,
                    syncOperation?.State);
            })
            .ToArray();
    }
}