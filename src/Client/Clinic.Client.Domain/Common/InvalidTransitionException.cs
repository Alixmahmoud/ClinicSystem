using Clinic.Client.Domain.Common;

namespace Clinic.Client.Domain.Common;

public sealed class InvalidTransitionException : DomainException
{
    public Enum FromState { get; }
    public Enum? ToState { get; }

    public InvalidTransitionException(Enum fromState, Enum? toState, string message)
        : base(message)
    {
        FromState = fromState;
        ToState = toState;
    }
}