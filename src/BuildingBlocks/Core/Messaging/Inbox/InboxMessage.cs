namespace BuildingBlocks.Core.Messaging.Inbox;

public sealed class InboxMessage
{
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = default!;
    public DateTime ProcessedAt { get; private set; } = DateTime.UtcNow;

    private InboxMessage() { }

    public static InboxMessage Create(Guid eventId, string eventType)
    {
        return new()
        {
            EventId = eventId,
            EventType = eventType
        };
    }
}