using SharedKernel;

namespace BuildingBlocks.Core.Events;

public abstract record IntegrationEvent(double Version = 1.0) : DomainEvent, INotification;