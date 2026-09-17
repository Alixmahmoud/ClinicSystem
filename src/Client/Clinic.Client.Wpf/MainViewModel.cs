using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Clinic.Client.Application.CheckIn;
using Clinic.Client.Application.Identity;
using Clinic.Client.Domain.Appointments;
using Clinic.Client.Sync;

namespace Clinic.Client.Wpf;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly CheckInAppointmentUseCase _checkInUseCase;
    private readonly IAppointmentListQuery _listQuery;
    private readonly ISyncEngine _syncEngine;
    private readonly ICurrentIdentity _identity;

    private AppointmentItemViewModel? _selectedAppointment;
    private string _statusMessage = "Ready.";
    private bool _busy;

    public MainViewModel(
        CheckInAppointmentUseCase checkInUseCase,
        IAppointmentListQuery listQuery,
        ISyncEngine syncEngine,
        ICurrentIdentity identity)
    {
        _checkInUseCase = checkInUseCase;
        _listQuery = listQuery;
        _syncEngine = syncEngine;
        _identity = identity;

        Appointments = new ObservableCollection<AppointmentItemViewModel>();
        CheckInCommand = new RelayCommand(async _ => await CheckInSelectedAsync(), _ => CanCheckInSelected);
        RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
        SyncNowCommand = new RelayCommand(async _ => await SyncNowAsync());
    }

    public ObservableCollection<AppointmentItemViewModel> Appointments { get; }

    public AppointmentItemViewModel? SelectedAppointment
    {
        get => _selectedAppointment;
        set
        {
            if (ReferenceEquals(_selectedAppointment, value)) return;
            _selectedAppointment = value;
            OnPropertyChanged();
            CheckInCommand.RaiseCanExecuteChanged();
        }
    }

    public string IdentityText => $"{_identity.UserName} @ {_identity.WorkstationName}";

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool CanCheckInSelected =>
        SelectedAppointment is { Status: AppointmentStatus.Scheduled } && !_busy;

    public RelayCommand CheckInCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand SyncNowCommand { get; }

    public async Task RefreshAsync()
    {
        if (_busy) return;

        _busy = true;
        try
        {
            var summaries = await _listQuery.ListAsync();
            var keepId = SelectedAppointment?.Id;

            Appointments.Clear();
            foreach (var summary in summaries)
            {
                Appointments.Add(new AppointmentItemViewModel(summary));
            }

            if (keepId is not null)
            {
                SelectedAppointment = Appointments.FirstOrDefault(a => a.Id == keepId);
            }

            if (summaries.Count == 0)
            {
                StatusMessage = "No appointments are loaded on this workstation.";
            }

            CheckInCommand.RaiseCanExecuteChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Refresh failed: {ex.Message}";
        }
        finally
        {
            _busy = false;
            CheckInCommand.RaiseCanExecuteChanged();
        }
    }

    public async Task CheckInSelectedAsync()
    {
        var selected = SelectedAppointment;
        if (selected is null || _busy) return;

        _busy = true;
        try
        {
            var result = await _checkInUseCase.CheckInAsync(selected.Id);
            StatusMessage = result.Outcome switch
            {
                CheckInAppointmentLocalOutcome.CheckedInAndQueuedForSync =>
                    $"Checked in locally (version {result.LocalVersion}); synchronized operation {result.OperationId:N} is queued.",
                CheckInAppointmentLocalOutcome.AlreadyCheckedIn =>
                    $"Already checked in locally (version {result.LocalVersion})." +
                    (result.HasPendingSync ? " Sync is still pending for this appointment." : " Fully synchronized."),
                CheckInAppointmentLocalOutcome.AppointmentNotFound =>
                    "Appointment not found on this workstation.",
                CheckInAppointmentLocalOutcome.InvalidTransition =>
                    $"Cannot check in: {result.Message}",
                _ => StatusMessage
            };
        }
        catch (Exception ex)
        {
            StatusMessage = $"Check-in failed: {ex.Message}";
        }
        finally
        {
            _busy = false;
            CheckInCommand.RaiseCanExecuteChanged();
        }

        await RefreshAsync();
    }

    public async Task SyncNowAsync()
    {
        if (_busy) return;

        _busy = true;
        var processedCount = 0;
        try
        {
            for (var pass = 0; pass < 8; pass++)
            {
                var progressed = await _syncEngine.ProcessPendingAsync(16);
                processedCount += progressed;
                if (progressed == 0)
                {
                    break;
                }
            }

            StatusMessage = processedCount == 0
                ? "Nothing pending to synchronize."
                : $"Synchronized {processedCount} pending operation(s) with the Clinic Authority.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Synchronization failed: {ex.Message}";
        }
        finally
        {
            _busy = false;
        }

        await RefreshAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public sealed class AppointmentItemViewModel
{
    public AppointmentItemViewModel(AppointmentSummary summary)
    {
        Summary = summary;
    }

    public AppointmentSummary Summary { get; }

    public Guid Id => Summary.Id;
    public Guid PatientId => Summary.PatientId;
    public Guid DoctorId => Summary.DoctorId;
    public AppointmentStatus Status => Summary.Status;
    public long Version => Summary.Version;

    public string HeaderText =>
        $"Patient {(PatientId == Guid.Empty ? "—" : PatientId.ToString("N").ToUpperInvariant())}";

    public string DetailText =>
        $"Doctor {DoctorId.ToString("N").ToUpperInvariant()} · {ScheduledAtUtc:HH:mm} · {StatusText} · v{Version}";

    public DateTimeOffset ScheduledAtUtc => Summary.ScheduledAtUtc;

    public string StatusText =>
        Summary.Status switch
        {
            AppointmentStatus.Scheduled => "Scheduled",
            AppointmentStatus.CheckedIn => "Checked In",
            AppointmentStatus.Completed => "Completed",
            AppointmentStatus.Cancelled => "Cancelled",
            AppointmentStatus.PotentialNoShow => "Potential No-Show",
            AppointmentStatus.NoShow => "No-Show",
            _ => Summary.Status.ToString()
        };

    public string SyncBadge =>
        Summary.HasPendingSync
            ? $"sync: {SyncStateText}"
            : (Summary.Status == AppointmentStatus.CheckedIn ? "synced" : string.Empty);

    private string SyncStateText =>
        Summary.SyncState switch
        {
            SyncOperationState.Queued => "queued",
            SyncOperationState.Submitting => "submitting",
            SyncOperationState.TemporarilyUnavailable => "retry later",
            SyncOperationState.UnknownOutcome => "outcome unknown",
            _ => "pending"
        };
}