namespace BuildingBlocks.Core.Events;

public interface IIntegrationEventHandler<TDomainEvent>
	: INotificationHandler<TDomainEvent>
	where TDomainEvent : IntegrationEvent;