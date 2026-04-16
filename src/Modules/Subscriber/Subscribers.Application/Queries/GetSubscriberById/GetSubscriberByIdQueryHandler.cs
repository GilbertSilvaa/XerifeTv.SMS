using BuildingBlocks.Core.CQRS;
using SharedKernel;
using Subscribers.Application.Queries.ReadModels;

namespace Subscribers.Application.Queries.GetSubscriberById;

internal sealed class GetSubscriberByIdQueryHandler : IQueryHandler<GetSubscriberByIdQuery, Result<SubscriberDto?>>
{
    private readonly ISubscribersReadRepository _readRepository;

    public GetSubscriberByIdQueryHandler(ISubscribersReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<SubscriberDto?>> Handle(GetSubscriberByIdQuery request, CancellationToken cancellationToken)
    {
        var subscriber = await _readRepository.GetSubscriberByIdAsync(request.Id);
        return Result<SubscriberDto?>.Success(subscriber);
    }
}