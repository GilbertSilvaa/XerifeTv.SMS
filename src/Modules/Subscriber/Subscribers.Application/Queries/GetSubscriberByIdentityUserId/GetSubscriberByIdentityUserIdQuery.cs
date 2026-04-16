using BuildingBlocks.Core.CQRS;
using SharedKernel;
using Subscribers.Application.Queries.ReadModels;

namespace Subscribers.Application.Queries.GetSubscriberByIdentityUserId;

public sealed record GetSubscriberByIdentityUserIdQuery(Guid IdentityUserId) : IQuery<Result<SubscriberDto?>>;