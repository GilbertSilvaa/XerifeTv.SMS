using BuildingBlocks.Core;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Cache;

public sealed class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ConcurrentDictionary<string, Lazy<Task<string?>>> _inFlight = new();

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var result = await _cache.GetStringAsync(key);

        if (!string.IsNullOrEmpty(result))
            return JsonSerializer.Deserialize<T>(result);

        return default;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration)
    {
        var result = await GetAsync<T>(key);
        if (result != null) return result;

        var lazySearch = _inFlight.GetOrAdd(
            key,
            _ => new Lazy<Task<string?>>(
                async () =>
                {
                    try
                    {
                        var response = await factory();
                        if (response == null) return null;

                        var options = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
                        };

                        string serializedResponse = JsonSerializer.Serialize(response);
                        await _cache.SetStringAsync(key, serializedResponse, options);

                        return serializedResponse;
                    }
                    finally
                    {
                        _inFlight.TryRemove(key, out var removed);
                    }
                },
                LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            string? lazyResult = await lazySearch.Value;
            return lazyResult != null ? JsonSerializer.Deserialize<T>(lazyResult) : default;
        }
        catch
        {
            _inFlight.TryRemove(key, out _);
            throw;
        }
    }

    public async Task DeleteAsync(string key)
    {
        _inFlight.TryRemove(key, out _);
        await _cache.RemoveAsync(key);
    }
}