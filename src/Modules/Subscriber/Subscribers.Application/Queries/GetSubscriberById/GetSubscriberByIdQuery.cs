using BuildingBlocks.Core.CQRS;
using SharedKernel;
using Subscribers.Application.Queries.ReadModels;

namespace Subscribers.Application.Queries.GetSubscriberById;

public sealed record GetSubscriberByIdQuery(Guid Id) : IQuery<Result<SubscriberDto?>>;