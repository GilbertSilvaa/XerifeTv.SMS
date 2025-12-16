using BuildingBlocks.Infrastructure;
using Subscribers.Domain;
using Subscribers.Domain.Entities;
using Subscribers.Infrastructure.Persistence.Database;

namespace Subscribers.Infrastructure.Persistence.Repositories;

public sealed class SubscriberRepository : BaseRepository<Subscriber>, ISubscriberRepository
{
    public SubscriberRepository(SubscriberDbContext dbContext) : base(dbContext) { }
}