namespace Plans.Domain.Exceptions;

public sealed class InvalidPlanNameException : Exception
{
	public InvalidPlanNameException(string name) : base($"The plan name '{name}' must be at least 5 characters long.") { }
}