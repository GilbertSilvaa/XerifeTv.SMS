using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace BuildingBlocks.Integration.Tests.Fakes.Validation;

public sealed class FakeValidationCommandHandler : ICommandHandler<FakeValidationCommand, Result>
{
    public Task<Result> Handle(FakeValidationCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }
}
