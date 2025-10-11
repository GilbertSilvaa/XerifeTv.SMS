using FluentValidation;

namespace Plans.Application.Commands.CreatePlan;

internal sealed class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
	public CreatePlanCommandValidator()
	{
		RuleFor(p => p.Name)
		   .NotEmpty().WithMessage("Name is required")
		   .MaximumLength(15).WithMessage("Name must be at most 15 characters");

		RuleFor(p => p.Description)
			.NotEmpty().WithMessage("Description is required")
			.MaximumLength(200).WithMessage("Description must be at most 200 characters");

		RuleFor(p => p.Screens)
			.GreaterThan(0).WithMessage("Screens must be greater than zero");

		RuleFor(p => p.Price)
			.NotNull().WithMessage("Price is required")
			.Must(m => m.Amount > 0).WithMessage("Price amount must be greater than zero")
			.Must(m => !string.IsNullOrWhiteSpace(m.Currency)).WithMessage("Currency is required");
	}
}
