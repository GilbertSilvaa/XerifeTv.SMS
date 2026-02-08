using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using Plans.Domain;
using SharedKernel;

namespace Plans.Application.Commands.DeletePlan;

internal sealed class DeletePlanCommandHandler : ICommandHandler<DeletePlanCommand, Result>
{
	private readonly IPlanRepository _repository;
	private readonly IUnitOfWork<Plan> _unitOfWork;

    public DeletePlanCommandHandler(IPlanRepository repository, IUnitOfWork<Plan> unitOfWork)
	{
		_repository = repository;
		_unitOfWork = unitOfWork;
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
		await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
	}
}