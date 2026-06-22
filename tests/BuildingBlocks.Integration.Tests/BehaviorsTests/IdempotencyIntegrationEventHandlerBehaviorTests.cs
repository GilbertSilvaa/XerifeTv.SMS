using BuildingBlocks.Behaviors;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Integration.Tests.Fakes;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BuildingBlocks.Integration.Tests.BehaviorsTests;

public class IdempotencyIntegrationEventHandlerBehaviorTests
{
    private readonly Mock<IInboxRepository> _inboxRepositoryMock = new();
    private readonly Mock<IInboxUnitOfWork> _inboxUnitOfWorkMock = new();

    private readonly HashSet<(Guid EventId, string HandlerKey)> _committed = new();
    private InboxMessage? _capturedMessage;

    private readonly IdempotencyIntegrationEventHandlerBehavior _sut;

    public IdempotencyIntegrationEventHandlerBehaviorTests()
    {
        _inboxRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<InboxMessage>()))
            .Callback<InboxMessage>(message => _capturedMessage = message)
            .Returns(Task.CompletedTask);

        _inboxUnitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                var key = (_capturedMessage!.EventId, _capturedMessage.HandlerKey);

                if (!_committed.Add(key))
                    throw new UniqueConstraintViolationException("IX_InboxMessages_EventId_HandlerKey");

                return Task.CompletedTask;
            });

        var services = new ServiceCollection();
        services.AddSingleton(_inboxRepositoryMock.Object);
        services.AddSingleton(_inboxUnitOfWorkMock.Object);

        var provider = services.BuildServiceProvider();

        _sut = new IdempotencyIntegrationEventHandlerBehavior(provider);
    }

    private void SeedAsAlreadyProcessed(Guid eventId, string handlerKey) =>
        _committed.Add((eventId, handlerKey));

    private static NotificationHandlerExecutor CreateExecutor<THandler>(
        THandler handler)
        where THandler : IIntegrationEventHandler<FakeIntegrationEvent>
    {
        return new NotificationHandlerExecutor(
            handler,
            (notification, cancellationToken) =>
                handler.Handle((FakeIntegrationEvent)notification, cancellationToken));
    }

    [Fact]
    public async Task Should_BypassInbox_When_NotificationIsNotIntegrationEvent()
    {
        // Arrange
        var domainEvent = new FakeDomainEvent(Guid.NewGuid(), "SomeProperty");
        var notification = new DomainEventNotification<FakeDomainEvent>(domainEvent);
        var handlerCalled = false;

        var executor = new NotificationHandlerExecutor(new object(), (_, _) =>
        {
            handlerCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await _sut.Publish([executor], notification, CancellationToken.None);

        // Assert
        handlerCalled.Should().BeTrue();
        _inboxRepositoryMock.Verify(r => r.AddAsync(It.IsAny<InboxMessage>()), Times.Never);
    }

    [Fact]
    public async Task Should_ExecuteAndPersist_When_EventIsNewForHandler()
    {
        // Arrange
        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());
        var handlerMock = new Mock<IIntegrationEventHandler<FakeIntegrationEvent>>();
        handlerMock.Setup(h => h.Handle(@event, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var executor = CreateExecutor(handlerMock.Object);

        // Act
        await _sut.Publish([executor], @event, CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.Handle(@event, It.IsAny<CancellationToken>()), Times.Once);
        _inboxRepositoryMock.Verify(r => r.AddAsync(It.IsAny<InboxMessage>()), Times.Once);
        _inboxUnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_SkipHandler_When_SameEventAndHandlerWereAlreadyProcessed()
    {
        // Arrange
        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());

        var handler = new FakeIntegrationEventHandler();

        SeedAsAlreadyProcessed(
            @event.EventId,
            typeof(FakeIntegrationEventHandler).FullName!);

        var executor = CreateExecutor(handler);

        // Act
        await _sut.Publish([executor], @event, CancellationToken.None);

        // Assert
        handler.Executed.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ExecutePendingHandler_When_PrecedingHandlerInTheListWasAlreadyProcessed()
    {
        // Arrange
        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());

        var alreadyProcessedHandler = new FakeIntegrationEventHandler();
        var pendingHandler = new SecondFakeIntegrationEventHandler();

        SeedAsAlreadyProcessed(
            @event.EventId,
            typeof(FakeIntegrationEventHandler).FullName!);

        var executors = new[]
        {
            CreateExecutor(alreadyProcessedHandler),
            CreateExecutor(pendingHandler)
        };

        // Act
        await _sut.Publish(executors, @event, CancellationToken.None);

        // Assert
        alreadyProcessedHandler.Executed.Should().BeFalse();

        pendingHandler.Executed.Should().BeTrue(
            "a pending handler should execute even if another handler for the same event has already been processed");
    }

    [Fact]
    public async Task Should_PropagateException_When_HandlerThrowsNonUniqueConstraintError()
    {
        // Arrange
        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());
        var handlerMock = new Mock<IIntegrationEventHandler<FakeIntegrationEvent>>();
        handlerMock
            .Setup(h => h.Handle(@event, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var executor = CreateExecutor(handlerMock.Object);

        // Act
        var act = async () => await _sut.Publish([executor], @event, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_StopProcessingRemainingHandlers_When_AnEarlierHandlerThrowsNonUniqueConstraintError()
    {
        // Arrange
        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());

        var failingHandlerMock = new Mock<IIntegrationEventHandler<FakeIntegrationEvent>>();
        failingHandlerMock
            .Setup(h => h.Handle(@event, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var neverReachedHandlerMock = new Mock<IIntegrationEventHandler<FakeIntegrationEvent>>();
        neverReachedHandlerMock.As<IDisposable>();

        var executors = new[]
        {
            CreateExecutor(failingHandlerMock.Object),
            CreateExecutor(neverReachedHandlerMock.Object)
        };

        // Act
        var act = async () => await _sut.Publish(executors, @event, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        neverReachedHandlerMock.Verify(h => h.Handle(It.IsAny<FakeIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}