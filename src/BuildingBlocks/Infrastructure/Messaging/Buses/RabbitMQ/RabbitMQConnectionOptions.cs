namespace BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;

public sealed class RabbitMQConnectionOptions
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";
}
