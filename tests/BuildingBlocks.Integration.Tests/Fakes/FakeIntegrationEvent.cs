using BuildingBlocks.Core.Events;

namespace BuildingBlocks.Integration.Tests.Fakes;

[EventMetadata("fake.test", 1.0)]
public record FakeIntegrationEvent(string Name, Guid ExcutionId, double Version = 1.0) : IntegrationEvent("fake.test", Version);

[EventMetadata("fake.test.not.mapped    ", 1.0)]
public record FakeIntegrationEventNotMapped(string Name, Guid ExcutionId, double Version = 1.0) : IntegrationEvent("fake.test.not.mapped", Version);