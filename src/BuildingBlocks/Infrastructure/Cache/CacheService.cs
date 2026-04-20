using BuildingBlocks.Core;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Cache;

public sealed class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;

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

        var response = await factory();
        if (response == null) return default;

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
        };

        await _cache.SetStringAsync(key, JsonSerializer.Serialize(response), options);
        return response;
    }

    public async Task DeleteAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }
}