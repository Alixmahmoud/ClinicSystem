using Clinic.Authority.Application.CheckIn;
using Clinic.Authority.Domain.Abstractions;
using Clinic.Authority.Domain.Security;
using Clinic.Authority.Infrastructure;
using Clinic.Authority.Infrastructure.Persistence;
using Clinic.Authority.Infrastructure.Persistence.Repositories;
using Clinic.Authority.Infrastructure.Persistence.Security;
using Clinic.Contracts.Operations;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Authority.Api;

public static class Program
{
    public static WebApplication BuildApp(string[] args, string? authorityConnection = null)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder, authorityConnection);
        var app = builder.Build();
        MapEndpoints(app);
        return app;
    }

    public static async Task RunDevelopmentAsync(string[] args)
    {
        var app = BuildApp(args, authorityConnection: null);
        var connectionString = app.Services.GetRequiredService<AuthorityConnectionSettings>().ConnectionString;
        await AuthorityDbInitializer.MigrateAsync(connectionString);
        await AuthorityDbInitializer.SeedAsync(connectionString);
        await app.RunAsync();
    }

    public static async Task Main(string[] args) => await RunDevelopmentAsync(args);

    private static void ConfigureServices(WebApplicationBuilder builder, string? authorityConnection)
    {
        var connectionString = authorityConnection
                               ?? builder.Configuration["AuthorityConnection"]
                               ?? AuthorityDbConnections.Default;

        builder.Services.AddSingleton(new AuthorityConnectionSettings(connectionString));
        builder.Services.AddDbContext<AuthorityDbContext>(options => options.UseNpgsql(connectionString));

        builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        builder.Services.AddScoped<IPatientFlowRepository, PatientFlowRepository>();
        builder.Services.AddScoped<IProcessedOperationRepository, ProcessedOperationRepository>();
        builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        builder.Services.AddScoped<IAuthoritativeChangeRepository, AuthoritativeChangeRepository>();
        builder.Services.AddScoped<IUnitOfWork, AuthorityUnitOfWork>();
        builder.Services.AddScoped<IUserAccountStore, UserAccountStore>();
        builder.Services.AddScoped<IWorkstationTrustStore, WorkstationTrustStore>();
        builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
        builder.Services.AddScoped<ICheckInAppointmentProcessor, CheckInAppointmentProcessor>();
        builder.Services.AddScoped<IOperationRecoveryService, OperationRecoveryService>();
    }

    private static void MapEndpoints(WebApplication app)
    {
        // Business outcomes are returned as HTTP 200 with the semantic outcome in the body.
        // Transport status is distinct from business outcome (frozen distinction).
        app.MapPost("/api/operations/submit", async (
            OperationEnvelope envelope,
            ICheckInAppointmentProcessor processor,
            CancellationToken ct) =>
        {
            var result = await processor.ProcessAsync(envelope, ct);
            return result.FailureCategory switch
            {
                OperationFailureCategory.Authentication => Results.Json(result, statusCode: StatusCodes.Status401Unauthorized),
                OperationFailureCategory.Authorization => Results.Json(result, statusCode: StatusCodes.Status403Forbidden),
                OperationFailureCategory.Validation => Results.Json(result, statusCode: StatusCodes.Status422UnprocessableEntity),
                OperationFailureCategory.MalformedProtocol => Results.Json(result, statusCode: StatusCodes.Status400BadRequest),
                OperationFailureCategory.Network => Results.Json(result, statusCode: StatusCodes.Status502BadGateway),
                OperationFailureCategory.TemporarilyUnavailable => Results.Json(result, statusCode: StatusCodes.Status503ServiceUnavailable),
                _ => Results.Json(result, statusCode: StatusCodes.Status200OK)
            };
        });

        // Unknown-outcome recovery by exact OperationId identity (OperationId is not a correlation id).
        app.MapGet("/api/operations/{workstationId:guid}/{operationId:guid}", async (
            Guid workstationId,
            Guid operationId,
            IOperationRecoveryService recovery,
            CancellationToken ct) =>
        {
            var result = await recovery.GetResultByOperationIdAsync(workstationId, operationId, ct);
            if (result is null)
            {
                return Results.Json(
                    new OperationResult(
                        operationId,
                        null,
                        OperationFailureCategory.Network,
                        string.Empty,
                        Guid.Empty,
                        null,
                        "No definitive result has been recorded for this OperationId.",
                        DateTimeOffset.UtcNow),
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Json(result, statusCode: StatusCodes.Status200OK);
        });
    }
}

/// <summary>Singleton carrying the resolved Authority connection string for startup work.</summary>
public sealed record AuthorityConnectionSettings(string ConnectionString);