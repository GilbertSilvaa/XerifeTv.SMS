using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using SharedKernel;

namespace BuildingBlocks.Infrastructure.Messaging;

public sealed class MediaRDomainEventPublisher : IDomainEventPublisher
{
	private readonly IMediator _mediator;

	public MediaRDomainEventPublisher(IMediator mediator)
	{
		_mediator = mediator;
	}

	public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken) where T : DomainEvent
	{
		var notificationType = typeof(DomainEventNotification<>).MakeGenericType(@event.GetType());

		if (Activator.CreateInstance(notificationType, @event) is INotification notification)
			await _mediator.Publish(notification, cancellationToken);
	}
}