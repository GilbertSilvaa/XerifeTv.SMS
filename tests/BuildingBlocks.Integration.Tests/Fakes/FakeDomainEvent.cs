using SharedKernel;

namespace BuildingBlocks.Integration.Tests.Fakes;

public record FakeDomainEvent(Guid ExecutionId, string Name) : DomainEvent;
