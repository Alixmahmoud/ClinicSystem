using Clinic.Contracts.Operations;

namespace Clinic.Authority.Application.CheckIn;

/// <summary>
/// Authority processing pipeline for the CheckInAppointment command.
/// Receive → Validate Envelope → Authenticate → Authorize → Check Idempotency →
/// Validate Dependencies → Validate BaseVersion / Concurrency → Execute Domain Operation →
/// Persist Transaction → Create Audit → Publish Authoritative Change → Return Result
/// </summary>
public interface ICheckInAppointmentProcessor
{
    Task<OperationResult> ProcessAsync(OperationEnvelope envelope, CancellationToken cancellationToken = default);
}