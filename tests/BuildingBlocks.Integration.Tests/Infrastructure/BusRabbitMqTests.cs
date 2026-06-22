using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Infrastructure.Messaging.Buses.RabbitMQ;
using BuildingBlocks.Integration.Tests.Fakes;
using BuildingBlocks.Integration.Tests.Infrastructure.Fixtures;
using BuildingBlocks.IntegrationEvents;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Text.Json;

namespace BuildingBlocks.Integration.Tests.Infrastructure;

[Collection("RabbitMq")]
public sealed class BusRabbitMqTests : IAsyncLifetime
{
    private readonly RabbitMqFixture _fixture;
    private readonly TaskCompletionSource<IntegrationEventEnvelope> _messageReceived;

    private IMessageBus _messageBus = default!;
    private IHost _publisherHost = default!;
    private IHost _consumerHost = default!;

    private readonly Mock<IMediator> _mediatorMock = new();

    public BusRabbitMqTests(RabbitMqFixture fixture)
    {
        _fixture = fixture;
        _messageReceived = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public async Task InitializeAsync()
    {
        var publisherBuilder = Host.CreateApplicationBuilder();
        publisherBuilder.AddWolverineRabbitMQPublisherConfiguration(_fixture.GetConnectionOptions());
        publisherBuilder.Services.AddSingleton<IMessageBus, WolverineRabbitMQMessageBus>();
        _publisherHost = publisherBuilder.Build();

        var consumerBuilder = Host.CreateApplicationBuilder();
        consumerBuilder.AddWolverineRabbitMQConsumerConfiguration(_fixture.GetConnectionOptions(), maxAttempsRetry: 1);

        consumerBuilder.Services
            .AddSingleton(_mediatorMock.Object)
            .AddSingleton(new IntegrationEventTypeMapper(new()
            {
                ["fake.test"] = typeof(FakeIntegrationEvent)
            }));

        _mediatorMock
            .Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((notification, _) =>
            {
                if (notification is IntegrationEvent integrationEvent)
                    _messageReceived.TrySetResult(IntegrationEventEnvelope.MapFromIntegrationEvent(integrationEvent));
            })
            .Returns(Task.CompletedTask);

        _consumerHost = consumerBuilder.Build();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await Task.WhenAll(_publisherHost.StartAsync(cts.Token), _consumerHost.StartAsync(cts.Token));

        _messageBus = _publisherHost.Services.GetRequiredService<IMessageBus>();
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(_publisherHost.StopAsync(), _consumerHost.StopAsync());

        _publisherHost.Dispose();
        _consumerHost.Dispose();
    }

    [Fact]
    public async Task Should_PublishMessage_When_EnvelopeIsValid()
    {
        // Arrange
        var integrationEvent = new FakeIntegrationEvent("Test Event", Guid.NewGuid());
        var envelope = IntegrationEventEnvelope.MapFromIntegrationEvent(integrationEvent);
        var message = JsonSerializer.Serialize(envelope);

        // Act
        var act = async () =>
            await _messageBus.PublishAsync(message, topic: integrationEvent.EventType);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_ThrowException_When_MessageIsInvalidJson()
    {
        // Arrange
        var invalidMessage = "{ invalid json }";

        // Act
        var act = async () =>
            await _messageBus.PublishAsync(invalidMessage, topic: "fake.test");

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Should_DeliverMessage_When_PublishedToExchange()
    {
        // Arrange
        var integrationEvent = new FakeIntegrationEvent("Test Event", Guid.NewGuid());
        var envelope = IntegrationEventEnvelope.MapFromIntegrationEvent(integrationEvent);
        var message = JsonSerializer.Serialize(envelope);

        // Act
        await _messageBus.PublishAsync(message, topic: integrationEvent.EventType);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var received = await _messageReceived.Task.WaitAsync(cts.Token);

        // Assert
        received.EventName.Should().Be(envelope.EventName);
        received.EventType.Should().Be(envelope.EventType);
    }

    [Fact]
    public async Task Should_DeliverMessage_WithCorrectPayload()
    {
        // Arrange
        var integrationEvent = new FakeIntegrationEvent("Payload Check", Guid.NewGuid());
        var envelope = IntegrationEventEnvelope.MapFromIntegrationEvent(integrationEvent);
        var message = JsonSerializer.Serialize(envelope);

        // Act
        await _messageBus.PublishAsync(message, topic: integrationEvent.EventType);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await _messageReceived.Task.WaitAsync(cts.Token);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(
                It.Is<INotification>(e =>
                    e.GetType() == typeof(FakeIntegrationEvent) &&
                    ((FakeIntegrationEvent)e).Name == integrationEvent.Name &&
                    ((FakeIntegrationEvent)e).ExcutionId == integrationEvent.ExcutionId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_SendToDeadLetter_When_HandlerThrowsAfterRetries()
    {
        // Arrange
        _mediatorMock
            .Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Permanent failure"));

        var integrationEvent = new FakeIntegrationEvent("Dead Letter Event", Guid.NewGuid());
        var envelope = IntegrationEventEnvelope.MapFromIntegrationEvent(integrationEvent);
        var message = JsonSerializer.Serialize(envelope);

        // Act
        await _messageBus.PublishAsync(message, topic: integrationEvent.EventType);
        await Task.Delay(TimeSpan.FromSeconds(15));

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}