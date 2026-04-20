using BuildingBlocks.Core;
using BuildingBlocks.Infrastructure.Cache;
using Plans.Domain;

namespace Plans.Infrastructure.Persistence;

public sealed class PlanCacheInvalidationInterceptor : CacheInvalidationInterceptor<Plan>
{
    private readonly ICacheService _cacheService;

    public PlanCacheInvalidationInterceptor(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async override Task InvalidateCacheAsync(List<Plan> plans)
    {
        foreach (var plan in plans)
            await _cacheService.DeleteAsync($"Plans:{plan.Id}");

        await _cacheService.DeleteAsync("Plans:All");
    }
}