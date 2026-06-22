using BuildingBlocks.Core.Events;

namespace BuildingBlocks.Integration.Tests.Fakes;

internal class FakeIntegrationEventHandler : IIntegrationEventHandler<FakeIntegrationEvent>
{
    public bool Executed { get; private set; }

    public Task Handle(FakeIntegrationEvent notification, CancellationToken cancellationToken)
    {
        Executed = true;
        return Task.CompletedTask;
    }
}

internal class SecondFakeIntegrationEventHandler : IIntegrationEventHandler<FakeIntegrationEvent>
{
    public bool Executed { get; private set; }

    public Task Handle(FakeIntegrationEvent notification, CancellationToken cancellationToken)
    {
        Executed = true;
        return Task.CompletedTask;
    }
}