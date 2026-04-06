using BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;

namespace Subscribers.Infrastructure.Persistence.Repositories;

public sealed class PlanCatalogRepository : BaseRepository<PlanItemCatalog>, IPlanCatalogRepository
{
    public PlanCatalogRepository(DbContext dbContext) : base(dbContext) { }
}