using BuildingBlocks.Core;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;

namespace BuildingBlocks.Integration.Tests.Fakes;

internal class FakeIntegrationEventHandler : BaseIntegrationEventHandler<FakeIntegrationEvent, FakeAggregate>
{
    public FakeIntegrationEventHandler(
        IInboxRepository<FakeAggregate> inboxRepository,
        IUnitOfWork<FakeAggregate> unitOfWork) : base(inboxRepository, unitOfWork) { }

    public bool Executed { get; private set; }

    public override Task Execute(FakeIntegrationEvent notification, CancellationToken cancellationToken)
    {
        Executed = true;
        return Task.CompletedTask;
    }
}

internal class SecondFakeIntegrationEventHandler : BaseIntegrationEventHandler<FakeIntegrationEvent, FakeAggregate>
{
    public SecondFakeIntegrationEventHandler(
        IInboxRepository<FakeAggregate> inboxRepository,
        IUnitOfWork<FakeAggregate> unitOfWork) : base(inboxRepository, unitOfWork) { }

    public bool Executed { get; private set; }

    public override Task Execute(FakeIntegrationEvent notification, CancellationToken cancellationToken)
    {
        Executed = true;
        return Task.CompletedTask;
    }
}

internal class ThrowingIntegrationEventHandler : BaseIntegrationEventHandler<FakeIntegrationEvent, FakeAggregate>
{
    public static readonly InvalidOperationException ExecuteException = new("boom");

    public ThrowingIntegrationEventHandler(
        IInboxRepository<FakeAggregate> inboxRepository,
        IUnitOfWork<FakeAggregate> unitOfWork) : base(inboxRepository, unitOfWork) { }

    public bool Executed { get; private set; }

    public override Task Execute(FakeIntegrationEvent notification, CancellationToken cancellationToken)
    {
        Executed = true;
        throw ExecuteException;
    }
}