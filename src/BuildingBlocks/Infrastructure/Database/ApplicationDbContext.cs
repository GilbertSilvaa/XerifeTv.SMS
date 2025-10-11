using BuildingBlocks.Core.Messaging;
using SharedKernel;

namespace BuildingBlocks.Infrastructure.Database;

public class ApplicationDbContext : DbContext
{
	private readonly IDomainEventPublisher _eventPublisher;

	public ApplicationDbContext(DbContextOptions options, IDomainEventPublisher eventPublisher) : base(options)
	{
		_eventPublisher = eventPublisher;
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;

		foreach (var entry in ChangeTracker.Entries<Entity>())
			entry.Entity.SetTimestamps(now);

		var entitiesWithEvents = ChangeTracker
			.Entries<AggregateRoot>()
			.Where(e => e.Entity.DomainEvents.Any())
			.Select(e => e.Entity)
			.ToList();

		var result = await base.SaveChangesAsync(cancellationToken);

		var tasks = entitiesWithEvents
			.SelectMany(e => e.DomainEvents)
			.Select(domainEvent => _eventPublisher.PublishAsync(domainEvent, cancellationToken));

		await Task.WhenAll(tasks);

		entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

		return result;
	}
}