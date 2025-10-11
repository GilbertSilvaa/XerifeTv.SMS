namespace Plans.Domain.Exceptions;

public sealed class InvalidPlanSimultaneousScreensException : Exception
{
	public InvalidPlanSimultaneousScreensException(int screens) : base($"The number of simultaneous screens in a plan must be at least one. Value: {screens}") { }
}