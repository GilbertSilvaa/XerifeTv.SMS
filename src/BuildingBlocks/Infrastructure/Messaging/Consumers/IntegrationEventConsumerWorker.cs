using BuildingBlocks.Common;
using BuildingBlocks.Core.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Infrastructure.Messaging.Consumers;

public sealed class IntegrationEventConsumerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessageBus _messageBus;

    public IntegrationEventConsumerWorker(
        IMessageBus messageBus,
        IServiceScopeFactory scopeFactory)
    {
        _messageBus = messageBus;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {    
        await _messageBus.SubscribeAsync(
            topic: MessagingConstants.INTEGRATION_EVENTS_TOPIC,
            handler: async (eventEnvelope) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var integrationEventDispatcher = scope.ServiceProvider.GetRequiredService<IIntegrationEventDispatcher>();

                await integrationEventDispatcher.DispatchAsync(eventEnvelope, stoppingToken);
            },
            cancellationToken: stoppingToken);
    }
}
