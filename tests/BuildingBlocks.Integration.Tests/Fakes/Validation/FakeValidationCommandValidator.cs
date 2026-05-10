using FluentValidation;

namespace BuildingBlocks.Integration.Tests.Fakes.Validation;

public sealed class FakeValidationCommandValidator : AbstractValidator<FakeValidationCommand>
{
    public FakeValidationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");
    }
}