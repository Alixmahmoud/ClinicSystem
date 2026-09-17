namespace Clinic.Client.Application.CheckIn;

/// <summary>
/// Aggregate type name used in operation envelopes. Not a persistence entity name and
/// not an API version token. Must match the Authority's value exactly.
/// </summary>
public static class AppointmentAggregateType
{
    public const string Value = "Appointment";
}

public static class CheckInAppointmentCommandName
{
    public const string Value = "CheckInAppointment";
}