using BuildingBlocks.Core;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Cache;

public sealed class DistributedLockCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(60);

    public DistributedLockCacheService(IDistributedCache cache, IConnectionMultiplexer redis)
    {
        _cache = cache;
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var result = await _cache.GetStringAsync(key);

        if (!string.IsNullOrEmpty(result))
            return JsonSerializer.Deserialize<T>(result);

        return default;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null)
    {
        var result = await GetAsync<T>(key);
        if (result != null) return result;

        var db = _redis.GetDatabase();

        string lockKey = $"{key}:lock";
        string channelName = $"{key}:channel";

        if (await db.StringSetAsync(lockKey, "locked", when: When.NotExists))
        {
            try
            {
                var extingValue = await GetAsync<T>(key);
                if (extingValue != null) return extingValue;

                var response = await factory();

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
                };

                string serializedResponse = JsonSerializer.Serialize(response);
                await _cache.SetStringAsync(key, serializedResponse, options);
                await _redis.GetSubscriber().PublishAsync(RedisChannel.Literal(channelName), "done");

                return response;
            }
            finally
            {
                await db.KeyDeleteAsync($"{key}:lock");
            }
        }

        var immediateResult = await GetAsync<T>(key);
        if (immediateResult != null) return immediateResult;

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var subscriber = _redis.GetSubscriber();
        RedisChannel channel = RedisChannel.Literal(channelName);

        void Handler(RedisChannel rc, RedisValue value) => tcs.TrySetResult(value.ToString());
        await subscriber.SubscribeAsync(channel, Handler);

        try
        {
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(_lockTimeout));

            if (completedTask == tcs.Task)
            {
                var message = await tcs.Task;

                if (message == "done")
                {
                    return await GetAsync<T>(key);
                }

                throw new InvalidOperationException("An error occurred while retrieving the cache.");
            }

            throw new TimeoutException("Timeout while waiting for cache to be populated.");
        }
        finally
        {
            await subscriber.UnsubscribeAsync(channel, Handler);
        }
    }

    public async Task DeleteAsync(string key)
    {
        await _cache.RemoveAsync(key);

        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync($"{key}:lock");

        await _redis.GetSubscriber().PublishAsync(RedisChannel.Literal($"{key}:channel"), "error");
    }
}
