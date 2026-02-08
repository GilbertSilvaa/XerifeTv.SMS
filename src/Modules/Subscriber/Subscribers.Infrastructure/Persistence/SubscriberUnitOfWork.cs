using BuildingBlocks.Core;
using BuildingBlocks.Infrastructure;
using Subscribers.Domain.Entities;
using Subscribers.Infrastructure.Persistence.Database;

namespace Subscribers.Infrastructure.Persistence;

public sealed class SubscriberUnitOfWork(SubscriberDbContext dbContext)
    : BaseUnitOfWork<SubscriberDbContext, Subscriber>(dbContext), IUnitOfWork<Subscriber>;