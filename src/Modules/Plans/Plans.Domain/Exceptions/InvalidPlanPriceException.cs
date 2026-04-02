using SharedKernel.Exceptions;

namespace Plans.Domain.Exceptions;

public sealed class InvalidPlanPriceException : DomainException
{
	private const string ERROR_CODE = "INVALID_PLAN_PRICE";

    public InvalidPlanPriceException() : base(ERROR_CODE, "The plan price must be greater than zero.") { }
}