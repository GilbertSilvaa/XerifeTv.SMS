using BuildingBlocks.Core.Events;

namespace BuildingBlocks.Core.Messaging;

public interface IIntegrationEventDispatcher
{
    Task DispatchAsync(IntegrationEventEnvelope eventEnvelope, CancellationToken cancellationToken);
}