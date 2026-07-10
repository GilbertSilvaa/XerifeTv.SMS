using BuildingBlocks.Core;
using BuildingBlocks.Infrastructure;
using Notifications.Infrastructure.Persistence.Database;

namespace Notifications.Infrastructure;

public sealed class NotificationAggregationRootUnitOfWork(NotificationDbContext dbContext)
    : BaseUnitOfWork<NotificationDbContext, NotificationAggregateRoot>(dbContext), IUnitOfWork<NotificationAggregateRoot>;
