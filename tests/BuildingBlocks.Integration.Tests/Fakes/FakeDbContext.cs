using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Core.Messaging.Outbox;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace BuildingBlocks.Integration.Tests.Fakes;

public sealed class FakeDbContext : DbContext
{
    public DbSet<FakeAggregate> FakeAggregates { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }

    public FakeDbContext(DbContextOptions<FakeDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxConfiguration());
        modelBuilder.ApplyConfiguration(new InboxConfiguration());
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FakeAggregate>().HasKey(f => f.Id);
        modelBuilder.Entity<OutboxMessage>();
        modelBuilder.Entity<InboxMessage>();
        modelBuilder.Ignore<DomainEvent>();
    }
}