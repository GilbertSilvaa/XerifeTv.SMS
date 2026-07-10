using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Messaging.Consumers;

public sealed class IntegrationEventEnvelopeHandler : INotificationHandler<IntegrationEventEnvelope>
{
    private readonly IMediator _mediator;
    private readonly IntegrationEventTypeMapper _integrationEventTypeMapper;

    public IntegrationEventEnvelopeHandler(
        IMediator mediator,
        IntegrationEventTypeMapper integrationEventTypeMapper)
    {
        _mediator = mediator;
        _integrationEventTypeMapper = integrationEventTypeMapper;
    }

    public async Task Handle(IntegrationEventEnvelope eventEnvelope, CancellationToken cancellationToken)
    {
        try
        {
            Type integrationEventType = _integrationEventTypeMapper.GetEventTypeByName(eventEnvelope.EventName)
                ?? throw new InvalidOperationException($"Integration event type '{eventEnvelope.EventName}' is not recognized.");

            var @event = (IntegrationEvent)JsonSerializer.Deserialize(eventEnvelope.Payload, integrationEventType)!;
            @event.SetEventId(eventEnvelope.EventId);

            await _mediator.Publish(@event, cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Deserialization of integration event '{eventEnvelope.EventName}' failed.", ex);
        }
        catch (Exception ex) when ((ex is not InvalidOperationException))
        {
            throw new InvalidOperationException($"Failed to dispatch integration event '{eventEnvelope.EventName}'.", ex);
        }
    }
}