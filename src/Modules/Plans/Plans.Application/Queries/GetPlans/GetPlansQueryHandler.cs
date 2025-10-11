using BuildingBlocks.Core.CQRS;
using Plans.Application.Contracts;
using Plans.Application.Contracts.DTOs;
using SharedKernel;

namespace Plans.Application.Queries.GetPlans;

internal sealed class GetPlansQueryHandler : IQueryHandler<GetPlansQuery, Result<IReadOnlyList<PlanDto>>>
{
	private readonly IPlansReadRepository _readRepository;

	public GetPlansQueryHandler(IPlansReadRepository readRepository)
	{
		_readRepository = readRepository;
	}

	public async Task<Result<IReadOnlyList<PlanDto>>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
	{
		var plans = await _readRepository.GetPlansAsync();
		return Result<IReadOnlyList<PlanDto>>.Success(plans);
	}
}