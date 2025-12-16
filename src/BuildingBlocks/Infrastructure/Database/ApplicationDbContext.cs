using BuildingBlocks.Core.Messaging;
using SharedKernel;

namespace BuildingBlocks.Infrastructure.Database;

public class ApplicationDbContext : DbContext
{
	private readonly IDomainEventDispatcher _domainEventPublisher;

	public ApplicationDbContext(DbContextOptions options, IDomainEventDispatcher eventPublisher) : base(options)
	{
		_domainEventPublisher = eventPublisher;
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
			.Select(domainEvent => _domainEventPublisher.DispatchAsync(domainEvent, cancellationToken));

		await Task.WhenAll(tasks);

		entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

		return result;
	}
}