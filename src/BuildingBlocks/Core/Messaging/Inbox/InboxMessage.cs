namespace BuildingBlocks.Core.Messaging.Inbox;

public sealed class InboxMessage
{
    public Guid EventId { get; private set; }
    public string HandlerKey { get; private set; } = default!;
    public string EventType { get; private set; } = default!;
    public DateTime ProcessedAt { get; private set; } = DateTime.UtcNow;

    private InboxMessage() { }

    public static InboxMessage Create(Guid eventId, string handlerKey, string eventType)
    {
        return new()
        {
            EventId = eventId,
            HandlerKey = handlerKey,
            EventType = eventType
        };
    }
}