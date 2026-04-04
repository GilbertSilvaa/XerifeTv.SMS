using SharedKernel;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;

namespace Subscribers.Domain.Services;

public sealed class PlanResolver
{
    private readonly IPlanCatalogRepository _planCatalogRepository;

    public PlanResolver(IPlanCatalogRepository planCatalogRepository)
    {
        _planCatalogRepository = planCatalogRepository;
    }

    public async Task<Result<PlanItemCatalog?>> ResolveActivePlanAsync(Guid planId)
    {
        var plan = await _planCatalogRepository.GetByIdAsync(planId);

        if (plan == null)
            return Result<PlanItemCatalog?>.Failure(new Error("PlanResolver.PlanNotFound", "Plan not found."));

        if (plan.IsDisabled)
            return Result<PlanItemCatalog?>.Failure(new Error("PlanResolver.PlanIsDisaled", "Plan is not active."));

        return Result<PlanItemCatalog?>.Success(plan);
    }
}
