using BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;
using Testcontainers.RabbitMq;

namespace BuildingBlocks.Integration.Tests.Infrastructure.Fixtures;

public class RabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container;

    public RabbitMqFixture()
    {
        _container = new RabbitMqBuilder("rabbitmq:3-management")
                            .WithUsername("guest")
                            .WithPassword("guest")
                            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    public RabbitMQConnectionOptions GetConnectionOptions() => new()
    {
        Host = _container.Hostname,
        Port = _container.GetMappedPublicPort(5672),
        UserName = "guest",
        Password = "guest",
        VirtualHost = "/"
    };
}

[CollectionDefinition("RabbitMq")]
public class RabbitMqFixtureCollection : ICollectionFixture<RabbitMqFixture>;