namespace BuildingBlocks.Core.Messaging.Inbox;

public sealed class InboxMessage
{
    public Guid EventId { get; private set; }
    public string HandlerKey { get; private set; } = default!;
    public string EventType { get; private set; } = default!;
    public DateTime? ReceivedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public EInboxMessageStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }

    private InboxMessage() { }

    public static InboxMessage Create(Guid eventId, string handlerKey, string eventType)
    {
        return new()
        {
            EventId = eventId,
            HandlerKey = handlerKey,
            EventType = eventType,
            Status = EInboxMessageStatus.PENDING,
            ReceivedAt = DateTime.UtcNow
        };
    }

    public void MarkAsProcessed()
    {
        Status = EInboxMessageStatus.PROCESSED;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = EInboxMessageStatus.FAILED;
        ErrorMessage = errorMessage;
        ProcessedAt = DateTime.UtcNow;
    }

    public bool IsLockExpired(TimeSpan lockTimeout, DateTime utcNow) =>
        Status == EInboxMessageStatus.PENDING
        && ReceivedAt.HasValue
        && (utcNow - ReceivedAt.Value) > lockTimeout;
}