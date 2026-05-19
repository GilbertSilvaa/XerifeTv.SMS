using SharedKernel;

namespace BuildingBlocks.Integration.Tests.Fakes;

public sealed class FakeAggregate : AggregateRoot
{
    public string Name { get; private set; }

    public FakeAggregate(string name)
    {
        Name = name;
        AddDomainEvent(new FakeDomainEvent(Id, name));
    }

    public void UpdateName(string newName)
    {
        Name = newName;
        AddDomainEvent(new FakeDomainEvent(Id, newName));
    }
}