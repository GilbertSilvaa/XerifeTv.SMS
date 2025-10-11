using SharedKernel;

namespace BuildingBlocks.Core;

public sealed class ValidationResult : Result
{
	private ValidationResult(Error[] errors) : base(false, new Error("ValidationResult"))
	{
		Errors = errors;
	}

	public Error[] Errors { get; }

	public static ValidationResult WithErrors(Error[] errors) => new(errors);
}