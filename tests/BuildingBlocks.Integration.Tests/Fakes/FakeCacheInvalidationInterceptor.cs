using BuildingBlocks.Core;
using BuildingBlocks.Infrastructure.Cache;

namespace BuildingBlocks.Integration.Tests.Fakes;

public sealed class FakeCacheInvalidationInterceptor : CacheInvalidationInterceptor<FakeAggregate>
{
    public List<FakeAggregate> InvalidatedItems { get; private set; } = [];

    public int ExecutionCount { get; private set; }

    public override Task InvalidateCacheAsync(List<FakeAggregate> changedItems)
    {
        ExecutionCount++;

        InvalidatedItems = changedItems;

        return Task.CompletedTask;
    }
}
