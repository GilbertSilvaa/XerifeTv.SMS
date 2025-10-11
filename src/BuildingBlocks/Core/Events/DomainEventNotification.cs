using SharedKernel;

namespace BuildingBlocks.Core.Events;

public record DomainEventNotification<T>(T DomainEvent) : INotification where T : DomainEvent;