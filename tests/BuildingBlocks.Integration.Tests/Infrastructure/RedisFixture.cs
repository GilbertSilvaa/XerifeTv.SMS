using Testcontainers.Redis;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

public sealed class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container;

    public string ConnectionString => _container.GetConnectionString();

    public RedisFixture()
    {
        _container = new RedisBuilder("redis:7").Build();
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
}

[CollectionDefinition("Redis")]
public sealed class RedisCollection : ICollectionFixture<RedisFixture>;