using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Outbox;
using BuildingBlocks.Infrastructure.Database;

namespace BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence.Database;

public class OutboxDbContext : ApplicationDbContext
{
	public OutboxDbContext(
		DbContextOptions<OutboxDbContext> options,
		IDomainEventDispatcher eventPublisher) : base(options, eventPublisher) { }

	public DbSet<OutboxMessage> OutboxMessages { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new OutboxConfiguration());
		base.OnModelCreating(modelBuilder);
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return await base.SaveChangesAsync(cancellationToken);
	}
}
