using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;

namespace BuildingBlocks.Infrastructure.Messaging;

public sealed class MediaRIntegrationEventPublisher : IIntegrationEventPublisher
{
	private readonly IMediator _mediator;

	public MediaRIntegrationEventPublisher(IMediator mediator)
	{
		_mediator = mediator;
	}

	public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken) where T : IntegrationEvent
		=> await _mediator.Publish(@event, cancellationToken);
}
