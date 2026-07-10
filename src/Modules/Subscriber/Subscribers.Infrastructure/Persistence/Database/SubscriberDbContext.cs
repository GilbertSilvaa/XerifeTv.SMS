using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Core.Messaging.Outbox;
using BuildingBlocks.Infrastructure.Database;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Subscribers.Domain.Entities;

namespace Subscribers.Infrastructure.Persistence.Database;

public class SubscriberDbContext : ApplicationDbContext
{
    public SubscriberDbContext(
        DbContextOptions<SubscriberDbContext> options,
        IDomainEventDispatcher eventPublisher) : base(options, eventPublisher) { }

    public DbSet<Subscriber> Subscribers { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SubscriberConfiguration());
        modelBuilder.ApplyConfiguration(new SignatureConfiguration());
        modelBuilder.ApplyConfiguration(new PlanCatalogConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxConfiguration());
        modelBuilder.ApplyConfiguration(new InboxConfiguration());
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Subscriber>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Signature>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<OutboxMessage>().ToTable("SubscriberOutboxMessages");
        modelBuilder.Entity<InboxMessage>().ToTable("SubscriberInboxMessages");
    }
}
