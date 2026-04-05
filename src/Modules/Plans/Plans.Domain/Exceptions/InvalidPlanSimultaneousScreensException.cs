using SharedKernel.Exceptions;

namespace Plans.Domain.Exceptions;

public sealed class InvalidPlanSimultaneousScreensException : DomainException
{
	private const string ERROR_CODE = "Plan.InvalidQtyScreens";

    public InvalidPlanSimultaneousScreensException(int screens) : base(ERROR_CODE, $"The number of simultaneous screens in a plan must be at least one. Value: {screens}") { }
}