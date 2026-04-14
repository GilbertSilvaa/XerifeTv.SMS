using BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;
using Subscribers.Infrastructure.Persistence.Database;

namespace Subscribers.Infrastructure.Persistence.Repositories;

public sealed class SubscriberRepository : BaseRepository<Subscriber>, ISubscriberRepository
{
    public SubscriberRepository(SubscriberDbContext dbContext) : base(dbContext) { }

    public async Task<Subscriber?> GetByIdentityUserIdAsync(Guid identityUserId)
    {
        return await _dataSet
            .Include(x => x.Signatures)
            .SingleOrDefaultAsync(e => e.IdentityUserId == identityUserId && !e.IsDeleted);
    }
}