using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace BuildingBlocks.Behaviors;

public sealed class ValidationPipelineBehavior<TRequest, TResponse>
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : ICommand<TResponse>
	where TResponse : Result
{
	private readonly IEnumerable<IValidator<TRequest>> _validators;

	public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
	{
		_validators = validators;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		if (!_validators.Any())
			return await next(cancellationToken);

		Error[] errors = [.. _validators
			.Select(validator => validator.Validate(request))
			.SelectMany(validator => validator.Errors)
			.Where(validationFailure => validationFailure != null)
			.Select(failure => new Error(failure.PropertyName, failure.ErrorMessage))
			.Distinct()];

		if (errors.Length > 0)
			return (ValidationResult.WithErrors(errors) as TResponse)!;

		return await next(cancellationToken);
	}
}