using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Outbox;
using SharedKernel;

namespace BuildingBlocks.Infrastructure.Messaging.Publishers;

public sealed class OutboxIntegrationEventPublisher<TAggregateRoot> 
	: IIntegrationEventPublisher<TAggregateRoot> 
	where TAggregateRoot : AggregateRoot
{
	private readonly IOutboxRepository<TAggregateRoot> _outboxRepository;

	public OutboxIntegrationEventPublisher(IOutboxRepository<TAggregateRoot> outboxRepository)
	{
		_outboxRepository = outboxRepository;
	}

	public async Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken) where T : IntegrationEvent
	{
        await _outboxRepository.AddOrUpdateAsync(OutboxMessage.Create(@event, routingKey));
	}
}
	