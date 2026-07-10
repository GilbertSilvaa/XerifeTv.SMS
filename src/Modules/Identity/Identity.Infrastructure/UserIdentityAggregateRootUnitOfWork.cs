using BuildingBlocks.Core;
using BuildingBlocks.Infrastructure;
using Identity.Infrastructure.Persistence.Database;

namespace Identity.Infrastructure;

public sealed class UserIdentityAggregateRootUnitOfWork(IdentityDbContext dbContext)
    : BaseUnitOfWork<IdentityDbContext, UserIdentityAggregateRoot>(dbContext), IUnitOfWork<UserIdentityAggregateRoot>;