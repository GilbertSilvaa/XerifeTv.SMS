using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using BuildingBlocks.Integration.Tests.Fakes;
using BuildingBlocks.Integration.Tests.Infrastructure.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

[Collection("PostgresFakeDbContext")]
public class InboxRepositoryTests : IAsyncLifetime
{
    private readonly FakeDbFixure _fixture;
    private readonly FakeDbContext _dbContext;
    private readonly InboxRepository<FakeAggregate, FakeDbContext> _repository;

    public InboxRepositoryTests(FakeDbFixure fixture)
    {
        _fixture = fixture;

        var options = new DbContextOptionsBuilder<FakeDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _dbContext = new FakeDbContext(options);
        _repository = new InboxRepository<FakeAggregate, FakeDbContext>(_dbContext);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _fixture.ResetDatabaseAsync();
    }

    private FakeDbContext CreateWorkerContext()
    {
        var options = new DbContextOptionsBuilder<FakeDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        return new FakeDbContext(options);
    }

    private static async Task BackdateReceivedAtAsync(
        FakeDbContext context, InboxMessage trackedMessage, TimeSpan elapsed)
    {
        context.Entry(trackedMessage).Property(nameof(InboxMessage.ReceivedAt)).CurrentValue =
            DateTime.UtcNow - elapsed;

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Should_AddMessage_When_MessageIsValid()
    {
        // Arrange
        var message = InboxMessage.Create(Guid.NewGuid(), "TestHandler", "TestEvent");

        await _repository.AddOrUpdateAsync(message);
        await _dbContext.SaveChangesAsync();

        // Act
        var savedMessage = await _dbContext
            .Set<InboxMessage>()
            .FirstOrDefaultAsync(x => x.EventId == message.EventId);

        // Assert
        savedMessage.Should().NotBeNull();
        savedMessage!.Status.Should().Be(EInboxMessageStatus.PENDING);
    }

    [Fact]
    public async Task Should_PersistCorrectValues_When_MessageIsSaved()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var message = InboxMessage.Create(eventId, "UserCreatedHandler", "UserCreatedEvent");

        await _repository.AddOrUpdateAsync(message);
        await _dbContext.SaveChangesAsync();

        // Act
        var savedMessage = await _dbContext
            .Set<InboxMessage>()
            .FirstAsync(x => x.EventId == eventId);

        // Assert
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
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(first).State = EntityState.Detached;

        // Act
        var action = async () =>
        {
            await _repository.AddOrUpdateAsync(duplicate);
            await _dbContext.SaveChangesAsync();
        };

        // Assert
        await action.Should().ThrowAsync<InboxUniqueConstraintViolationException>();
    }

    [Fact]
    public async Task Should_ThrowUniqueConstraintViolationException_Immediately_When_MessageIsAlreadyProcessed()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var message = InboxMessage.Create(eventId, "SameHandler", "Event");
        message.MarkAsProcessed();

        await _repository.AddOrUpdateAsync(message);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(message).State = EntityState.Detached;

        var duplicate = InboxMessage.Create(eventId, "SameHandler", "Event");

        // Act
        var action = async () =>
        {
            await _repository.AddOrUpdateAsync(duplicate);
            await _dbContext.SaveChangesAsync();
        };

