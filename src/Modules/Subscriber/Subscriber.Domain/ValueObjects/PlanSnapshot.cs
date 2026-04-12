using SharedKernel;

namespace Subscribers.Domain.ValueObjects;

public sealed record PlanSnapshot
{
    public Guid PlanId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Screens { get; private set; }
    public Money Price { get; private set; } = default!;

    private PlanSnapshot() { }

    public PlanSnapshot(Guid planId, string name, int screens, Money price)
    {
        PlanId = planId;
        Name = name;
        Screens = screens;
        Price = price;
    }
}