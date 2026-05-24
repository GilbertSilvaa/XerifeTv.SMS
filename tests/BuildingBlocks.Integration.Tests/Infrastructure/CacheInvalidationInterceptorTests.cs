using BuildingBlocks.Integration.Tests.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

public class CacheInvalidationInterceptorTests
{
    private readonly FakeDbContext _dbContext;
    private readonly FakeCacheInvalidationInterceptor _interceptor;

    public CacheInvalidationInterceptorTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton<FakeCacheInvalidationInterceptor>();

        var provider = services.BuildServiceProvider();

        _interceptor = provider.GetRequiredService<FakeCacheInvalidationInterceptor>();

        var options = new DbContextOptionsBuilder<FakeDbContext>()
            .UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
            .AddInterceptors(_interceptor)
            .Options;

        _dbContext = new FakeDbContext(options);
    }

    [Fact]
    public async Task Should_Invalidate_When_EntityIsAdded()
    {
        // Arrange
        var entity = new FakeAggregate("Movie");
        _dbContext.Add(entity);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _interceptor.ExecutionCount
            .Should()
            .Be(1);

        _interceptor.InvalidatedItems
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(entity);
    }

    [Fact]
    public async Task Should_Invalidate_When_EntityIsModified()
    {
        // Arrange
        var entity = new FakeAggregate("Movie");
        _dbContext.Add(entity);

        await _dbContext.SaveChangesAsync();

        entity.UpdateName("Updated");

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _interceptor.ExecutionCount
            .Should()
            .Be(2);

        _interceptor.InvalidatedItems
            .Should()
            .Contain(entity);
    }

    [Fact]
    public async Task Should_Invalidate_When_EntityIsDeleted()
    {
        // Arrange
        var entity = new FakeAggregate("Movie");
        _dbContext.Add(entity);

        await _dbContext.SaveChangesAsync();

        _dbContext.Remove(entity);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _interceptor.ExecutionCount
            .Should()
            .Be(2);

        _interceptor.InvalidatedItems
            .Should()
            .Contain(entity);
    }

    [Fact]
    public async Task Should_Not_Invalidate_When_NoChangesExist()
    {
        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _interceptor.ExecutionCount
            .Should()
            .Be(0);

        _interceptor.InvalidatedItems
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task Should_Invalidate_AllChangedEntities()
    {
        // Arrange
        var entity1 = new FakeAggregate("Movie1");
        var entity2 = new FakeAggregate("Movie2");

        _dbContext.AddRange(entity1, entity2);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _interceptor.ExecutionCount
            .Should()
            .Be(1);

        _interceptor.InvalidatedItems
            .Should()
            .HaveCount(2);

        _interceptor.InvalidatedItems
            .Should()
            .Contain(entity1);

        _interceptor.InvalidatedItems
            .Should()
            .Contain(entity2);
    }

    [Fact]
    public async Task Should_Not_Invalidate_DetachedEntities()
    {
        // Arrange
        var entity = new FakeAggregate("Movie");

        _dbContext.Entry(entity).State = EntityState.Detached;

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _interceptor.ExecutionCount
            .Should()
            .Be(0);

        _interceptor.InvalidatedItems
            .Should()
            .BeEmpty();
    }
}