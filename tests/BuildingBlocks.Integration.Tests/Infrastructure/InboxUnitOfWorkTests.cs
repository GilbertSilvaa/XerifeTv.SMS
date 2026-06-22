using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using BuildingBlocks.Integration.Tests.Infrastructure.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

[Collection("PostgresInboxDbContext")]
public class InboxUnitOfWorkTests : IAsyncLifetime
{
    private readonly InboxDbFixture _fixture;
    private readonly InboxDbContext _dbContext;
    private readonly InboxUnitOfWork _sut;

    public InboxUnitOfWorkTests(InboxDbFixture fixture)
    {
        _fixture = fixture;

        var options = new DbContextOptionsBuilder<InboxDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _dbContext = new InboxDbContext(options, default!);
        _sut = new InboxUnitOfWork(_dbContext);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _fixture.ResetDatabaseAsync();
    }

    [Fact]
    public async Task Should_SaveChanges_When_EntityIsValid()
    {
        // Arrange
        var message = InboxMessage.Create(
            Guid.NewGuid(),
            "TestHandler",
            "TestEvent");

        _dbContext.InboxMessages.Add(message);

        // Act
        await _sut.SaveChangesAsync();

        // Assert
        var savedMessage = await _dbContext.InboxMessages
            .FirstOrDefaultAsync(x => x.EventId == message.EventId);

        savedMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ThrowUniqueConstraintViolationException_When_UniqueConstraintIsViolated()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        _dbContext.InboxMessages.Add(
            InboxMessage.Create(eventId, "Handler", "Event"));

        await _sut.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        _dbContext.InboxMessages.Add(
            InboxMessage.Create(eventId, "Handler", "Event"));

        // Act
        var act = () => _sut.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<UniqueConstraintViolationException>();
    }

    [Fact]
    public async Task Should_PreserveOriginalException_When_UniqueConstraintIsViolated()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        _dbContext.InboxMessages.Add(
            InboxMessage.Create(eventId, "Handler", "Event"));

        await _sut.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        _dbContext.InboxMessages.Add(
            InboxMessage.Create(eventId, "Handler", "Event"));

        // Act
        var exception = await Assert.ThrowsAsync<UniqueConstraintViolationException>(
            () => _sut.SaveChangesAsync());

        // Assert
        exception.InnerException.Should().BeOfType<DbUpdateException>();
    }
}