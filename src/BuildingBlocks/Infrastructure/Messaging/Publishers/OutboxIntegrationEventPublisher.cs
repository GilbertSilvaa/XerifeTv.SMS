using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Outbox;

namespace BuildingBlocks.Infrastructure.Messaging.Publishers;

public sealed class OutboxIntegrationEventPublisher : IIntegrationEventPublisher
{
	private readonly IOutboxRepository _outboxRepository;

	public OutboxIntegrationEventPublisher(IOutboxRepository outboxRepository)
	{
		_outboxRepository = outboxRepository;
	}

	public async Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken) where T : IntegrationEvent
	{
		var outboxMessage = OutboxMessage.Create(@event, routingKey);
        await _outboxRepository.AddOrUpdateAsync(outboxMessage);
	}
}
