using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using Plans.Domain;
using SharedKernel;

namespace Plans.Application.Commands.CreatePlan;

internal sealed class CreatePlanCommandHandler : ICommandHandler<CreatePlanCommand, Result>
{
	private readonly IPlanRepository _repository;
	private readonly PlanService _domainService;
	private readonly IUnitOfWork<Plan> _unitOfWork;

    public CreatePlanCommandHandler(IPlanRepository repository, IUnitOfWork<Plan> unitOfWork)
	{
		_repository = repository;
		_domainService = new PlanService(_repository);
		_unitOfWork = unitOfWork;
    }

	public async Task<Result> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
	{
		var planResult = await _domainService.CreatePLanAsync(request.Name, request.Description, request.Screens, request.Price);

		if (planResult.IsFailure)
			return Result.Failure(planResult.Error);

		await _repository.AddOrUpdateAsync(planResult.Data!);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
	}
}