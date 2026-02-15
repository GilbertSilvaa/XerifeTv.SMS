using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Subscribers.Domain.Entities;

namespace Subscribers.Infrastructure.Persistence.Database;

public class SubscriberDbContext : ApplicationDbContext
{
    public SubscriberDbContext(
        DbContextOptions<SubscriberDbContext> options,
        IDomainEventDispatcher eventPublisher) : base(options, eventPublisher) { }

    public DbSet<Subscriber> Subscribers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SubscriberConfiguration());
        modelBuilder.ApplyConfiguration(new SignatureConfiguration());
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Subscriber>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Signature>().HasQueryFilter(p => !p.IsDeleted);
    }
}
