namespace SharedKernel;

public abstract record DomainEvent
{
	public readonly Guid EventId = Guid.NewGuid();
	public readonly DateTime OccurredOn = DateTime.UtcNow;
	public string EventType => GetType().AssemblyQualifiedName!;
}