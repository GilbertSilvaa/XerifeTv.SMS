using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.IntegrationEvents;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Messaging.Dispatchers;

public sealed class MediaRIntegrationEventDispatcher : IIntegrationEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly IntegrationEventTypeMapper _integrationEventTypeMapper;
    private readonly IInboxRepository _inboxRepository;

    public MediaRIntegrationEventDispatcher(
        IMediator mediator,
        IntegrationEventTypeMapper integrationEventTypeMapper,
        IInboxRepository inboxRepository)
    {
        _mediator = mediator;
        _integrationEventTypeMapper = integrationEventTypeMapper;
        _inboxRepository = inboxRepository;
    }

    public async Task DispatchAsync(IntegrationEventEnvelope eventEnvelope, CancellationToken cancellationToken)
    {
        try
        {
            await _inboxRepository.AddAsync(InboxMessage.Create(eventEnvelope.EventId, eventEnvelope.EventType));

            Type integrationEventType = _integrationEventTypeMapper.GetEventTypeByName(eventEnvelope.EventName)
                ?? throw new InvalidOperationException($"Integration event type '{eventEnvelope.EventName}' is not recognized.");

            var @event = (IntegrationEvent?)JsonSerializer.Deserialize(eventEnvelope.Payload, integrationEventType)
                ?? throw new InvalidOperationException($"Deserialization of integration event '{eventEnvelope.EventName}' failed.");

            await _mediator.Publish(@event, cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to dispatch integration event '{eventEnvelope.EventName}'.", ex);
        }
    }
}