using BuildingBlocks.Core.Messaging;
using Identity.Infrastructure;
using Plans.Domain;
using Subscribers.Domain.Entities;

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
            var planOutboxMessageDispatcher = scope.ServiceProvider.GetRequiredService<IOutboxMessageDispatcher<Plan>>();
            var subscriberOutboxMessageDispatcher = scope.ServiceProvider.GetRequiredService<IOutboxMessageDispatcher<Subscriber>>();
            var identityOutboxMessageDispatcher = scope.ServiceProvider.GetRequiredService<IOutboxMessageDispatcher<UserIdentityAggregateRoot>>();

            await Task.WhenAll([
                planOutboxMessageDispatcher.DispatchAsync(MAX_RETRY_PUBLISH_MESSAGE, stoppingToken),
                subscriberOutboxMessageDispatcher.DispatchAsync(MAX_RETRY_PUBLISH_MESSAGE, stoppingToken),
                identityOutboxMessageDispatcher.DispatchAsync(MAX_RETRY_PUBLISH_MESSAGE, stoppingToken)
                ]);

            await Task.Delay(millisecondsDelay: 2000, stoppingToken);
        }
    }
}