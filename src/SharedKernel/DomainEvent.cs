namespace SharedKernel;

public abstract record DomainEvent
{
    public Guid EventId = Guid.NewGuid();
    public readonly DateTime OccurredOn = DateTime.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName!;

    public void SetEventId(Guid eventId)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId cannot be empty.", nameof(eventId));

        EventId = eventId;
    }
}