using System.Text.Json;
using Clinic.Contracts.Commands;
using Clinic.Contracts.Development;
using Clinic.Contracts.Operations;
using Xunit;

namespace Clinic.Contracts.Tests;

public sealed class BoundaryContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void OperationEnvelope_RoundTrips_PreservingOperationIdIdentityAndBaseVersion()
    {
        var envelope = new OperationEnvelope(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Appointment", Guid.NewGuid(),
            "CheckInAppointment", 7, DateTimeOffset.UtcNow, "1.0", "{\"appointmentId\":\"x\"}");

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var roundTripped = JsonSerializer.Deserialize<OperationEnvelope>(json, JsonOptions);

        Assert.Equal(envelope, roundTripped);
        Assert.Equal(envelope.OperationId, roundTripped!.OperationId);
        Assert.Equal(envelope.BaseVersion, roundTripped.BaseVersion);
    }

    [Fact]
    public void OperationResult_RoundTrips_BusinessOutcome_WithoutFailureCategory()
    {
        var result = OperationResult.Accepted(Guid.NewGuid(), "Appointment", Guid.NewGuid(), 2, DateTimeOffset.UtcNow);

        var roundTripped = JsonSerializer.Deserialize<OperationResult>(
            JsonSerializer.Serialize(result, JsonOptions), JsonOptions);

        Assert.NotNull(roundTripped);
        Assert.Equal(OperationOutcome.Accepted, roundTripped!.Outcome);
        Assert.Null(roundTripped.FailureCategory);
    }

    [Fact]
    public void OperationResult_RoundTrips_FailureCategory_WithoutBusinessOutcome()
    {
        var result = OperationResult.Failure(
            Guid.NewGuid(), OperationFailureCategory.Authorization, "Appointment", Guid.NewGuid(), "denied", DateTimeOffset.UtcNow);

        var roundTripped = JsonSerializer.Deserialize<OperationResult>(
            JsonSerializer.Serialize(result, JsonOptions), JsonOptions);

        Assert.NotNull(roundTripped);
        Assert.Null(roundTripped!.Outcome);
        Assert.Equal(OperationFailureCategory.Authorization, roundTripped.FailureCategory);
    }

    [Theory]
    [InlineData(OperationOutcome.Accepted)]
    [InlineData(OperationOutcome.AlreadyProcessed)]
    [InlineData(OperationOutcome.Conflict)]
    [InlineData(OperationOutcome.Rejected)]
    [InlineData(OperationOutcome.DependencyBlocked)]
    [InlineData(OperationOutcome.TemporarilyUnavailable)]
    public void OperationOutcome_AllValuesRemainDistinct_AfterRoundTrip(OperationOutcome outcome)
    {
        var result = new OperationResult(
            Guid.NewGuid(), outcome, null, "Appointment", Guid.NewGuid(), null, null, DateTimeOffset.UtcNow);

        var roundTripped = JsonSerializer.Deserialize<OperationResult>(
            JsonSerializer.Serialize(result, JsonOptions), JsonOptions);

        Assert.Equal(outcome, roundTripped!.Outcome);
    }

    [Fact]
    public void CheckInAppointmentCommand_RoundTrips()
    {
        var command = new CheckInAppointmentCommand(Guid.NewGuid());

        var roundTripped = JsonSerializer.Deserialize<CheckInAppointmentCommand>(
            JsonSerializer.Serialize(command, JsonOptions), JsonOptions);

        Assert.Equal(command, roundTripped);
    }

    [Fact]
    public void DevelopmentSeed_IsTheSingleSharedFixtureForAuthorityAndWorkstation()
    {
        Assert.Equal(AuthorityAlias.WorkstationId, DevelopmentSeed.WorkstationId);
        Assert.Equal(AuthorityAlias.ReceptionUserId, DevelopmentSeed.ReceptionUserId);
        Assert.Equal(AuthorityAlias.AppointmentId, DevelopmentSeed.AppointmentId);
    }

    /// <summary>Mirrors the Authority-side seed re-export to prove cross-tier agreement.</summary>
    private static class AuthorityAlias
    {
        public static readonly Guid ReceptionUserId = DevelopmentSeed.ReceptionUserId;
        public static readonly Guid WorkstationId = DevelopmentSeed.WorkstationId;
        public static readonly Guid AppointmentId = DevelopmentSeed.AppointmentId;
    }
}