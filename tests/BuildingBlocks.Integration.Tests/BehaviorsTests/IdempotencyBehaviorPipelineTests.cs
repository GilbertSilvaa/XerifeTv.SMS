using BuildingBlocks.Behaviors;
using BuildingBlocks.Core;
using BuildingBlocks.Infrastructure.Cache;
using BuildingBlocks.Integration.Tests.Fakes;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace BuildingBlocks.Integration.Tests.BehaviorsTests;

public class IdempotencyBehaviorPipelineTests
{
    private readonly IMediator _mediator;

    public IdempotencyBehaviorPipelineTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<FakeIdempotentCommand>();
        });

        services.AddTransient<
            IPipelineBehavior<FakeIdempotentCommand, Result>,
            IdempotencyPipelineBehavior<FakeIdempotentCommand, Result>>();

        services.AddDistributedMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Should_ReturnSameResponse_When_SameIdempotencyKeyIsUsed()
    {
        // Arrange
        FakeCommandHandler.ExecutionCount = 0;

        var command = new FakeIdempotentCommand(Guid.NewGuid(), Guid.NewGuid().ToString());

        // Act
        var result1 = await _mediator.Send(command);
        var result2 = await _mediator.Send(command);

        // Assert
        result1.Should().BeEquivalentTo(result2);

        FakeCommandHandler.ExecutionCount.Should().Be(1);
    }

    [Fact]
    public async Task Should_ExecuteHandlerOnlyOnce_When_ConcurrentRequestsUseSameIdempotencyKey()
    {
        // Arrange
        FakeCommandHandler.ExecutionCount = 0;

        var command = new FakeIdempotentCommand(Guid.NewGuid(), Guid.NewGuid().ToString());

        var gate = new TaskCompletionSource();

        async Task<Result> Send()
        {
            await gate.Task;

            return await _mediator.Send(command);
        }

        var task1 = Send();
        var task2 = Send();

        gate.SetResult();

        // Act
        var results = await Task.WhenAll(task1, task2);

        // Assert
        results[0].Should().BeEquivalentTo(results[1]);

        FakeCommandHandler.ExecutionCount.Should().Be(1);
    }
}
