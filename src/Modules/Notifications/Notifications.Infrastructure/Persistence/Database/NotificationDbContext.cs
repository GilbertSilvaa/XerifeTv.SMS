using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Database;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Infrastructure.Persistence.Database;

public class NotificationDbContext : ApplicationDbContext
{
    public NotificationDbContext(
        DbContextOptions<NotificationDbContext> options,
        IDomainEventDispatcher eventPublisher) : base(options, eventPublisher) { }

    public DbSet<InboxMessage> InboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InboxConfiguration());
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InboxMessage>().ToTable("NotificationInboxMessages");
    }
}
