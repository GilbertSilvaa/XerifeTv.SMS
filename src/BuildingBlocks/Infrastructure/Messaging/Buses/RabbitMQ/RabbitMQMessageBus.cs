using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;

public sealed class RabbitMQMessageBus : IMessageBus
{
    private const string DEFAULT_EXCHANGE_TYPE = "topic";
    private const int MAX_ATTEMPTS_PROCESS_MESSAGE = 5;
    private readonly RabbitMqConnectionProvider _provider;

    public RabbitMQMessageBus(RabbitMqConnectionProvider provider)
    {
        _provider = provider;
    }

    public async Task PublishAsync(string message, string topic, string? key = null, CancellationToken cancellationToken = default)
    {
        IChannel channel = await _provider.GetChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: topic,
            type: DEFAULT_EXCHANGE_TYPE,
            durable: true,
            cancellationToken: cancellationToken);

        BasicProperties props = new()
        {
            Persistent = true,
            ContentType = "application/json"
        };

        byte[] messageBody = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: topic,
            routingKey: key ?? string.Empty,
            mandatory: false,
            basicProperties: props,
            body: messageBody,
            cancellationToken: cancellationToken);
    }

    public async Task SubscribeAsync(string topic, Func<IntegrationEventEnvelope, Task> handler, CancellationToken cancellationToken = default)
    {
        IChannel channel = await _provider.GetChannelAsync();

        _ = await DeclareDeadQueueAsync(channel, topic, cancellationToken);
        var queue = await DeclareMainQueueAsync(channel, topic, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            int attempts = 0;

            if (ea.BasicProperties?.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-attempts", out var raw))
            {
                if (raw is byte[] bytes && int.TryParse(Encoding.UTF8.GetString(bytes), out var parsed))
                    attempts = parsed;
            }

            try
            {
                var messageJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<IntegrationEventEnvelope>(messageJson);

                if (message != null)
                    await handler(message);

                await channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                if (attempts + 1 >= MAX_ATTEMPTS_PROCESS_MESSAGE)
                {
                    await channel.BasicRejectAsync(
                        deliveryTag: ea.DeliveryTag,
                        requeue: false,
                        cancellationToken);

                    return;
                }

                BasicProperties props = new()
                {
                    Persistent = true,
                    ContentType = "application/json",
                    Headers = new Dictionary<string, object?>()
                    {
                        ["x-attempts"] = Encoding.UTF8.GetBytes((attempts + 1).ToString()),
                        ["X-error-reason"] = ex.Message
                    }
                };

                await channel.BasicPublishAsync(
                    exchange: topic,
                    routingKey: ea.RoutingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: ea.Body,
                    cancellationToken);

                await channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    cancellationToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queue.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }

    private static async Task<QueueDeclareOk> DeclareDeadQueueAsync(IChannel channel, string topic, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: $"{topic}.dlx",
            type: "fanout",
            durable: true,
            cancellationToken: cancellationToken);

        var deadQueue = await channel.QueueDeclareAsync(
            queue: $"{topic}.dead",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: deadQueue.QueueName,
            exchange: $"{topic}.dlx",
            routingKey: "",
            cancellationToken: cancellationToken);

        return deadQueue;
    }

    private static async Task<QueueDeclareOk> DeclareMainQueueAsync(IChannel channel, string topic, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: topic,
            type: DEFAULT_EXCHANGE_TYPE,
            durable: true,
            cancellationToken: cancellationToken);

        Dictionary<string, object> queueArgs = new()
        {
            ["x-dead-letter-exchange"] = $"{topic}.dlx"
        };

        var queue = await channel.QueueDeclareAsync(
            queue: topic,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs!,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue.QueueName,
            exchange: topic,
            routingKey: "#",
            cancellationToken: cancellationToken);

        return queue;
    }
}