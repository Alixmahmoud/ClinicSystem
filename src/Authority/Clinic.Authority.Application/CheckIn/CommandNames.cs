namespace Clinic.Authority.Application.CheckIn;

/// <summary>
/// Aggregate type name used in operation envelopes. Not a persistence entity name and
/// not an API version token; it identifies the aggregate that owns the operation.
/// </summary>
public static class AppointmentAggregateType
{
    public const string Value = "Appointment";
}

public static class CheckInAppointmentCommandType
{
    public const string Value = "CheckInAppointment";
}