using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;

namespace BuildingBlocks.Infrastructure.Messaging.Dispatchers;

public sealed class MediaRIntegrationEventDispatcher : IIntegrationEventDispatcher
{
    private readonly IMediator _mediator;

    public MediaRIntegrationEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAsync(IntegrationEventEnvelope eventEnvelope, CancellationToken cancellationToken)
    {
        await _mediator.Publish(eventEnvelope, cancellationToken);
    }
}