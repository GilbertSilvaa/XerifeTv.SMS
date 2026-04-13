using SharedKernel;
using Subscribers.Domain.Entities;

namespace Subscribers.Domain.Repositories;

public interface ISubscriberRepository : IRepository<Subscriber>
{
    public Task<Subscriber?> GetByIdentityUserIdAsync(Guid identityUserId);
}