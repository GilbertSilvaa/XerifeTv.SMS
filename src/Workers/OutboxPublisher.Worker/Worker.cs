using BuildingBlocks.Core.Messaging;

namespace OutboxPublisher.Worker;

public class Worker : BackgroundService
{
    private const int MAX_RETRY_PUBLISH_MESSAGE = 5;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var outboxMessageDispatcher = scope.ServiceProvider.GetRequiredService<IOutboxMessageDispatcher>();

            await outboxMessageDispatcher.DispatchAsync(MAX_RETRY_PUBLISH_MESSAGE, stoppingToken);

            await Task.Delay(millisecondsDelay: 2000, stoppingToken);
        }
    }
}