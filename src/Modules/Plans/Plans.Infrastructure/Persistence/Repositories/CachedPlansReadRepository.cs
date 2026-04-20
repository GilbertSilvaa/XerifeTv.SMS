using BuildingBlocks.Core;
using Plans.Application.Queries.ReadModels;

namespace Plans.Infrastructure.Persistence.Repositories;

public sealed class CachedPlansReadRepository : IPlansReadRepository
{
    private readonly IPlansReadRepository _plansReadRepository;
    private readonly ICacheService _cacheService;

    public CachedPlansReadRepository(IPlansReadRepository plansReadRepository, ICacheService cacheService)
    {
        _plansReadRepository = plansReadRepository;
        _cacheService = cacheService;
    }

    public async Task<PlanDto?> GetPlanByIdAsync(Guid id)
    {
        string cacheKey = $"Plans:{id}";

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                return await _plansReadRepository.GetPlanByIdAsync(id);
            },
            TimeSpan.FromHours(1));
    }

    public async Task<IReadOnlyList<PlanDto>> GetPlansAsync()
    {
        string cacheKey = $"Plans:All";

        var result = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                return await _plansReadRepository.GetPlansAsync();
            },
            TimeSpan.FromHours(1));

        return result ?? [];
    }
}