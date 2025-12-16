using System.Text.Json;

namespace BuildingBlocks.Core.Events;

public sealed class IntegrationEventEnvelope
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = default!;
    public string EventName { get; set; } = default!;
    public double Version { get; set; }
    public DateTime OccurredAOn { get; set; }
    public string Payload { get; set; } = default!;

    public IntegrationEventEnvelope() { }

    public static IntegrationEventEnvelope MapFromIntegrationEvent<T>(T @event) where T : IntegrationEvent
    {
        return new ()
        {
            EventId = @event.EventId,
            EventType = @event.EventType,
            EventName = @event.EventName,
            Version = @event.Version,
            OccurredAOn = @event.OccurredOn,
            Payload = JsonSerializer.Serialize(@event)
        };
    }
}