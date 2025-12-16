using BuildingBlocks.Common;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Infrastructure.Outbox;

public sealed class OutboxProcessorWorker : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly IServiceScopeFactory _scopeFactory;

    public OutboxProcessorWorker(
        IMessageBus messageBus,
        IServiceScopeFactory scopeFactory)
    {
        _messageBus = messageBus;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

            var messages = await outboxRepository.FetchByStatusAsync(
                status: EOutboxMessageStatus.PENDING,
                take: MessagingConstants.MAX_MESSAGES_PER_BATCH);

            if (messages?.Any() == true)
            {
                foreach (var message in messages)
                {
                    try
                    {
                        message.MarkAsProcessing();
                        await outboxRepository.AddOrUpdateAsync(message);

                        await _messageBus.PublishAsync(
                            message: message.Payload,
                            topic: MessagingConstants.INTEGRATION_EVENTS_TOPIC,
                            key: message.RoutingKey,
                            stoppingToken);

                        message.MarkAsCompleted();
                        await outboxRepository.AddOrUpdateAsync(message);
                    }
                    catch (Exception)
                    {
                        message.MarkAsFailed();
                        await outboxRepository.AddOrUpdateAsync(message);
                    }
                }
            }

            await Task.Delay(millisecondsDelay: 5000, stoppingToken);
        }
    }
}
