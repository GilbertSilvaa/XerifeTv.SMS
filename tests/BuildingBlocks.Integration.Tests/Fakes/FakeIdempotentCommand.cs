using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace BuildingBlocks.Integration.Tests.Fakes;

public record FakeIdempotentCommand(Guid Id, string Value) : IIdempotentCommand<Result>
{
    public string IdempotencyKey => $"FAKE_{Id}-{Value}";
}

public record FakeResponse(Guid ExecutionId);