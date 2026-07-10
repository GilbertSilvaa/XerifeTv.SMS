using BuildingBlocks.Common;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Outbox;
using BuildingBlocks.Infrastructure.Messaging.Dispatchers;
using BuildingBlocks.Integration.Tests.Fakes;
using FluentAssertions;
using Moq;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

public class OutboxMessageDispatcherTests
{
    private readonly Mock<IOutboxRepository<FakeAggregate>> _outboxRepositoryMock;
    private readonly Mock<IUnitOfWork<FakeAggregate>> _unitOfWorkMock;
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly IOutboxMessageDispatcher<FakeAggregate> _dispatcher;

    public OutboxMessageDispatcherTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxRepository<FakeAggregate>>();
        _messageBusMock = new Mock<IMessageBus>();
        _unitOfWorkMock = new Mock<IUnitOfWork<FakeAggregate>>();

        _dispatcher = new OutboxMessageDispatcher<FakeAggregate>(_messageBusMock.Object, _outboxRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Should_NotPublish_When_NoPendingMessagesExist()
    {
        // Arrange
        _outboxRepositoryMock
            .Setup(x => x.FetchByStatusAsync(
                EOutboxMessageStatus.PENDING,
                MessagingConstants.MAX_MESSAGES_PER_BATCH))
            .ReturnsAsync([]);

        // Act
        await _dispatcher.DispatchAsync(3, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_PublishMessage_When_MessageIsPending()
    {
        // Arrange
        var message = CreateMessage();

        _outboxRepositoryMock
            .Setup(x => x.FetchByStatusAsync(
                EOutboxMessageStatus.PENDING,
                MessagingConstants.MAX_MESSAGES_PER_BATCH))
            .ReturnsAsync([message]);

        // Act
        await _dispatcher.DispatchAsync(3, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                message.Payload,
                MessagingConstants.INTEGRATION_EVENTS_TOPIC,
                message.RoutingKey,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_MarkAsProcessing_BeforePublishing()
    {
        // Arrange
        var message = CreateMessage();
        var executionOrder = new List<string>();

        _outboxRepositoryMock
            .Setup(x => x.AddOrUpdateAsync(
                It.Is<OutboxMessage>(m =>
                    m.Status == EOutboxMessageStatus.PROCESSING)))
            .Callback(() => executionOrder.Add("PROCESSING"));

        _messageBusMock
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => executionOrder.Add("PUBLISH"));

        _outboxRepositoryMock
            .Setup(x => x.FetchByStatusAsync(
                EOutboxMessageStatus.PENDING,
                MessagingConstants.MAX_MESSAGES_PER_BATCH))
            .ReturnsAsync([message]);

        // Act
        await _dispatcher.DispatchAsync(3, CancellationToken.None);

        // Assert
        executionOrder.Should().ContainInOrder("PROCESSING", "PUBLISH");
    }

    [Fact]
    public async Task Should_MarkMessageAsCompleted_When_PublishSucceeds()
    {
        // Arrange
        var message = CreateMessage();
        var capturedStatuses = new List<EOutboxMessageStatus>();

        _outboxRepositoryMock
            .Setup(x => x.FetchByStatusAsync(
                EOutboxMessageStatus.PENDING,
                MessagingConstants.MAX_MESSAGES_PER_BATCH))
            .ReturnsAsync([message]);

        _outboxRepositoryMock
            .Setup(x => x.AddOrUpdateAsync(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => capturedStatuses.Add(m.Status));

        // Act
        await _dispatcher.DispatchAsync(3, CancellationToken.None);

        // Assert
        capturedStatuses.Should().Equal(
            EOutboxMessageStatus.PROCESSING,
            EOutboxMessageStatus.PROCESSED);
    }

    [Fact]
    public async Task Should_MarkMessageAsFailed_When_PublishThrows()
    {
        // Arrange
        var message = CreateMessage();
        var capturedStatuses = new List<EOutboxMessageStatus>();

        _outboxRepositoryMock
            .Setup(x => x.FetchByStatusAsync(
                EOutboxMessageStatus.PENDING,
                MessagingConstants.MAX_MESSAGES_PER_BATCH))
            .ReturnsAsync([message]);

        _outboxRepositoryMock
            .Setup(x => x.AddOrUpdateAsync(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => capturedStatuses.Add(m.Status));

        _messageBusMock
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act
        await _dispatcher.DispatchAsync(1, CancellationToken.None);

        // Assert
        capturedStatuses.Should().Equal(
            EOutboxMessageStatus.PROCESSING,
            EOutboxMessageStatus.FAILED);
    }

    [Fact]
    public async Task Should_RetryPublish_Until_MaxRetries()
    {
        // Arrange
        var message = CreateMessage();

        _outboxRepositoryMock
            .Setup(x => x.FetchByStatusAsync(
                EOutboxMessageStatus.PENDING,
                MessagingConstants.MAX_MESSAGES_PER_BATCH))
            .ReturnsAsync([message]);

        _messageBusMock
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act
        await _dispatcher.DispatchAsync(3, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task Should_StopRetrying_When_PublishEventuallySucceeds()
    {
        // Arrange
        var message = CreateMessage();

        _outboxRepositoryMock
            .Setup(x => x.FetchByStatusAsync(
                EOutboxMessageStatus.PENDING,
                MessagingConstants.MAX_MESSAGES_PER_BATCH))
            .ReturnsAsync([message]);

        _messageBusMock
            .SetupSequence(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception())
            .ThrowsAsync(new Exception())
            .Returns(Task.CompletedTask);

        // Act
        await _dispatcher.DispatchAsync(5, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task Should_ProcessAllMessages()
    {
        // Arrange
        var messages = new[]
        {
            CreateMessage(),
            CreateMessage(),
            CreateMessage()
        };

        _outboxRepositoryMock
            .Setup(x => x.FetchByStatusAsync(
                EOutboxMessageStatus.PENDING,
                MessagingConstants.MAX_MESSAGES_PER_BATCH))
            .ReturnsAsync(messages);

        // Act
        await _dispatcher.DispatchAsync(3, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task Should_FetchPendingMessages_WithConfiguredBatchSize()
    {
        // Act
        await _dispatcher.DispatchAsync(3, CancellationToken.None);

        // Assert
        _outboxRepositoryMock.Verify(
            x => x.FetchByStatusAsync(
                EOutboxMessageStatus.PENDING,
                MessagingConstants.MAX_MESSAGES_PER_BATCH),
            Times.Once);
    }

    private static OutboxMessage CreateMessage()
    {
        FakeIntegrationEvent @event = new("fake test", Guid.NewGuid());

        return OutboxMessage.Create(@event, routingKey: "fake.test");
    }
}
