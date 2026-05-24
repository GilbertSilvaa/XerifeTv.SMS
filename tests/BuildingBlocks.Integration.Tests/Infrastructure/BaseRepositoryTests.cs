using BuildingBlocks.Integration.Tests.Fakes;
using BuildingBlocks.Integration.Tests.Infrastructure.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

[Collection("Postgres")]
public class BaseRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly FakeDbContext _dbContext;
    private readonly FakeRepository _repository;

    public BaseRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;

        var options = new DbContextOptionsBuilder<FakeDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _dbContext = new FakeDbContext(options);
        _repository = new FakeRepository(_dbContext);
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
    public async Task Shoud_AddEntityToDatabase_When_EntityIsValid()
    {
        // Arrange
        var entity = new FakeAggregate("Test Entity");

        // Act
        await _repository.AddOrUpdateAsync(entity);
        await _dbContext.SaveChangesAsync();

        // Assert
        var addedEntity = await _dbContext.FakeAggregates
            .FirstOrDefaultAsync(e => e.Id == entity.Id);

        addedEntity.Should().NotBeNull();
        addedEntity!.Name.Should().Be("Test Entity");
    }

    [Fact]
    public async Task Should_UpdateEntity_When_EntityAlreadyExists()
    {
        // Arrange
        var entity = new FakeAggregate("Old Name");

        await _repository.AddOrUpdateAsync(entity);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(entity).State = EntityState.Detached;

        var existingEntity = await _dbContext
            .FakeAggregates
            .FirstAsync(x => x.Id == entity.Id);

        existingEntity.UpdateName("New Name");

        // Act
        await _repository.AddOrUpdateAsync(existingEntity);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedEntity = await _dbContext
            .FakeAggregates
            .FirstAsync(x => x.Id == entity.Id);

        updatedEntity.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Should_ReturnCorrectCount_When_EntitiesExist()
    {
        // Arrange
        await _repository.AddOrUpdateAsync(
            new FakeAggregate("Entity 1"));

        await _repository.AddOrUpdateAsync(
            new FakeAggregate("Entity 2"));

        await _repository.AddOrUpdateAsync(
            new FakeAggregate("Entity 3"));

        await _dbContext.SaveChangesAsync();

        // Act
        var count = await _repository.CountAsync();

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task Should_IgnoreDeletedEntities_When_Counting()
    {
        // Arrange
        var entity1 = new FakeAggregate("Entity 1");
        var entity2 = new FakeAggregate("Entity 2");

        await _repository.AddOrUpdateAsync(entity1);
        await _repository.AddOrUpdateAsync(entity2);

        await _dbContext.SaveChangesAsync();

        await _repository.RemoveAsync(entity1.Id);
        await _dbContext.SaveChangesAsync();

        // Act
        var count = await _repository.CountAsync();

        // Assert
        count.Should().Be(1);
    }

    [Fact]
    public async Task Should_ReturnEntity_When_GetByIdExists()
    {
        // Arrange
        var entity = new FakeAggregate("Test");

        await _repository.AddOrUpdateAsync(entity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task Should_ReturnNull_When_GetByIdDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_ReturnNull_When_EntityWasDeleted()
    {
        // Arrange
        var entity = new FakeAggregate("Test");

        await _repository.AddOrUpdateAsync(entity);
        await _dbContext.SaveChangesAsync();

        await _repository.RemoveAsync(entity.Id);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(entity.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_SoftDeleteEntity_When_RemoveAsyncIsCalled()
    {
        // Arrange
        var entity = new FakeAggregate("Test");

        await _repository.AddOrUpdateAsync(entity);
        await _dbContext.SaveChangesAsync();

        // Act
        await _repository.RemoveAsync(entity.Id);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedEntity = await _dbContext
            .FakeAggregates
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == entity.Id);

        deletedEntity.Should().NotBeNull();
        deletedEntity!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Should_NotThrow_When_RemovingNonExistingEntity()
    {
        // Act
        var action = async () =>
        {
            await _repository.RemoveAsync(Guid.NewGuid());
            await _dbContext.SaveChangesAsync();
        };

        // Assert
        await action.Should().NotThrowAsync();
    }
}