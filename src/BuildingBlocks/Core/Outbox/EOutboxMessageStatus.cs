namespace BuildingBlocks.Core.Outbox;

public enum EOutboxMessageStatus
{
	PENDING = 0,
	PROCESSING = 1,
	PROCESSED = 2,
	FAILED = 3
}