using SharedKernel;
using Subscribers.Domain.ValueObjects;

namespace Subscribers.Application.PlanCatalog;

public sealed class PlanItemCatalog
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public int MaxSimultaneousScreens { get; private set; }
    public Money Price { get; private set; } = default!;
    public bool IsDeleted { get; private set; }

    private PlanItemCatalog() { }

    public PlanItemCatalog(Guid id, string name, int maxSimultaneousScreens, Money price)
    {
        Id = id;
        Name = name;
        MaxSimultaneousScreens = maxSimultaneousScreens;
        Price = price;
    }

    public void Update(string name, int maxSimultaneousScreens, Money price)
    {
        Name = name;
        MaxSimultaneousScreens = maxSimultaneousScreens;
        Price = price;
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    public PlanSnapshot ToPlanSnapshot()
    {
        return new PlanSnapshot(Id, Name, MaxSimultaneousScreens, Price);
    }
}