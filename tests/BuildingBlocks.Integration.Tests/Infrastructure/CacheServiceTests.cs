using BuildingBlocks.Core;
using BuildingBlocks.Infrastructure.Cache;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

[Collection("Redis")]
public sealed class CacheServiceTests
{
    private readonly ICacheService _cacheService;
    private readonly IConnectionMultiplexer _redis;

    public CacheServiceTests(RedisFixture fixture)
    {
        var services = new ServiceCollection();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = fixture.ConnectionString;
            options.InstanceName = "TestInstance";
        });

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(fixture.ConnectionString));

        services.AddSingleton<ICacheService>(_ => new DistributedLockCacheService(
           services.BuildServiceProvider().GetRequiredService<IDistributedCache>(),
           services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>(),
           TimeSpan.FromSeconds(3)
        ));

        var provider = services.BuildServiceProvider();

        _cacheService = provider.GetRequiredService<ICacheService>();
        _redis = provider.GetRequiredService<IConnectionMultiplexer>();
    }

    [Fact]
    public async Task Should_SetAndGetValue_When_CacheIsUsed()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var value = "test_value";

        // Act
        await _cacheService.GetOrCreateAsync(
            key,
            () => Task.FromResult<string?>(value),
            TimeSpan.FromMinutes(5));

        var cachedValue = await _cacheService.GetAsync<string>(key);

        // Assert
        cachedValue.Should().Be(value);
    }

    [Fact]
    public async Task Should_ReturnNull_When_KeyDoesNotExist()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_NotExecuteFactory_When_ValueAlreadyExists()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();

        await _cacheService.GetOrCreateAsync(
            key,
            () => Task.FromResult<string?>("cached"));

        var factoryExecuted = false;

        // Act
        var result = await _cacheService.GetOrCreateAsync(
            key,
            () =>
            {
                factoryExecuted = true;
                return Task.FromResult<string?>("new");
            });

        // Assert
        result.Should().Be("cached");
        factoryExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_RemoveValue_When_DeleteIsCalled()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();

        await _cacheService.GetOrCreateAsync(
            key,
            () => Task.FromResult<string?>("value"));

        // Act
        await _cacheService.DeleteAsync(key);

        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_ExecuteFactoryOnlyOnce_When_MultipleRequestsOccur()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();

        int executionCount = 0;

        async Task<string?> Factory()
        {
            Interlocked.Increment(ref executionCount);

            await Task.Delay(1000);

            return "value";
        }

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _cacheService.GetOrCreateAsync(key, Factory));

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBeEquivalentTo("value");

        executionCount.Should().Be(1);
    }

    [Fact]
    public async Task Should_ReturnCreatedValue_When_ConcurrentRequestsWait()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();

        async Task<string?> Factory()
        {
            await Task.Delay(1000);

            return "shared";
        }

        // Act
        var task1 = _cacheService.GetOrCreateAsync(key, Factory);

        await Task.Delay(200);

        var task2 = _cacheService.GetOrCreateAsync(
            key,
            () => Task.FromResult<string?>("other"));

        var result1 = await task1;
        var result2 = await task2;

        // Assert
        result1.Should().Be("shared");
        result2.Should().Be("shared");
    }

    [Fact]
    public async Task Should_ExpireCache_When_ExpirationIsReached()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();

        await _cacheService.GetOrCreateAsync(
            key,
            () => Task.FromResult<string?>("temporary"),
            TimeSpan.FromMilliseconds(500));

        // Act
        await Task.Delay(1000);

        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_ThrowTimeout_When_LockExistsAndNoMessageIsPublished()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();

        var db = _redis.GetDatabase();

        await db.StringSetAsync(
            $"{key}:lock",
            "locked");

        // Act
        Func<Task> action = async () =>
        {
            await _cacheService.GetOrCreateAsync<string>(
                key,
                () => Task.FromResult<string?>("value"));
        };

        // Assert
        await action
            .Should()
            .ThrowAsync<TimeoutException>();
    }
}