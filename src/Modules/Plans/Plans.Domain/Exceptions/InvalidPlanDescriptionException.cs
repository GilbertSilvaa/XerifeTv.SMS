namespace Plans.Domain.Exceptions;

public sealed class InvalidPlanDescriptionException : Exception
{
	public InvalidPlanDescriptionException() : base("The plan description must be at least 10 characters long.") { }
}