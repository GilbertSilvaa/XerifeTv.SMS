using SharedKernel;
using Subscribers.Domain.Entities;

namespace Subscribers.Domain.Repositories;

public interface ISubscribersRepository : IRepository<Subscriber>
{
    public Task<Subscriber?> GetByIdentityUserIdAsync(Guid identityUserId);
}