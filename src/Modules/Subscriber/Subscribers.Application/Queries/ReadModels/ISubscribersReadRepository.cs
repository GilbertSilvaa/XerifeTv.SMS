using BuildingBlocks.Core.Pagination;

namespace Subscribers.Application.Queries.ReadModels;

public interface ISubscribersReadRepository
{
    Task<PagedList<SubscriberDto>> GetSubscribersAsync(PagedQuery query);
    Task<SubscriberDto?> GetSubscriberByIdAsync(Guid id);
    Task<SubscriberDto?> GetSubscriberByEmailAsync(string email);
    Task<SubscriberDto?> GetSubscriberByUserNameAsync(string userName);
    Task<SubscriberDto?> GetSubscriberByIdentityUserIdAsync(Guid identityUserId);
}
