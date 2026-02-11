using BuildingBlocks.Common;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Infrastructure.Messaging.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;
using ICoreMessageBus = BuildingBlocks.Core.Messaging.IMessageBus;
using IWolverineBus = Wolverine.IMessageBus;

namespace BuildingBlocks.Infrastructure.Messaging.Buses;

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

        await bus.PublishAsync(integrationEventEnvelope, new DeliveryOptions
        {
            Headers = { ["topic"] = topic, ["key"] = key ?? string.Empty }
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
        IConfiguration configuration)
    {
        builder.UseWolverine(opts =>
        {
            opts.UseRabbitMq(rabbit =>
            {
                rabbit.HostName = configuration["RabbitMQ:Host"]!;
                rabbit.Port = int.Parse(configuration["RabbitMQ:Port"]!);
                rabbit.UserName = configuration["RabbitMQ:Username"]!;
                rabbit.Password = configuration["RabbitMQ:Password"]!;
                rabbit.VirtualHost = configuration["RabbitMQ:VHost"]!;
            })
            .DeclareExchange(MessagingConstants.INTEGRATION_EVENTS_TOPIC, ex =>
            {
                ex.ExchangeType = ExchangeType.Topic;
                ex.BindQueue(MessagingConstants.INTEGRATION_EVENTS_TOPIC, "#");
            })
            .AutoProvision();

            opts.PublishAllMessages()
                .ToRabbitExchange(MessagingConstants.INTEGRATION_EVENTS_TOPIC);
        });
    }

    public static void AddWolverineRabbitMQConsumerConfiguration(
        this IHostApplicationBuilder builder,
        IConfiguration configuration)
    {
        builder.UseWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(IntegrationEventEnvelopeHandler).Assembly);

            opts.UseRabbitMq(rabbit =>
            {
                rabbit.HostName = configuration["RabbitMQ:Host"]!;
                rabbit.Port = int.Parse(configuration["RabbitMQ:Port"]!);
                rabbit.UserName = configuration["RabbitMQ:Username"]!;
                rabbit.Password = configuration["RabbitMQ:Password"]!;
                rabbit.VirtualHost = configuration["RabbitMQ:VHost"]!;
            })
            .DeclareExchange(MessagingConstants.INTEGRATION_EVENTS_TOPIC, ex =>
            {
                ex.ExchangeType = ExchangeType.Topic;
                ex.BindQueue(MessagingConstants.INTEGRATION_EVENTS_TOPIC, "#");
            })
            .AutoProvision();

            opts.ListenToRabbitQueue(MessagingConstants.INTEGRATION_EVENTS_TOPIC)
                .UseDurableInbox()
                .PreFetchCount((ushort)MessagingConstants.MAX_MESSAGES_PER_BATCH)
                .ListenerCount(3);

            opts.Policies.OnException<Exception>()
                .RetryWithCooldown(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15));
        });
    }
}