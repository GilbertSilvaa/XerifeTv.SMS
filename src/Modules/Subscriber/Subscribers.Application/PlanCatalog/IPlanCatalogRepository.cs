namespace Subscribers.Application.PlanCatalog;

public interface IPlanCatalogRepository
{
    Task<PlanItemCatalog?> GetByIdAsync(Guid id);
    Task AddOrUpdateAsync(PlanItemCatalog plan);
    Task RemoveAsync(Guid id);
}
