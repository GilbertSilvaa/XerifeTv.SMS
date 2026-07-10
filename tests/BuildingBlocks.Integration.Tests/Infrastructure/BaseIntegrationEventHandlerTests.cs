using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Integration.Tests.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

public class BaseIntegrationEventHandlerTests
{
    private readonly Mock<IInboxRepository<FakeAggregate>> _inboxRepositoryMock = new();
    private readonly Mock<IUnitOfWork<FakeAggregate>> _unitOfWorkMock = new();

    private InboxMessage? _capturedMessage;
    private EInboxMessageStatus _statusAtInsertTime;

    public BaseIntegrationEventHandlerTests()
    {
        _inboxRepositoryMock
            .Setup(r => r.AddOrUpdateAsync(It.IsAny<InboxMessage>()))
            .Callback<InboxMessage>(m =>
            {
                _capturedMessage = m;
                _statusAtInsertTime = m.Status;
            })
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Should_InsertAsPendingThenExecuteThenMarkAsProcessed_When_EventIsNewForHandler()
    {
        // Arrange
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());
        var sut = new FakeIntegrationEventHandler(_inboxRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await sut.Handle(@event, CancellationToken.None);

        // Assert
        sut.Executed.Should().BeTrue();

        _statusAtInsertTime.Should().Be(EInboxMessageStatus.PENDING);

        _capturedMessage.Should().NotBeNull();
        _capturedMessage!.EventId.Should().Be(@event.EventId);
        _capturedMessage.HandlerKey.Should().Be(typeof(FakeIntegrationEventHandler).FullName);
        _capturedMessage.Status.Should().Be(EInboxMessageStatus.PROCESSED);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Should_UseHandlerFullNameAsHandlerKey_When_DifferentHandlersProcessSameEvent()
    {
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());

        var firstHandler = new FakeIntegrationEventHandler(_inboxRepositoryMock.Object, _unitOfWorkMock.Object);
        await firstHandler.Handle(@event, CancellationToken.None);
        var firstHandlerKey = _capturedMessage!.HandlerKey;

        var secondHandler = new SecondFakeIntegrationEventHandler(_inboxRepositoryMock.Object, _unitOfWorkMock.Object);
        await secondHandler.Handle(@event, CancellationToken.None);
        var secondHandlerKey = _capturedMessage!.HandlerKey;

        // Assert
        firstHandler.Executed.Should().BeTrue();
        secondHandler.Executed.Should().BeTrue();

        firstHandlerKey.Should().Be(typeof(FakeIntegrationEventHandler).FullName);
        secondHandlerKey.Should().Be(typeof(SecondFakeIntegrationEventHandler).FullName);
        firstHandlerKey.Should().NotBe(secondHandlerKey);
    }

    [Fact]
    public async Task Should_SkipExecution_When_InsertSaveThrowsUniqueConstraintViolation()
    {
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InboxUniqueConstraintViolationException(
                "IX_InboxMessages_EventId_HandlerKey",
                new Exception("duplicate key")));

        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());
        var sut = new FakeIntegrationEventHandler(_inboxRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => sut.Handle(@event, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        sut.Executed.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_SkipExecution_When_InsertSaveThrowsDbUpdateConcurrencyException()
    {
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("concurrency conflict"));

        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());
        var sut = new FakeIntegrationEventHandler(_inboxRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => sut.Handle(@event, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        sut.Executed.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_MarkAsFailedAndRethrow_When_ExecuteThrows()
    {
        // Arrange
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());
        var sut = new ThrowingIntegrationEventHandler(_inboxRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => sut.Handle(@event, CancellationToken.None);

        // Assert
        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(ThrowingIntegrationEventHandler.ExecuteException);

        sut.Executed.Should().BeTrue();
        _capturedMessage!.Status.Should().Be(EInboxMessageStatus.FAILED);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Should_SwallowOriginalException_When_FailedSaveThrowsUniqueConstraintViolation()
    {
        _unitOfWorkMock
            .SetupSequence(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .ThrowsAsync(new InboxUniqueConstraintViolationException(
                "IX_InboxMessages_EventId_HandlerKey",
                new Exception("already processed elsewhere")));

        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());
        var sut = new ThrowingIntegrationEventHandler(_inboxRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => sut.Handle(@event, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        sut.Executed.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Should_SwallowOriginalException_When_FailedSaveThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        _unitOfWorkMock
            .SetupSequence(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .ThrowsAsync(new DbUpdateConcurrencyException("concurrency conflict"));

        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());
        var sut = new ThrowingIntegrationEventHandler(_inboxRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => sut.Handle(@event, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        sut.Executed.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Should_PropagateNewException_And_LoseOriginal_When_FailedSaveThrowsUnexpectedException()
    {
        var unexpected = new TimeoutException("db timeout");

        _unitOfWorkMock
            .SetupSequence(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .ThrowsAsync(unexpected);

        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());
        var sut = new ThrowingIntegrationEventHandler(_inboxRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => sut.Handle(@event, CancellationToken.None);

        // Assert
        var thrown = await act.Should().ThrowAsync<TimeoutException>();
        thrown.Which.Should().BeSameAs(unexpected);
    }

    [Fact]
    public async Task Should_MarkAsFailedAndRethrow_And_NeverExecute_When_InitialInsertThrowsUnexpectedException()
    {
        // Arrange
        var repositoryException = new InvalidOperationException("insert failed");

        _inboxRepositoryMock
            .Setup(r => r.AddOrUpdateAsync(It.IsAny<InboxMessage>()))
            .ThrowsAsync(repositoryException);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());
        var sut = new FakeIntegrationEventHandler(_inboxRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => sut.Handle(@event, CancellationToken.None);

        // Assert
        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(repositoryException);

        sut.Executed.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_PropagateCancellationToken_ToSaveChanges()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(token))
            .Returns(Task.CompletedTask);

        var @event = new FakeIntegrationEvent("John", Guid.NewGuid());
        var sut = new FakeIntegrationEventHandler(_inboxRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await sut.Handle(@event, token);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(token), Times.Exactly(2));
    }
}