namespace BuildingBlocks.Core.Events;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EventMetadataAttribute : Attribute
{
    public string Name { get; }
    public double Version { get; }

    public EventMetadataAttribute(string name, double version = 1.0)
    {
        Name = name;
        Version = version;
    }
}