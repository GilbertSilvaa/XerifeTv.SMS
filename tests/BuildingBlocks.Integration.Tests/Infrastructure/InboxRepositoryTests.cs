using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using BuildingBlocks.Integration.Tests.Infrastructure.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

[Collection("PostgresInboxDbContext")]
public class InboxRepositoryTests : IAsyncLifetime
{
    private readonly InboxDbFixture _fixture;
    private readonly InboxDbContext _dbContext;
    private readonly InboxRepository _repository;
    private readonly InboxUnitOfWork _unitOfWork;

    public InboxRepositoryTests(InboxDbFixture fixture)
    {
        _fixture = fixture;

        var options = new DbContextOptionsBuilder<InboxDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _dbContext = new InboxDbContext(options, default!);
        _repository = new InboxRepository(_dbContext);
        _unitOfWork = new InboxUnitOfWork(_dbContext);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _fixture.ResetDatabaseAsync();
    }

    [Fact]
    public async Task Should_AddMessage_When_MessageIsValid()
    {
        // Arrange
        var message = InboxMessage.Create(Guid.NewGuid(), "TestHandler", "TestEvent");

        // Act
        await _repository.AddOrUpdateAsync(message);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var savedMessage = await _dbContext
            .Set<InboxMessage>()
            .FirstOrDefaultAsync(x => x.EventId == message.EventId);

        savedMessage.Should().NotBeNull();
        savedMessage!.Status.Should().Be(EInboxMessageStatus.PENDING);
    }

    [Fact]
    public async Task Should_PersistCorrectValues_When_MessageIsSaved()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var message = InboxMessage.Create(eventId, "UserCreatedHandler", "UserCreatedEvent");

        // Act
        await _repository.AddOrUpdateAsync(message);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var savedMessage = await _dbContext
            .Set<InboxMessage>()
            .FirstAsync(x => x.EventId == eventId);

        savedMessage.EventId.Should().Be(eventId);
        savedMessage.HandlerKey.Should().Be("UserCreatedHandler");
        savedMessage.EventType.Should().Be("UserCreatedEvent");
        savedMessage.Status.Should().Be(EInboxMessageStatus.PENDING);
        savedMessage.ReceivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        savedMessage.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task Should_ThrowUniqueConstraintViolationException_When_EventAlreadyExistsAndIsPendingOrProcessing()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var first = InboxMessage.Create(eventId, "SameHandler", "Event");
        var duplicate = InboxMessage.Create(eventId, "SameHandler", "Event");

        await _repository.AddOrUpdateAsync(first);
        await _unitOfWork.SaveChangesAsync();

        _dbContext.Entry(first).State = EntityState.Detached;

        // Act
        var action = async () =>
        {
            await _repository.AddOrUpdateAsync(duplicate);
            await _unitOfWork.SaveChangesAsync();
        };

        // Assert
        await action.Should().ThrowAsync<UniqueConstraintViolationException>();
    }

    [Fact]
    public async Task Should_ThrowUniqueConstraintViolationException_Immediately_When_MessageIsAlreadyProcessed()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var message = InboxMessage.Create(eventId, "SameHandler", "Event");
        message.MarkAsProcessed();

        await _repository.AddOrUpdateAsync(message);
        await _unitOfWork.SaveChangesAsync();
        _dbContext.Entry(message).State = EntityState.Detached;

        var duplicate = InboxMessage.Create(eventId, "SameHandler", "Event");

        // Act
        var action = async () =>
        {
            await _repository.AddOrUpdateAsync(duplicate);
            await _unitOfWork.SaveChangesAsync();
        };

        // Assert
        await action.Should().ThrowAsync<UniqueConstraintViolationException>();
    }

    [Fact]
    public async Task Should_UpdateStatus_When_MessageExistsAsFailed()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var message = InboxMessage.Create(eventId, "RetryHandler", "Event");
        message.MarkAsFailed("Erro de timeout na primeira tentativa");

        await _repository.AddOrUpdateAsync(message);
        await _unitOfWork.SaveChangesAsync();
        _dbContext.Entry(message).State = EntityState.Detached;

        var retryMessage = InboxMessage.Create(eventId, "RetryHandler", "Event");

        // Act
        await _repository.AddOrUpdateAsync(retryMessage);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedMessage = await _dbContext
            .Set<InboxMessage>()
            .FirstAsync(x => x.EventId == eventId && x.HandlerKey == "RetryHandler");

        updatedMessage.Status.Should().Be(EInboxMessageStatus.PENDING);
        updatedMessage.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task Should_PreserveOriginalException_When_UniqueViolationOccurs()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var first = InboxMessage.Create(eventId, "SameHandler", "Event");

        await _repository.AddOrUpdateAsync(first);
        await _unitOfWork.SaveChangesAsync();
        _dbContext.Entry(first).State = EntityState.Detached;

        var duplicate = InboxMessage.Create(eventId, "SameHandler", "Event");

        // Act
        var exception = await Assert.ThrowsAsync<UniqueConstraintViolationException>(async () =>
        {
            await _repository.AddOrUpdateAsync(duplicate);
            await _unitOfWork.SaveChangesAsync();
        });

        // Assert
        exception.InnerException.Should().NotBeNull();
        exception.InnerException.Should().BeOfType<DbUpdateException>();
    }

    [Fact]
    public async Task Should_AllowSameEvent_When_HandlerKeyIsDifferent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var first = InboxMessage.Create(eventId, "HandlerA", "UserCreatedEvent");
        var second = InboxMessage.Create(eventId, "HandlerB", "UserCreatedEvent");

        await _repository.AddOrUpdateAsync(first);
        await _unitOfWork.SaveChangesAsync();
        _dbContext.Entry(first).State = EntityState.Detached;

        // Act
        var action = async () =>
        {
            await _repository.AddOrUpdateAsync(second);
            await _unitOfWork.SaveChangesAsync();
        };

        // Assert
        await action.Should().NotThrowAsync();

        var totalCount = await _dbContext.Set<InboxMessage>().CountAsync(x => x.EventId == eventId);
        totalCount.Should().Be(2);
    }
}