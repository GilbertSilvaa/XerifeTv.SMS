using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.IntegrationEvents;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Messaging.Consumers;

public sealed class IntegrationEventEnvelopeHandler : INotificationHandler<IntegrationEventEnvelope>
{
    private readonly IMediator _mediator;
    private readonly IntegrationEventTypeMapper _integrationEventTypeMapper;
    private readonly IInboxRepository _inboxRepository;

    public IntegrationEventEnvelopeHandler(
        IMediator mediator,
        IntegrationEventTypeMapper integrationEventTypeMapper,
        IInboxRepository inboxRepository)
    {
        _mediator = mediator;
        _integrationEventTypeMapper = integrationEventTypeMapper;
        _inboxRepository = inboxRepository;
    }

    public async Task Handle(IntegrationEventEnvelope eventEnvelope, CancellationToken cancellationToken)
    {
        try
        {
            await _inboxRepository.AddAsync(InboxMessage.Create(eventEnvelope.EventId, eventEnvelope.EventType));

            Type integrationEventType = _integrationEventTypeMapper.GetEventTypeByName(eventEnvelope.EventName)
                ?? throw new InvalidOperationException($"Integration event type '{eventEnvelope.EventName}' is not recognized.");

            var @event = (IntegrationEvent)JsonSerializer.Deserialize(eventEnvelope.Payload, integrationEventType)!;

            await _mediator.Publish(@event, cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            return;
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
