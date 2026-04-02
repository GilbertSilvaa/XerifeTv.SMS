using SharedKernel.Exceptions;

namespace Plans.Domain.Exceptions;

public sealed class InvalidPlanNameException : DomainException
{
	private const string ERROR_CODE = "INVALID_PLAN_NAME";

    public InvalidPlanNameException(string name) : base(ERROR_CODE, $"The plan name '{name}' must be at least 5 characters long.") { }
}