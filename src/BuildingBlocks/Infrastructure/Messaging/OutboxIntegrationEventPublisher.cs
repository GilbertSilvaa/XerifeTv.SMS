using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Outbox;

namespace BuildingBlocks.Infrastructure.Messaging;

public sealed class OutboxIntegrationEventPublisher : IIntegrationEventPublisher
{
	private readonly IOutboxRepository _outboxRepository;

	public OutboxIntegrationEventPublisher(IOutboxRepository outboxRepository)
	{
		_outboxRepository = outboxRepository;
	}

	public async Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken) where T : IntegrationEvent
	{
		await _outboxRepository.AddOrUpdateAsync(new(@event, routingKey));
	}
}
