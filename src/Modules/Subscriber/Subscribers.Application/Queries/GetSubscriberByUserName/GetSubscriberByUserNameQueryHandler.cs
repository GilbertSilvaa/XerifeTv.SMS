using BuildingBlocks.Core.CQRS;
using SharedKernel;
using Subscribers.Application.Queries.ReadModels;

namespace Subscribers.Application.Queries.GetSubscriberByUserName;

internal sealed class GetSubscriberByUserNameQueryHandler : IQueryHandler<GetSubscriberByUserNameQuery, Result<SubscriberDto?>>
{
    private readonly ISubscribersReadRepository _readRepository;

    public GetSubscriberByUserNameQueryHandler(ISubscribersReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<SubscriberDto?>> Handle(GetSubscriberByUserNameQuery request, CancellationToken cancellationToken)
    {
        var subscriber = await _readRepository.GetSubscriberByUserNameAsync(request.UserName);
        return Result<SubscriberDto?>.Success(subscriber);
    }
}
