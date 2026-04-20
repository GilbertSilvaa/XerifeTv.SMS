using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Plans.Domain;

namespace Plans.Infrastructure.Persistence.Database;

public class PlanDbContext : ApplicationDbContext
{
    public PlanDbContext(
        DbContextOptions<PlanDbContext> options,
        IDomainEventDispatcher eventPublisher) : base(options, eventPublisher) { }

    public DbSet<Plan> Plans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PlanConfiguration());
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Plan>().HasQueryFilter(p => !p.IsDeleted);
    }
}