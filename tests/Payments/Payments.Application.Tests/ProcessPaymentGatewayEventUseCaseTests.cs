using BuildingBlocks.Core;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using FluentAssertions;
using Moq;
using Payments.Application.Abstractions;
using Payments.Application.Commands.ProcessPaymentGatewayEvent;
using Payments.Domain.PaymentSetup;
using SharedKernel;
using System.Runtime.InteropServices;

namespace Payments.Application.Tests;

public class ProcessPaymentGatewayEventUseCaseTests
{
    private const string EventId = "evt_test_123";
    private const string EventType = "checkout.session.completed";
    private const string DataJson = "{\"id\":\"cs_test_123\"}";

    private readonly Mock<IIntegrationEventPublisher<PaymentSetup>> _publisherMock = new();
    private readonly Mock<IUnitOfWork<PaymentSetup>> _unitOfWorkMock = new();

    [Fact]
    public async Task Should_ReturnSuccess_When_TranslatorCanTranslateEvent()
    {
        // Arrange
        var integrationEvent = new TestIntegrationEvent();
        var translator = ArrangeTranslator(EventType, integrationEvent);
        var handler = CreateHandler(translator.Object);

        // Act
        var result = await handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Should_TranslateUsingDataJson_When_TranslatorMatchesEventType()
    {
        // Arrange
        var translator = ArrangeTranslator(EventType, new TestIntegrationEvent());
        var handler = CreateHandler(translator.Object);

        // Act
        await handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        translator.Verify(t => t.CanTranslate(EventType), Times.AtLeastOnce);
        translator.Verify(t => t.Translate(DataJson), Times.Once);
    }

    [Fact]
    public async Task Should_PublishTranslatedEventWithItsEventName_When_TranslationSucceeds()
    {
        // Arrange
        var integrationEvent = new TestIntegrationEvent();
        var translator = ArrangeTranslator(EventType, integrationEvent);
        var handler = CreateHandler(translator.Object);

        // Act
        await handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        _publisherMock.Verify(
            p => p.PublishAsync<IntegrationEvent>(
                integrationEvent,
                integrationEvent.EventName,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_SaveChangesOnce_When_EventIsPublished()
    {
        // Arrange
        var translator = ArrangeTranslator(EventType, new TestIntegrationEvent());
        var handler = CreateHandler(translator.Object);

        // Act
        await handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_PublishBeforeSaveChanges_When_ProcessingEvent()
    {
        // Arrange
        var calls = new List<string>();

        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<IntegrationEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("publish"))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("save"))
            .Returns(Task.CompletedTask);

        var translator = ArrangeTranslator(EventType, new TestIntegrationEvent());
        var handler = CreateHandler(translator.Object);

        // Act
        await handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        calls.Should().Equal("publish", "save");
    }

    [Fact]
    public async Task Should_PropagateCancellationToken_When_PublishingAndSaving()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var translator = ArrangeTranslator(EventType, new TestIntegrationEvent());
        var handler = CreateHandler(translator.Object);

        // Act
        await handler.Handle(NewCommand(), cts.Token);

        // Assert
        _publisherMock.Verify(
            p => p.PublishAsync(It.IsAny<IntegrationEvent>(), It.IsAny<string>(), cts.Token), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task Should_UseOnlyMatchingTranslator_When_MultipleTranslatorsAreRegistered()
    {
        // Arrange
        var other = ArrangeNonMatchingTranslator();
        var matching = ArrangeTranslator(EventType, new TestIntegrationEvent());
        var handler = CreateHandler(other.Object, matching.Object);

        // Act
        var result = await handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        other.Verify(t => t.Translate(It.IsAny<string>()), Times.Never);
        matching.Verify(t => t.Translate(DataJson), Times.Once);
    }

    [Fact]
    public async Task Should_UseFirstTranslator_When_MoreThanOneCanTranslate()
    {
        // Arrange
        var firstEvent = new TestIntegrationEvent();
        var secondEvent = new TestIntegrationEvent();
        var first = ArrangeTranslator(EventType, firstEvent);
        var second = ArrangeTranslator(EventType, secondEvent);
        var handler = CreateHandler(first.Object, second.Object);

        // Act
        await handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        first.Verify(t => t.Translate(DataJson), Times.Once);
        second.Verify(t => t.Translate(It.IsAny<string>()), Times.Never);

        _publisherMock.Verify(
            p => p.PublishAsync<IntegrationEvent>(firstEvent, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        _publisherMock.Verify(
            p => p.PublishAsync<IntegrationEvent>(secondEvent, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("checkout.session.completed")]
    [InlineData("checkout.session.expired")]
    [InlineData("setup_intent.succeeded")]
    public async Task Should_AskTranslatorsWithTheCommandEventType_When_Handling(string eventType)
    {
        // Arrange
        var translator = ArrangeTranslator(eventType, new TestIntegrationEvent());
        var handler = CreateHandler(translator.Object);

        // Act
        var result = await handler.Handle(NewCommand(eventType), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        translator.Verify(t => t.CanTranslate(eventType), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_NoTranslatorsAreRegistered()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        AssertTranslationFailure(result);
        AssertNothingPublishedNorSaved();
    }

    [Fact]
    public async Task Should_ReturnFailure_When_NoTranslatorCanTranslateEventType()
    {
        // Arrange
        var other = ArrangeNonMatchingTranslator();
        var handler = CreateHandler(other.Object);

        // Act
        var result = await handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        AssertTranslationFailure(result);
        other.Verify(t => t.Translate(It.IsAny<string>()), Times.Never);
        AssertNothingPublishedNorSaved();
    }

    [Fact]
    public async Task Should_ReturnFailure_When_TranslatorReturnsNull()
    {
        // Arrange
        var translator = ArrangeTranslator(EventType, integrationEvent: null);
        var handler = CreateHandler(translator.Object);

        // Act
        var result = await handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        AssertTranslationFailure(result);
        AssertNothingPublishedNorSaved();
    }

    [Fact]
    public async Task Should_IncludeEventTypeInErrorMessage_When_TranslationFails()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(NewCommand("unknown.event"), CancellationToken.None);

        // Assert
        result.Error.Description.Should().Contain("unknown.event");
    }

    [Fact]
    public async Task Should_PropagateExceptionAndNotPublishOrSave_When_TranslatorThrows()
    {
        // Arrange
        var translator = new Mock<IPaymentGatewayEventToIntegrationEventTranslator>();
        translator.Setup(t => t.CanTranslate(EventType)).Returns(true);
        translator.Setup(t => t.Translate(It.IsAny<string>())).Throws(new FormatException("invalid json"));
        var handler = CreateHandler(translator.Object);

        // Act
        var act = () => handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FormatException>();
        AssertNothingPublishedNorSaved();
    }

    [Fact]
    public async Task Should_NotSaveChanges_When_PublisherThrows()
    {
        // Arrange
        var translator = ArrangeTranslator(EventType, new TestIntegrationEvent());
        var handler = CreateHandler(translator.Object);

        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<IntegrationEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("broker down"));

        // Act
        var act = () => handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_PropagateException_When_SaveChangesThrows()
    {
        // Arrange
        var translator = ArrangeTranslator(EventType, new TestIntegrationEvent());
        var handler = CreateHandler(translator.Object);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        // Act
        var act = () => handler.Handle(NewCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ---------- Helpers ----------

    private ProcessPaymentGatewayEventCommandHandler CreateHandler(
        params IPaymentGatewayEventToIntegrationEventTranslator[] translators) =>
        new(_publisherMock.Object, translators, _unitOfWorkMock.Object);

    private static ProcessPaymentGatewayEventCommand NewCommand(string eventType = EventType) =>
    new(EventId: EventId, EventType: eventType, DataJson: DataJson);

    private static Mock<IPaymentGatewayEventToIntegrationEventTranslator> ArrangeTranslator(
        string eventType, IntegrationEvent? integrationEvent)
    {
        var mock = new Mock<IPaymentGatewayEventToIntegrationEventTranslator>();
        mock.Setup(t => t.CanTranslate(eventType)).Returns(true);
        mock.Setup(t => t.Translate(It.IsAny<string>())).Returns(integrationEvent);
        return mock;
    }

    private static Mock<IPaymentGatewayEventToIntegrationEventTranslator> ArrangeNonMatchingTranslator()
    {
        var mock = new Mock<IPaymentGatewayEventToIntegrationEventTranslator>();
        mock.Setup(t => t.CanTranslate(It.IsAny<string>())).Returns(false);
        return mock;
    }

    private static void AssertTranslationFailure(Result result)
    {
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("ProcessPaymentGatewayEvent.NotPossibleToTranslateIntegrationEvent");
    }

    private void AssertNothingPublishedNorSaved()
    {
        _publisherMock.Verify(
            p => p.PublishAsync(It.IsAny<IntegrationEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed record TestIntegrationEvent() : IntegrationEvent("payments.test-event");
}