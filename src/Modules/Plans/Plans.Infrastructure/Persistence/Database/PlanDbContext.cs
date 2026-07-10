using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Core.Messaging.Outbox;
using BuildingBlocks.Infrastructure.Database;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Plans.Domain;

namespace Plans.Infrastructure.Persistence.Database;

public class PlanDbContext : ApplicationDbContext
{
    public PlanDbContext(
        DbContextOptions<PlanDbContext> options,
        IDomainEventDispatcher eventPublisher) : base(options, eventPublisher) { }

    public DbSet<Plan> Plans { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PlanConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxConfiguration());
        modelBuilder.ApplyConfiguration(new InboxConfiguration());
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Plan>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<OutboxMessage>().ToTable("PlanOutboxMessages");
        modelBuilder.Entity<InboxMessage>().ToTable("PlanInboxMessages");
    }
}