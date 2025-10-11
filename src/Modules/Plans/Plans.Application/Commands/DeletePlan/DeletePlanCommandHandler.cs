using BuildingBlocks.Core.CQRS;
using Plans.Domain;
using SharedKernel;

namespace Plans.Application.Commands.DeletePlan;

internal sealed class DeletePlanCommandHandler : ICommandHandler<DeletePlanCommand, Result>
{
	private readonly IPlanRepository _repository;

	public DeletePlanCommandHandler(IPlanRepository repository)
	{
		_repository = repository;
	}

	public async Task<Result> Handle(DeletePlanCommand request, CancellationToken cancellationToken)
	{
		var plan = await _repository.GetByIdAsync(request.Id);

		if (plan == null)
		{
			Error error = new("DeletePlanCommandHandler.Handle", "Plan not found in database.");
			return Result.Failure(error);
		}

		await _repository.RemoveAsync(plan.Id);

		return Result.Success();
	}
}