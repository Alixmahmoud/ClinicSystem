using Clinic.Authority.Domain.Common;

namespace Clinic.Authority.Domain.Common;

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