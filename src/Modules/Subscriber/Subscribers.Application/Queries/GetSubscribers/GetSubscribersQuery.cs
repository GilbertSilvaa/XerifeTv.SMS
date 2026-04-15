using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Pagination;
using SharedKernel;
using Subscribers.Application.Queries.ReadModels;

namespace Subscribers.Application.Queries.GetSubscribers;

public sealed record GetSubscribersQuery(PagedQuery Query) : IQuery<Result<PagedList<SubscriberDto>>>;