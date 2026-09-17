using Clinic.Authority.Domain.Appointments;
using Clinic.Authority.Domain.Common;
using Xunit;

namespace Clinic.Authority.Domain.Tests;

public sealed class AppointmentTests
{
    private static Appointment NewScheduled(long version = 1) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(1),
            AppointmentStatus.Scheduled,
            version);

    [Fact]
    public void CheckIn_FromScheduled_TransitionsToCheckedIn_IncrementsVersion_AndPublishesEvent()
    {
        var appointment = NewScheduled();

        appointment.CheckIn();

        Assert.Equal(AppointmentStatus.CheckedIn, appointment.Status);
        Assert.Equal(2, appointment.Version);
        var domainEvent = Assert.Single(appointment.DomainEvents);
        Assert.IsType<PatientCheckedInEvent>(domainEvent);
    }

    [Fact]
    public void CheckIn_WhenAlreadyCheckedIn_ThrowsInvalidTransition()
    {
        var appointment = NewScheduled();
        appointment.CheckIn();

        Assert.Throws<InvalidTransitionException>(() => appointment.CheckIn());
    }

    [Fact]
    public void CheckIn_WhenCancelled_ThrowsInvalidTransition_AndLeavesStateUnchanged()
    {
        var appointment = new Appointment(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, AppointmentStatus.Cancelled, 1);

        Assert.Throws<InvalidTransitionException>(() => appointment.CheckIn());
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        Assert.Equal(1, appointment.Version);
    }
}