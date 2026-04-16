using BuildingBlocks.Core.CQRS;
using SharedKernel;
using Subscribers.Application.Queries.ReadModels;

namespace Subscribers.Application.Queries.GetSubscriberByEmail;

public sealed record GetSubscriberByEmailQuery(string Email) : IQuery<Result<SubscriberDto?>>;