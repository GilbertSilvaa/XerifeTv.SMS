namespace BuildingBlocks.Core;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null);
    Task DeleteAsync(string key);
}
