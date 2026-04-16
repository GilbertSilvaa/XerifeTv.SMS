using BuildingBlocks.Core.CQRS;
using SharedKernel;
using Subscribers.Application.Queries.ReadModels;

namespace Subscribers.Application.Queries.GetSubscriberByEmail;

internal sealed class GetSubscriberByEmailQueryHandler : IQueryHandler<GetSubscriberByEmailQuery, Result<SubscriberDto?>>
{
    private readonly ISubscribersReadRepository _readRepository;

    public GetSubscriberByEmailQueryHandler(ISubscribersReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<SubscriberDto?>> Handle(GetSubscriberByEmailQuery request, CancellationToken cancellationToken)
    {
        var subscriber = await _readRepository.GetSubscriberByEmailAsync(request.Email);
        return Result<SubscriberDto?>.Success(subscriber);
    }
}