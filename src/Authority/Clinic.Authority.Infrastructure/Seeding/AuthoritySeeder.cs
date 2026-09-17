using Clinic.Authority.Domain.Appointments;
using Clinic.Authority.Domain.Security;
using Clinic.Authority.Infrastructure.Persistence;
using Clinic.Authority.Infrastructure.Persistence.Security;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Authority.Infrastructure.Seeding;

/// <summary>
/// Idempotent bootstrap seed for the slice: a reception user, a trusted workstation, and
/// one scheduled appointment. Creating the appointment is a seed/bootstrap activity, not a
/// CheckInAppointment bypass; the check-in command still validates the Scheduled state.
/// CreateAppointment is a separate command outside this slice.
/// </summary>
public static class AuthoritySeeder
{
    public static async Task SeedAsync(AuthorityDbContext db, CancellationToken cancellationToken = default)
    {
        var seededAt = DateTimeOffset.UtcNow;

        if (!await db.Users.AnyAsync(u => u.Id == AuthoritySeedConstants.ReceptionUserId, cancellationToken))
        {
            db.Users.Add(new UserAccountRecord(
                AuthoritySeedConstants.ReceptionUserId,
                AuthoritySeedConstants.ReceptionUsername,
                "Front Desk Reception",
                UserRole.Reception,
                isActive: true));
        }

        if (!await db.Workstations.AnyAsync(w => w.Id == AuthoritySeedConstants.WorkstationId, cancellationToken))
        {
            db.Workstations.Add(new WorkstationRecord(
                AuthoritySeedConstants.WorkstationId,
                AuthoritySeedConstants.WorkstationDisplayName,
                WorkstationTrustState.Trusted));
        }

        if (!await db.Appointments.AnyAsync(a => a.Id == AuthoritySeedConstants.AppointmentId, cancellationToken))
        {
            db.Appointments.Add(new Appointment(
                AuthoritySeedConstants.AppointmentId,
                AuthoritySeedConstants.PatientId,
                AuthoritySeedConstants.DoctorId,
                scheduledAtUtc: seededAt.AddHours(1),
                AppointmentStatus.Scheduled,
                version: 1));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}