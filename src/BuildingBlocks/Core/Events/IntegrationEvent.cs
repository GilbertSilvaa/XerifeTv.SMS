using SharedKernel;

namespace BuildingBlocks.Core.Events;

public abstract record IntegrationEvent : DomainEvent, INotification;