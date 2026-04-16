using BuildingBlocks.Core.CQRS;
using SharedKernel;
using Subscribers.Application.Queries.ReadModels;

namespace Subscribers.Application.Queries.GetSubscriberByIdentityUserId;

internal sealed class GetSubscriberByIdentityUserIdQueryHandler : IQueryHandler<GetSubscriberByIdentityUserIdQuery, Result<SubscriberDto?>>
{
    private readonly ISubscribersReadRepository _readRepository;

    public GetSubscriberByIdentityUserIdQueryHandler(ISubscribersReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<SubscriberDto?>> Handle(GetSubscriberByIdentityUserIdQuery request, CancellationToken cancellationToken)
    {
        var subscriber = await _readRepository.GetSubscriberByIdentityUserIdAsync(request.IdentityUserId);
        return Result<SubscriberDto?>.Success(subscriber);
    }
}