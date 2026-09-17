using System.Windows;
using Clinic.Client.Application.CheckIn;
using Clinic.Client.Application.Identity;
using Clinic.Client.Infrastructure;
using Clinic.Client.Infrastructure.Persistence;
using Clinic.Client.Infrastructure.Persistence.Repositories;
using Clinic.Client.Sync;
using Clinic.Contracts.Development;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Client.Wpf;

/// <summary>
/// Composition root for the workstation desktop app. Local store is initialized first;
/// the Authority connection is optional at startup (the sync engine activates on demand).
/// </summary>
public partial class App : System.Windows.Application
{
    public const string DefaultAuthorityBaseUrl = "http://localhost:5210";

    private AppServices? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var (services, viewModel) = await BuildAsync(e.Args);
            _services = services;

            var window = new MainWindow { DataContext = viewModel };
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The workstation could not start.\n\n{ex.Message}",
                "Clinic Workstation",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }

    private static async Task<(AppServices Services, MainViewModel ViewModel)> BuildAsync(string[] args)
    {
        var authorityBaseUrl = Environment.GetEnvironmentVariable("CLINIC_AUTHORITY_BASE_URL") ?? DefaultAuthorityBaseUrl;
        var connectionString = Environment.GetEnvironmentVariable("CLINIC_WORKSTATION_CONNECTION")
                               ?? WorkstationDbConnections.DefaultLocal;

        await WorkstationDbInitializer.InitializeAsync(connectionString);

        var db = WorkstationDbConnections.CreateContext(connectionString);

        var identity = new CurrentIdentity
        {
            UserId = DevelopmentSeed.ReceptionUserId,
            UserName = DevelopmentSeed.ReceptionUsername,
            WorkstationId = DevelopmentSeed.WorkstationId,
            WorkstationName = DevelopmentSeed.WorkstationDisplayName
        };

        var appointmentRepository = new AppointmentRepository(db);
        var patientFlowRepository = new PatientFlowRepository(db);
        var syncOperationRepository = new SyncOperationRepository(db);
        var unitOfWork = new WorkstationUnitOfWork(db);

        var transport = new AuthorityHttpTransport(authorityBaseUrl);
        var useCase = new CheckInAppointmentUseCase(
            appointmentRepository, patientFlowRepository, syncOperationRepository, unitOfWork, identity);
        var listQuery = new AppointmentListQuery(appointmentRepository, syncOperationRepository);
        var syncEngine = new SyncEngine(syncOperationRepository, appointmentRepository, unitOfWork, transport);

        var viewModel = new MainViewModel(useCase, listQuery, syncEngine, identity);
        await viewModel.RefreshAsync();

        return (new AppServices(db, transport), viewModel);
    }
}

/// <summary>Lifetime handle for disposable services owned by the application root.</summary>
public sealed class AppServices : IDisposable
{
    private readonly WorkstationDbContext _db;
    private readonly AuthorityHttpTransport _transport;

    public AppServices(WorkstationDbContext db, AuthorityHttpTransport transport)
    {
        _db = db;
        _transport = transport;
    }

    public void Dispose()
    {
        _transport.Dispose();
        _db.Dispose();
    }
}