using BuildingBlocks.Core.CQRS;
using SharedKernel;
using Subscribers.Application.Queries.ReadModels;

namespace Subscribers.Application.Queries.GetSubscriberByUserName;

public sealed record GetSubscriberByUserNameQuery(string UserName) : IQuery<Result<SubscriberDto?>>;