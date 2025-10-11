namespace SharedKernel;

public abstract class AggregateRoot : Entity
{
	private readonly List<DomainEvent> _domainEvents = [];
	public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

	public void AddDomainEvent(DomainEvent domainEvent)
		=> _domainEvents.Add(domainEvent);

	public void ClearDomainEvents() => _domainEvents.Clear();
}