        // Assert
        await action.Should().ThrowAsync<InboxUniqueConstraintViolationException>();
    }

    [Fact]
    public async Task Should_UpdateStatus_When_MessageExistsAsFailed()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var message = InboxMessage.Create(eventId, "RetryHandler", "Event");
        message.MarkAsFailed("Erro de timeout na primeira tentativa");

        // Act
        await _repository.AddOrUpdateAsync(message);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(message).State = EntityState.Detached;

        var retryMessage = InboxMessage.Create(eventId, "RetryHandler", "Event");

        await _repository.AddOrUpdateAsync(retryMessage);
        await _dbContext.SaveChangesAsync();

        var updatedMessage = await _dbContext
            .Set<InboxMessage>()
            .FirstAsync(x => x.EventId == eventId && x.HandlerKey == "RetryHandler");

        // Assert
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
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(first).State = EntityState.Detached;

        var duplicate = InboxMessage.Create(eventId, "SameHandler", "Event");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InboxUniqueConstraintViolationException>(async () =>
        {
            await _repository.AddOrUpdateAsync(duplicate);
            await _dbContext.SaveChangesAsync();
        });

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
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(first).State = EntityState.Detached;

        // Act
        var action = async () =>
        {
            await _repository.AddOrUpdateAsync(second);
            await _dbContext.SaveChangesAsync();
        };

        // Assert
        await action.Should().NotThrowAsync();

        var totalCount = await _dbContext.Set<InboxMessage>().CountAsync(x => x.EventId == eventId);
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_UpdateToProcessed_When_PendingMessageCompletesSuccessfully()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var pending = InboxMessage.Create(eventId, "SuccessHandler", "Event");

        // Act
        await _repository.AddOrUpdateAsync(pending);
        await _dbContext.SaveChangesAsync();

        pending.MarkAsProcessed();
        await _dbContext.SaveChangesAsync();

        var updated = await _dbContext
            .Set<InboxMessage>()
            .FirstAsync(x => x.EventId == eventId && x.HandlerKey == "SuccessHandler");

        // Assert
        updated.Status.Should().Be(EInboxMessageStatus.PROCESSED);
        updated.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_AllowReclaim_When_PendingLockHasExpired()
    {
        // Arrange
        var repository = new InboxRepository<FakeAggregate, FakeDbContext>(_dbContext, lockTimeoutInMinutes: 0);

        var eventId = Guid.NewGuid();
        var original = InboxMessage.Create(eventId, "ExpiringHandler", "Event");

        await repository.AddOrUpdateAsync(original);
        await _dbContext.SaveChangesAsync();

        await BackdateReceivedAtAsync(_dbContext, original, TimeSpan.FromMinutes(1));
        _dbContext.Entry(original).State = EntityState.Detached;

        var reclaim = InboxMessage.Create(eventId, "ExpiringHandler", "Event");

        // Act
        var action = async () =>
        {
            await repository.AddOrUpdateAsync(reclaim);
            await _dbContext.SaveChangesAsync();
        };

        await action.Should().NotThrowAsync();

        var updated = await _dbContext
            .Set<InboxMessage>()
            .FirstAsync(x => x.EventId == eventId && x.HandlerKey == "ExpiringHandler");

        // Assert
        updated.Status.Should().Be(EInboxMessageStatus.PENDING);
        updated.ReceivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Should_ThrowDbUpdateConcurrencyException_When_TwoStaleTrackedReadsRaceToUpdateSameRow()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const string handlerKey = "RaceHandler";

        var original = InboxMessage.Create(eventId, handlerKey, "Event");
        await _repository.AddOrUpdateAsync(original);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(original).State = EntityState.Detached;

        var workerA = CreateWorkerContext();
        var workerB = CreateWorkerContext();

        try
        {
            // Act
            var trackedByA = await workerA.Set<InboxMessage>()
                .FirstAsync(x => x.EventId == eventId && x.HandlerKey == handlerKey);
            var trackedByB = await workerB.Set<InboxMessage>()
                .FirstAsync(x => x.EventId == eventId && x.HandlerKey == handlerKey);

            trackedByA.MarkAsProcessed();
            await workerA.SaveChangesAsync();

            trackedByB.MarkAsFailed("timeout simulando corrida");

            var action = async () => await workerB.SaveChangesAsync();

            // Assert
            await action.Should().ThrowAsync<DbUpdateConcurrencyException>();
        }
        finally
        {
            await workerA.DisposeAsync();
            await workerB.DisposeAsync();
        }
    }
}