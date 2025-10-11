using FluentValidation;

namespace Plans.Application.Commands.DeletePlan;

internal sealed class DeletePlanCommandValidator : AbstractValidator<DeletePlanCommand>
{
	public DeletePlanCommandValidator()
	{
		RuleFor(p => p.Id)
			.NotNull().WithMessage("Id is required")
			.NotEqual(Guid.Empty).WithMessage("Id cannot be Guid empty");
	}
}