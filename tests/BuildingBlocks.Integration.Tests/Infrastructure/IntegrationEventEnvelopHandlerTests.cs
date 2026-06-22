using BuildingBlocks.Core.Events;
using BuildingBlocks.Infrastructure.Messaging.Consumers;
using BuildingBlocks.Integration.Tests.Fakes;
using BuildingBlocks.IntegrationEvents;
using FluentAssertions;
using MediatR;
using Moq;
using System.Text.Json;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

public class IntegrationEventEnvelopeHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IntegrationEventTypeMapper _eventTypeMapper;
    private readonly IntegrationEventEnvelopeHandler _handler;

    public IntegrationEventEnvelopeHandlerTests()
    {
        _mediatorMock = new Mock<IMediator>();

        _eventTypeMapper = new IntegrationEventTypeMapper(new()
        {
            ["fake.test"] = typeof(FakeIntegrationEvent)
        });

        _handler = new IntegrationEventEnvelopeHandler(
            _mediatorMock.Object,
            _eventTypeMapper);
    }

    [Fact]
    public async Task Should_PublishIntegrationEvent_When_EventIsValid()
    {
        // Arrange
        var integrationEvent = new FakeIntegrationEvent("Test Event", Guid.NewGuid());
        var envelope = IntegrationEventEnvelope.MapFromIntegrationEvent(integrationEvent);

        // Act
        await _handler.Handle(envelope, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(
                It.Is<IntegrationEvent>(e =>
                    e.GetType() == typeof(FakeIntegrationEvent) &&
                    e.EventName == integrationEvent.EventName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_EventTypeIsNotRecognized()
    {
        // Arrange
        var integrationEvent = new FakeIntegrationEventNotMapped("Test Event", Guid.NewGuid());
        var envelope = IntegrationEventEnvelope.MapFromIntegrationEvent(integrationEvent);

        // Act
        Func<Task> action = () => _handler.Handle(envelope, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not recognized*");
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_DeserializationFails()
    {
        // Arrange
        var integrationEvent = new FakeIntegrationEvent("Test Event", Guid.NewGuid());
        var envelope = IntegrationEventEnvelope.MapFromIntegrationEvent(integrationEvent);
        envelope.Payload = "Invalid JSON";

        // Act
        var action = () => _handler.Handle(envelope, CancellationToken.None);

        // Assert
        var exception = await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Deserialization of integration event '{integrationEvent.EventName}' failed.");

        exception.Which.InnerException.Should().BeOfType<JsonException>();
    }

    [Fact]
    public async Task Should_WrapMediatorException()
    {
        // Arrange
        var integrationEvent = new FakeIntegrationEvent("Test Event", Guid.NewGuid());
        var envelope = IntegrationEventEnvelope.MapFromIntegrationEvent(integrationEvent);
        var exception = new Exception("Mediator failed");

        _mediatorMock
            .Setup(x => x.Publish(
                It.IsAny<INotification>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        Func<Task> action = () => _handler.Handle(envelope, CancellationToken.None);

        // Assert
        var result = await action.Should().ThrowAsync<InvalidOperationException>();
        result.Which.InnerException.Should().Be(exception);
    }

    [Fact]
    public async Task Should_WrapInboxException()
    {
        // Arrange
        var integrationEvent = new FakeIntegrationEvent("Test Event", Guid.NewGuid());
        var envelope = IntegrationEventEnvelope.MapFromIntegrationEvent(integrationEvent);
        var exception = new Exception("Inbox failed");

        _mediatorMock.Setup(x => x.Publish(
                                    It.IsAny<INotification>(),
                                    It.IsAny<CancellationToken>()))
                                .ThrowsAsync(exception);

        // Act
        Func<Task> action = () => _handler.Handle(envelope, CancellationToken.None);

        // Assert
        var result = await action.Should().ThrowAsync<InvalidOperationException>();
        result.Which.InnerException.Should().Be(exception);
    }
}