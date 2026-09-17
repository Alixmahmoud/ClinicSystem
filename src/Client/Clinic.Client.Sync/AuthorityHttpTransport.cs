using System.Net.Http.Json;
using Clinic.Contracts.Operations;

namespace Clinic.Client.Sync;

/// <summary>
/// HTTP transport to the Clinic Authority. Maps only transport status: a definitive reply
/// that carries a business outcome (HTTP 200 with OperationResult body, or the definitive
/// failure categories returned as 4xx/5xx) versus requests that never got a definitive reply.
/// </summary>
public sealed class AuthorityHttpTransport : ISyncTransport, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsClient;

    public AuthorityHttpTransport(HttpClient httpClient, bool ownsClient = false)
    {
        _httpClient = httpClient;
        _ownsClient = ownsClient;
    }

    public AuthorityHttpTransport(string baseUrl)
        : this(new HttpClient { BaseAddress = Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ? uri : new Uri(baseUrl) }, ownsClient: true)
    {
    }

    public async Task<SyncSubmitTransportResult> SubmitAsync(OperationEnvelope envelope, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync("api/operations/submit", envelope, cancellationToken);
            if (response.IsSuccessStatusCode || IsDefinitiveErrorStatus(response))
            {
                var result = await response.Content.ReadFromJsonAsync<OperationResult>(cancellationToken);
                if (result is not null)
                {
                    return SyncTransportResults.Definite(result);
                }
                return SyncTransportResults.NonRetryableFailure();
            }

            return SyncTransportResults.TransientFailure();
        }
        catch (HttpRequestException)
        {
            return SyncTransportResults.TransientFailure();
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout (no server response) — the Authority may still have processed the operation.
            return SyncTransportResults.TransientFailure();
        }
    }

    public async Task<SyncSubmitTransportResult> GetResultAsync(Guid workstationId, Guid operationId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"api/operations/{workstationId:N}/{operationId:N}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<OperationResult>(cancellationToken);
                return SyncTransportResults.Definite(result);
            }

            if ((int)response.StatusCode == StatusCodes.Status404NotFound)
            {
                // Definitive: the Authority has no record for this OperationId → safe to re-submit it.
                return SyncTransportResults.Definite(null);
            }

            return SyncTransportResults.TransientFailure();
        }
        catch (HttpRequestException)
        {
            return SyncTransportResults.TransientFailure();
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return SyncTransportResults.TransientFailure();
        }
    }

    public void Dispose() => _httpClient.Dispose();

    private static bool IsDefinitiveErrorStatus(HttpResponseMessage response) =>
        (int)response.StatusCode is StatusCodes.Status400BadRequest
            or StatusCodes.Status401Unauthorized
            or StatusCodes.Status403Forbidden
            or StatusCodes.Status422UnprocessableEntity
            or StatusCodes.Status502BadGateway
            or StatusCodes.Status503ServiceUnavailable;
}

internal static class StatusCodes
{
    public const int Status400BadRequest = 400;
    public const int Status401Unauthorized = 401;
    public const int Status403Forbidden = 403;
    public const int Status404NotFound = 404;
    public const int Status422UnprocessableEntity = 422;
    public const int Status502BadGateway = 502;
    public const int Status503ServiceUnavailable = 503;
}