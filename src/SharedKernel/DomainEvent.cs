namespace SharedKernel;

public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public readonly DateTime OccurredOn = DateTime.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName!;
}