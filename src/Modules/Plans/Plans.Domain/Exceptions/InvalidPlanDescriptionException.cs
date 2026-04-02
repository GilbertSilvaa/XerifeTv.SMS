using SharedKernel.Exceptions;

namespace Plans.Domain.Exceptions;

public sealed class InvalidPlanDescriptionException : DomainException
{
	private const string ERROR_CODE = "INVALID_PLAN_DESCRIPTION";

    public InvalidPlanDescriptionException() : base(ERROR_CODE, "The plan description must be at least 10 characters long.") { }
}