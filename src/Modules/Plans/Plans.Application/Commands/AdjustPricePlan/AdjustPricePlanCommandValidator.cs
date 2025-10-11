using FluentValidation;

namespace Plans.Application.Commands.AdjustPricePlan;

internal sealed class AdjustPricePlanCommandValidator : AbstractValidator<AdjustPricePlanCommand>
{
	public AdjustPricePlanCommandValidator()
	{
		RuleFor(p => p.Id)
			.NotNull().WithMessage("Id is required")
			.NotEqual(Guid.Empty).WithMessage("Id cannot be Guid empty");

		RuleFor(p => p.Price)
			.NotNull().WithMessage("Price is required")
			.Must(m => m.Amount > 0).WithMessage("Price amount must be greater than zero")
			.Must(m => !string.IsNullOrWhiteSpace(m.Currency)).WithMessage("Currency is required");
	}
}
