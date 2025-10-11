namespace Plans.Domain.Exceptions;

public sealed class InvalidPlanPriceException : Exception
{
	public InvalidPlanPriceException() : base("The plan price must be greater than zero.") { }
}