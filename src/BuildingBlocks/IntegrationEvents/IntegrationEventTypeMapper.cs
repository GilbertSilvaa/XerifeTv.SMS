using BuildingBlocks.Core.Events;
using System.Reflection;

namespace BuildingBlocks.IntegrationEvents;

public sealed class IntegrationEventTypeMapper
{
    private readonly Dictionary<string, Type> _eventTypeMappings = [];

    public IntegrationEventTypeMapper()
    {
        var integrationEventType = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IntegrationEvent)))
            .ToList();

        foreach (var type in integrationEventType)
        {
            var metadata = type.GetCustomAttribute<EventMetadataAttribute>();
            if (metadata is null) continue;

            var eventName = metadata.Name;

            if (!string.IsNullOrWhiteSpace(eventName))
                _eventTypeMappings[eventName] = type;
        }
    }

    public Type? GetEventTypeByName(string eventName)
    {
        if (_eventTypeMappings.TryGetValue(eventName, out var eventType))
            return eventType;

        return null;
    }
}
