using System.Text.Json;

namespace BuildingBlocks.Core.Outbox;

public sealed class OutboxMessage
{
	public Guid Id { get; private set; } = Guid.NewGuid();
	public string Payload { get; private set; } = default!;
	public string RoutingKey { get; private set; } = default!;
	public string EventType { get; private set; } = default!;

	public EOutboxMessageStatus Status { get; private set; } = EOutboxMessageStatus.PENDING;
	public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
	public DateTime? ProcessedAt { get; private set; }
	public DateTime? LastAttemptAt { get; private set; }
	public int Attempts { get; private set; }

	private OutboxMessage() { }

	public OutboxMessage(object @event, string routingKey)
	{
		Payload = JsonSerializer.Serialize(@event);
		EventType = @event.GetType().AssemblyQualifiedName!;
		RoutingKey = routingKey;
	}

	public void MarkAsProcessing()
	{
		Status = EOutboxMessageStatus.PROCESSING;
		LastAttemptAt = DateTime.UtcNow;
	}

	public void MarkAsCompleted()
	{
		Status = EOutboxMessageStatus.PROCESSED;
		ProcessedAt = DateTime.UtcNow;
	}

	public void MarkAsFailed()
	{
		Status = EOutboxMessageStatus.FAILED;
		LastAttemptAt = DateTime.UtcNow;
		Attempts++;
	}
}
