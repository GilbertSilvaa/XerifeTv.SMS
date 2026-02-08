using BuildingBlocks.Core;
using BuildingBlocks.Infrastructure;
using Plans.Domain;
using Plans.Infrastructure.Persistence.Database;

namespace Plans.Infrastructure.Persistence;

public sealed class PlanUnitOfWork(PlanDbContext dbContext)
    : BaseUnitOfWork<PlanDbContext, Plan>(dbContext), IUnitOfWork<Plan>;
