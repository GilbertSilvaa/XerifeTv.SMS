using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Pagination;
using SharedKernel;
using Subscribers.Application.Queries.ReadModels;

namespace Subscribers.Application.Queries.GetSubscribers;

internal sealed class GetSubscribersQueryHandler : IQueryHandler<GetSubscribersQuery, Result<PagedList<SubscriberDto>>>
{
    private readonly ISubscribersReadRepository _readRepository;

    public GetSubscribersQueryHandler(ISubscribersReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<PagedList<SubscriberDto>>> Handle(GetSubscribersQuery request, CancellationToken cancellationToken)
    {
        var subscribersPagined = await _readRepository.GetSubscribersAsync(request.Query);
        return Result<PagedList<SubscriberDto>>.Success(subscribersPagined);
    }
}
