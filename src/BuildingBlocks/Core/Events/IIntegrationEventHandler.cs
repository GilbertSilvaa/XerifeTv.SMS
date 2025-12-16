namespace BuildingBlocks.Core.Events;

public interface IIntegrationEventHandler<TIntegrationEvent>
    : INotificationHandler<TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent;