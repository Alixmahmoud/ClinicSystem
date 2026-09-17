using System.Text.Json;
using Clinic.Contracts.Commands;

namespace Clinic.Client.Application.CheckIn;

/// <summary>
/// Encodes the boundary command to the wire payload. The Authority decodes with the exact
/// same conventions, so payload validity here determines which failure category it may hit
/// there (MalformedProtocol vs Validation is never collapsed).
/// </summary>
public static class CheckInAppointmentCommandCodec
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string Encode(CheckInAppointmentCommand command) =>
        JsonSerializer.Serialize(command, JsonOptions);
}