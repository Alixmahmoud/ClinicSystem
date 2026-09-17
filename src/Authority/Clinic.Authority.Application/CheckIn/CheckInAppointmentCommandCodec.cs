using System.Text.Json;
using Clinic.Contracts.Commands;

namespace Clinic.Authority.Application.CheckIn;

/// <summary>
/// Maps the wire-level command payload (open transport detail) back into the typed
/// boundary command. Malformed or unsupported payloads must be distinguishable from
/// valid payloads (MalformedProtocol vs Validation).
/// </summary>
public static class CheckInAppointmentCommandCodec
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static bool TryDecode(string commandPayloadJson, out CheckInAppointmentCommand command, out string error)
    {
        command = null!;
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(commandPayloadJson))
        {
            error = "Command payload is missing.";
            return false;
        }

        try
        {
            command = JsonSerializer.Deserialize<CheckInAppointmentCommand>(commandPayloadJson, JsonOptions)!;
        }
        catch (JsonException ex)
        {
            error = $"Command payload is not valid JSON for {CheckInAppointmentCommandType.Value}: {ex.Message}";
            return false;
        }

        if (command is null)
        {
            error = "Command payload could not be decoded.";
            return false;
        }

        return true;
    }

    public static string Encode(CheckInAppointmentCommand command) =>
        JsonSerializer.Serialize(command, JsonOptions);
}