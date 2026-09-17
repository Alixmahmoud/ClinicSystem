using Clinic.Authority.Domain.Common;

namespace Clinic.Authority.Domain.Common;

public interface IAggregateRoot
{
    Guid Id { get; }
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
}