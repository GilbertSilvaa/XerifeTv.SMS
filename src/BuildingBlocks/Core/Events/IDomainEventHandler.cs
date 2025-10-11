using SharedKernel;

namespace BuildingBlocks.Core.Events;

public interface IDomainEventHandler<TDomainEvent>
	: INotificationHandler<DomainEventNotification<TDomainEvent>>
	where TDomainEvent : DomainEvent;
