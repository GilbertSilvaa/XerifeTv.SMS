using BuildingBlocks.Common;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Outbox;

namespace OutboxPublisher.Worker;

public class Worker : BackgroundService
{
    private const int MAX_RETRY_PUBLISH_MESSAGE = 5;
    private readonly IMessageBus _messageBus;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(
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
                    for (int attempt = 0; attempt < MAX_RETRY_PUBLISH_MESSAGE; attempt++)
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
                            break;
                        }
                        catch (Exception)
                        {
                            message.MarkAsFailed();
                            await outboxRepository.AddOrUpdateAsync(message);
                        }

                        await Task.Delay(TimeSpan.FromSeconds(2 * attempt).Milliseconds, stoppingToken);
                    }
                }
            }

            await Task.Delay(millisecondsDelay: 2000, stoppingToken);
        }
    }
}
