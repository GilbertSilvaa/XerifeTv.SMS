using SharedKernel;

namespace Subscribers.Domain.Entities;

public sealed class PlanItemCatalog : Entity
{
    public string Name { get; private set; } = default!;
    public int MaxSimultaneousScreens { get; private set; }
    public Money Price { get; private set; } = default!;
    public bool IsDisabled => IsDeleted;

    public PlanItemCatalog(Guid id, string name, int maxSimultaneousScreens, Money price)
    {
        Id = id;
        Name = name;
        MaxSimultaneousScreens = maxSimultaneousScreens;
        Price = price;
    }
}