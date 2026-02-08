using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using Plans.Domain;
using SharedKernel;

namespace Plans.Application.Commands.AdjustSimultaneousScreensPlan;

internal sealed class AdjustSimultaneousScreensPlanCommandHandler : ICommandHandler<AdjustSimultaneousScreensPlanCommand, Result>
{
	private readonly IPlanRepository _repository;
	private readonly PlanService _domainService;
	public readonly IUnitOfWork<Plan> _unitOfWork;

    public AdjustSimultaneousScreensPlanCommandHandler(IPlanRepository repository,  IUnitOfWork<Plan> unitOfWork)
	{
		_repository = repository;
		_domainService = new PlanService(_repository);
		_unitOfWork = unitOfWork;
    }

	public async Task<Result> Handle(AdjustSimultaneousScreensPlanCommand request, CancellationToken cancellationToken)
	{
		var plan = await _repository.GetByIdAsync(request.Id);

		if (plan == null)
		{
			Error error = new("AdjustSimultaneousScreensPlanCommandHandler.Handle", "Plan not found in database.");
			return Result.Failure(error);
		}

		var planResult = await _domainService.AdjustPlanMaxSimultaneousScreensAsync(plan, request.MaxSimultaneousScreens);

		if (planResult.IsFailure)
			return Result.Failure(planResult.Error);

		await _repository.AddOrUpdateAsync(planResult.Data!);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
	}
}