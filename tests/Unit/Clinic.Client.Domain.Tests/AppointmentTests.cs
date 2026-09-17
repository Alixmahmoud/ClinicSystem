using Clinic.Client.Domain.Appointments;
using Clinic.Client.Domain.Common;
using Xunit;

namespace Clinic.Client.Domain.Tests;

public sealed class AppointmentTests
{
    private static Appointment NewScheduled(long version = 1, long? confirmedVersion = 1) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(1),
            AppointmentStatus.Scheduled,
            version,
            confirmedVersion);

    [Fact]
    public void CheckIn_FromScheduled_TransitionsToCheckedIn_AndIncrementsVersion()
    {
        var appointment = NewScheduled();

        appointment.CheckIn();

        Assert.Equal(AppointmentStatus.CheckedIn, appointment.Status);
        Assert.Equal(2, appointment.Version);
    }

    [Fact]
    public void CheckIn_FromCheckedIn_ThrowsInvalidTransition()
    {
        var appointment = NewScheduled();
        appointment.CheckIn();

        var exception = Assert.Throws<InvalidTransitionException>(() => appointment.CheckIn());

        Assert.Equal(AppointmentStatus.Scheduled, (AppointmentStatus)exception.FromState);
        Assert.Equal(AppointmentStatus.CheckedIn, (AppointmentStatus)exception.ToState);
    }

    [Fact]
    public void CheckIn_FromCancelled_ThrowsInvalidTransition()
    {
        var appointment = new Appointment(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, AppointmentStatus.Cancelled, 1, null);

        Assert.Throws<InvalidTransitionException>(() => appointment.CheckIn());
    }

    [Fact]
    public void ConfirmVersion_RecordsAuthoritativeVersion_AndRaisesLocalVersionWhenAuthorityIsAhead()
    {
        var appointment = NewScheduled();
        appointment.CheckIn();

        appointment.ConfirmVersion(5);

        Assert.Equal(5, appointment.ConfirmedVersion);
        Assert.Equal(5, appointment.Version);
    }
}