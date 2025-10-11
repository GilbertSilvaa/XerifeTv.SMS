using Microsoft.Extensions.Caching.Distributed;
using Plans.Application.Contracts;
using Plans.Application.Contracts.DTOs;
using System.Text.Json;

namespace Plans.Infrastructure.Persistence.Repositories;

public sealed class CachedPlansReadRepository : IPlansReadRepository
{
	private const string CACHE_KEY_ROOT = "Plans";

	private readonly IPlansReadRepository _plansReadRepository;
	private readonly IDistributedCache _cache;

	public CachedPlansReadRepository(IPlansReadRepository plansReadRepository, IDistributedCache cache)
	{
		_plansReadRepository = plansReadRepository;
		_cache = cache;
	}

	public async Task<PlanDto?> GetPlanByIdAsync(Guid id)
	{
		var plansCachedJson = await _cache.GetStringAsync(CACHE_KEY_ROOT);
		PlansCache? plansCache = null;

		if (!string.IsNullOrEmpty(plansCachedJson))
			plansCache = JsonSerializer.Deserialize<PlansCache>(plansCachedJson);

		if (plansCache?.PlanById != null && plansCache.PlanById.TryGetValue(id, out var cachedPlan))
			return cachedPlan;

		var plan = await _plansReadRepository.GetPlanByIdAsync(id);
		if (plan == null) return null;

		if (plansCache == null)
			plansCache = new PlansCache([], []);

		plansCache.PlanById[id] = plan;

		await _cache.SetStringAsync(CACHE_KEY_ROOT, JsonSerializer.Serialize(plansCache));

		return plan;
	}

	public async Task<IReadOnlyList<PlanDto>> GetPlansAsync()
	{
		var plansCachedJson = await _cache.GetStringAsync(CACHE_KEY_ROOT);
		PlansCache? plansCache = null;

		if (!string.IsNullOrEmpty(plansCachedJson))
			plansCache = JsonSerializer.Deserialize<PlansCache>(plansCachedJson);

		if (plansCache?.Plans?.Count > 0)
			return [.. plansCache.Plans];

		var plans = await _plansReadRepository.GetPlansAsync();

		var newCache = plansCache ?? new PlansCache([], []);
		newCache.Plans.Clear();
		newCache.Plans.AddRange(plans);
		await _cache.SetStringAsync(CACHE_KEY_ROOT, JsonSerializer.Serialize(newCache));

		return plans;
	}

	private record PlansCache(List<PlanDto> Plans, Dictionary<Guid, PlanDto> PlanById);
}