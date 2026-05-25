using BuildingBlocks.Core.Events;

namespace BuildingBlocks.Integration.Tests.Fakes;

[EventMetadata("fake.test", 1.0)]
public record FakeIntegrationEvent(string Name, Guid ExcutionId, double Version = 1.0) : IntegrationEvent("fake.test", Version);
