using BuildingBlocks.Core.CQRS;
using Plans.Application.Abstractions;
using Plans.Application.Abstractions.DTOs;
using SharedKernel;

namespace Plans.Application.Queries.GetPlanById;

internal sealed class GetPlanByIdQueryHandler : IQueryHandler<GetPlanByIdQuery, Result<PlanDto?>>
{
	private readonly IPlansReadRepository _readRepository;

	public GetPlanByIdQueryHandler(IPlansReadRepository readRepository)
	{
		_readRepository = readRepository;
	}

	public async Task<Result<PlanDto?>> Handle(GetPlanByIdQuery request, CancellationToken cancellationToken)
	{
		var plan = await _readRepository.GetPlanByIdAsync(request.Id);
		return Result<PlanDto?>.Success(plan);
	}
}