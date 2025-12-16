using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Messaging.Dispatchers;

public sealed class MediaRIntegrationEventDispatcher : IIntegrationEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly IntegrationEventTypeMapper _integrationEventTypeMapper;

    public MediaRIntegrationEventDispatcher(
        IMediator mediator,
        IntegrationEventTypeMapper integrationEventTypeMapper)
    {
        _mediator = mediator;
        _integrationEventTypeMapper = integrationEventTypeMapper;
    }

    public async Task DispatchAsync(IntegrationEventEnvelope eventEnvelope, CancellationToken cancellationToken)
    {
        Type integrationEventType = _integrationEventTypeMapper.GetEventTypeByName(eventEnvelope.EventName)
           ?? throw new InvalidOperationException($"Integration event type '{eventEnvelope.EventName}' is not recognized.");

        var @event = (IntegrationEvent?)JsonSerializer.Deserialize(eventEnvelope.Payload, integrationEventType)
            ?? throw new InvalidOperationException($"Deserialization of integration event '{eventEnvelope.EventName}' failed.");

        await _mediator.Publish(@event, cancellationToken);
    }
}