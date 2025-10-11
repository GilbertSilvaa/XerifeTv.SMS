using FluentValidation;

namespace Plans.Application.Commands.AdjustSimultaneousScreensPlan;

internal sealed class AdjustSimultaneousScreensPlanCommandValidator : AbstractValidator<AdjustSimultaneousScreensPlanCommand>
{
	public AdjustSimultaneousScreensPlanCommandValidator()
	{
		RuleFor(p => p.Id)
			.NotNull().WithMessage("Id is required")
			.NotEqual(Guid.Empty).WithMessage("Id cannot be Guid empty");

		RuleFor(p => p.MaxSimultaneousScreens)
			.GreaterThan(0).WithMessage("Screens must be greater than zero");
	}
}
