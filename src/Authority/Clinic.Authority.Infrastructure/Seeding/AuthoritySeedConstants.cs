using Clinic.Contracts.Development;

namespace Clinic.Authority.Infrastructure.Seeding;

/// <summary>
/// Re-export of the fixed dev/test seed values (see Clinic.Contracts.Development.DevelopmentSeed
/// for the single source shared with the workstation and end-to-end tests).
/// </summary>
public static class AuthoritySeedConstants
{
    public static readonly Guid ReceptionUserId = DevelopmentSeed.ReceptionUserId;
    public static readonly Guid WorkstationId = DevelopmentSeed.WorkstationId;
    public static readonly Guid DoctorId = DevelopmentSeed.DoctorId;
    public static readonly Guid PatientId = DevelopmentSeed.PatientId;
    public static readonly Guid AppointmentId = DevelopmentSeed.AppointmentId;

    public const string ReceptionUsername = DevelopmentSeed.ReceptionUsername;
    public const string WorkstationDisplayName = DevelopmentSeed.WorkstationDisplayName;
}