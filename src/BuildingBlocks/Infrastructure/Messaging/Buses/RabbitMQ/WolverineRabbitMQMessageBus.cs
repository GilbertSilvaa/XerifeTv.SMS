using BuildingBlocks.Common;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Infrastructure.Messaging.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;
using ICoreMessageBus = BuildingBlocks.Core.Messaging.IMessageBus;
using IWolverineBus = Wolverine.IMessageBus;

namespace BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;

public sealed class WolverineRabbitMQMessageBus : ICoreMessageBus
{
    private readonly IServiceScopeFactory _factory;

    public WolverineRabbitMQMessageBus(IServiceScopeFactory factory)
    {
        _factory = factory;
    }

    public async Task PublishAsync(string message, string topic, string? key = null, CancellationToken cancellationToken = default)
    {
        using var scope = _factory.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IWolverineBus>();

        var integrationEventEnvelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(message);

        await bus.BroadcastToTopicAsync(key!, integrationEventEnvelope!, new DeliveryOptions
        {
            Headers = { ["topic"] = topic, ["key"] = key ?? string.Empty },  
        });
    }

    public Task SubscribeAsync(string topic, Func<IntegrationEventEnvelope, Task> handler, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public static class WolverineRabbitMQMessageBusExtensions
{
    public static void AddWolverineRabbitMQPublisherConfiguration(
        this IHostApplicationBuilder builder,
        RabbitMQConnectionOptions configuration)
    {
        builder.UseWolverine(opts =>
        {
            opts.UseRabbitMq(rabbit =>
            {
                rabbit.HostName = configuration.Host;
                rabbit.Port = configuration.Port;
                rabbit.UserName = configuration.UserName;
                rabbit.Password = configuration.Password;
                rabbit.VirtualHost = configuration.VirtualHost;
            })
            .CustomizeDeadLetterQueueing(new($"{MessagingConstants.INTEGRATION_EVENTS_TOPIC}.dead"))
            .AutoProvision();

            opts.PublishAllMessages()
                .ToRabbitTopics(MessagingConstants.INTEGRATION_EVENTS_TOPIC, exchange =>
                {
                    exchange.BindTopic($"{MessagingConstants.INTEGRATION_EVENTS_TOPIC}.identity.#")
                            .ToQueue($"identity.{MessagingConstants.INTEGRATION_EVENTS_TOPIC}");

                    exchange.BindTopic($"{MessagingConstants.INTEGRATION_EVENTS_TOPIC}.plan.#")
                            .ToQueue($"plan.{MessagingConstants.INTEGRATION_EVENTS_TOPIC}");

                    exchange.BindTopic($"{MessagingConstants.INTEGRATION_EVENTS_TOPIC}.subscriber.#")
                            .ToQueue($"subscriber.{MessagingConstants.INTEGRATION_EVENTS_TOPIC}");
                });
        });
    }

    public static void AddWolverineRabbitMQConsumerConfiguration(
        this IHostApplicationBuilder builder,
        RabbitMQConnectionOptions configuration,
        int maxAttempsRetry = 4)
    {
        builder.UseWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(IntegrationEventEnvelopeHandler).Assembly);

            opts.UseRabbitMq(rabbit =>
            {
                rabbit.HostName = configuration.Host;
                rabbit.Port = configuration.Port;
                rabbit.UserName = configuration.UserName;
                rabbit.Password = configuration.Password;
                rabbit.VirtualHost = configuration.VirtualHost;
            })
            .DeclareExchange(MessagingConstants.INTEGRATION_EVENTS_TOPIC, ex =>
            {
                ex.ExchangeType = ExchangeType.Topic;
                ex.BindQueue($"identity.{MessagingConstants.INTEGRATION_EVENTS_TOPIC}", $"{MessagingConstants.INTEGRATION_EVENTS_TOPIC}.identity.#");
                ex.BindQueue($"plan.{MessagingConstants.INTEGRATION_EVENTS_TOPIC}", $"{MessagingConstants.INTEGRATION_EVENTS_TOPIC}.plan.#");
                ex.BindQueue($"subscriber.{MessagingConstants.INTEGRATION_EVENTS_TOPIC}", $"{MessagingConstants.INTEGRATION_EVENTS_TOPIC}.subscriber.#");
            })
            .AutoProvision();

            opts.ListenToRabbitQueue($"identity.{MessagingConstants.INTEGRATION_EVENTS_TOPIC}")
                .PreFetchCount((ushort)MessagingConstants.MAX_MESSAGES_PER_BATCH)
                .ListenerCount(3);

            opts.ListenToRabbitQueue($"plan.{MessagingConstants.INTEGRATION_EVENTS_TOPIC}")
                .PreFetchCount((ushort)MessagingConstants.MAX_MESSAGES_PER_BATCH)
                .ListenerCount(3);

            opts.ListenToRabbitQueue($"subscriber.{MessagingConstants.INTEGRATION_EVENTS_TOPIC}")
                .PreFetchCount((ushort)MessagingConstants.MAX_MESSAGES_PER_BATCH)
                .ListenerCount(3);

            List<TimeSpan> retryDelay = [];

            foreach (int attempt in Enumerable.Range(1, maxAttempsRetry))
                retryDelay.Add(TimeSpan.FromSeconds(Math.Pow(5, attempt)));

            opts.Policies.OnException<Exception>()
                .RetryWithCooldown([.. retryDelay]);
        });
    }
}