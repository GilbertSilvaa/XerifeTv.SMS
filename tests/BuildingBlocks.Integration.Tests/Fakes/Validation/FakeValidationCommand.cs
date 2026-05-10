using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace BuildingBlocks.Integration.Tests.Fakes.Validation;

public record FakeValidationCommand(string Name) : ICommand<Result>;
