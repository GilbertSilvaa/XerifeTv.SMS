using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace BuildingBlocks.Integration.Tests.Fakes;

public class FakeCommandHandler : ICommandHandler<FakeIdempotentCommand, Result>
{
    public static int ExecutionCount = 0;

    public async Task<Result> Handle(FakeIdempotentCommand request, CancellationToken cancellationToken)
    {
        ExecutionCount++;

        await Task.Delay(1500, cancellationToken);

        return Result.Success();
    }
}
