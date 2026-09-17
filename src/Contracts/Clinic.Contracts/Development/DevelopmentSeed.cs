namespace Clinic.Contracts.Development;

/// <summary>
/// Fixed dev/test identity and appointment bootstrap values shared by the Authority seed,
/// the workstation seed, and the end-to-end tests. These are slice bootstrap values, not
/// production data, and are deliberately fixed so the two sides can agree without coupling.
/// </summary>
public static class DevelopmentSeed
{
    public static readonly Guid ReceptionUserId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid WorkstationId = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid DoctorId = new("33333333-3333-3333-3333-333333333333");
    public static readonly Guid PatientId = new("44444444-4444-4444-4444-444444444444");
    public static readonly Guid AppointmentId = new("55555555-5555-5555-5555-555555555555");

    public const string ReceptionUsername = "reception";
    public const string WorkstationDisplayName = "FRONT-DESK-1";
}