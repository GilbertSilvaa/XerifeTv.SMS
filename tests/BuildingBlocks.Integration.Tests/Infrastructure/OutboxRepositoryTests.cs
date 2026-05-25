using BuildingBlocks.Core.Messaging.Outbox;
using BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence;
using BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence.Database;
using BuildingBlocks.Integration.Tests.Fakes;
using BuildingBlocks.Integration.Tests.Infrastructure.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

[Collection("PostgresOutboxDbContext")]
public class OutboxRepositoryTests : IAsyncLifetime
{
    private readonly OutboxDbFixture _fixture;
    private readonly OutboxDbContext _dbContext;
    private readonly OutboxRepository _repository;

    public OutboxRepositoryTests(OutboxDbFixture fixture)
    {
        _fixture = fixture;

        var options = new DbContextOptionsBuilder<OutboxDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _dbContext = new OutboxDbContext(options, default!);

        _repository = new OutboxRepository(_dbContext);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _fixture.ResetDatabaseAsync();
    }

    [Fact]
    public async Task Should_AddMessage_When_MessageDoesNotExist()
    {
        // Arrange
        var message = CreateMessage(EOutboxMessageStatus.PENDING);

        // Act
        await _repository.AddOrUpdateAsync(message);

        // Assert
        var savedMessage = await _dbContext
            .Set<OutboxMessage>()
            .FirstOrDefaultAsync(x => x.Id == message.Id);

        savedMessage.Should().NotBeNull();
        savedMessage!.Status.Should().Be(EOutboxMessageStatus.PENDING);
    }

    [Fact]
    public async Task Should_UpdateMessage_When_MessageAlreadyExists()
    {
        // Arrange
        var message = CreateMessage(EOutboxMessageStatus.PENDING);

        await _repository.AddOrUpdateAsync(message);

        _dbContext.Entry(message).State = EntityState.Detached;

        var existingMessage = await _dbContext
            .Set<OutboxMessage>()
            .FirstAsync(x => x.Id == message.Id);

        existingMessage.MarkAsCompleted();

        // Act
        await _repository.AddOrUpdateAsync(existingMessage);

        // Assert
        var updatedMessage = await _dbContext
            .Set<OutboxMessage>()
            .FirstAsync(x => x.Id == message.Id);

        updatedMessage.Status.Should().Be(EOutboxMessageStatus.PROCESSED);
    }

    [Fact]
    public async Task Should_ReturnOnlyMessagesWithRequestedStatus()
    {
        // Arrange
        var pending = CreateMessage(EOutboxMessageStatus.PENDING);
        var processed = CreateMessage(EOutboxMessageStatus.PROCESSED);

        await _repository.AddOrUpdateAsync(pending);
        await _repository.AddOrUpdateAsync(processed);

        // Act
        var result = await _repository.FetchByStatusAsync(EOutboxMessageStatus.PENDING, 10);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(EOutboxMessageStatus.PENDING);
    }

    [Fact]
    public async Task Should_RespectTakeLimit_When_FetchingMessages()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
            await _repository.AddOrUpdateAsync(CreateMessage(EOutboxMessageStatus.PENDING));

        // Act
        var result = await _repository.FetchByStatusAsync(EOutboxMessageStatus.PENDING, 2);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_ReturnEmptyCollection_When_NoMessagesExist()
    {
        // Act
        var result = await _repository.FetchByStatusAsync(EOutboxMessageStatus.PENDING, 10);

        // Assert
        result.Should().BeEmpty();
    }

    private static OutboxMessage CreateMessage(EOutboxMessageStatus status)
    {
        var integrationEvent = new FakeIntegrationEvent("TestEvent", Guid.NewGuid());
        var message = OutboxMessage.Create(integrationEvent, integrationEvent.EventName);

        switch (status)
        {
            case EOutboxMessageStatus.PENDING:
                break;
            case EOutboxMessageStatus.PROCESSING:
                message.MarkAsProcessing();
                break;
            case EOutboxMessageStatus.PROCESSED:
                message.MarkAsCompleted();
                break;
            case EOutboxMessageStatus.FAILED:
                message.MarkAsFailed();
                break;
        }

        return message;
    }
}