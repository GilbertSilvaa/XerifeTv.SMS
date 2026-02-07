using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Database;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;

public class InboxDbContext : ApplicationDbContext
{
    public InboxDbContext(
        DbContextOptions<InboxDbContext> options, 
        IDomainEventDispatcher eventPublisher) : base(options, eventPublisher) { }

    public DbSet<InboxMessage> InboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InboxConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
