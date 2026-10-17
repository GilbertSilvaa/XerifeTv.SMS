using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.IntegrationEvents.Subscribers;
using FluentAssertions;
using Moq;
using Payments.Application.Abstractions;
using Payments.Application.IntegrationEventHandlers;
using Payments.Domain.Customer;

namespace Payments.Application.Tests;

public class CreateCustomerOnSubscriberCreatedHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IInboxRepository<Customer>> _inboxRepositoryMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly Mock<IUnitOfWork<Customer>> _unitOfWorkMock;
    private readonly CreateCustomerOnSubscriberCreatedHandler _handler;

    public CreateCustomerOnSubscriberCreatedHandlerTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _inboxRepositoryMock = new Mock<IInboxRepository<Customer>>();
        _paymentGatewayMock = new Mock<IPaymentGateway>();
        _unitOfWorkMock = new Mock<IUnitOfWork<Customer>>();

        _handler = new CreateCustomerOnSubscriberCreatedHandler(
            _inboxRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _customerRepositoryMock.Object,
            _paymentGatewayMock.Object
        );
    }

    [Fact]
    public async Task Should_CreateCustomer_When_SubscriberCreatedEventReceived()
    {
        // Arrange
        var @event = new SubscriberCreatedIntegrationEvent(Guid.NewGuid(), "test@example.com", "TestUser", Guid.NewGuid());
        var gatewayCustomerId = Guid.CreateVersion7().ToString();

        _paymentGatewayMock
            .Setup(pg => pg.CreateCustomerAsync(It.IsAny<CreateCustomerRequest>()))
            .ReturnsAsync(new CreateCustomerResponse(gatewayCustomerId));

        // Act
        await _handler.Execute(@event, CancellationToken.None);

        // Assert
        _paymentGatewayMock.Verify(pg => pg.CreateCustomerAsync(It.Is<CreateCustomerRequest>(req =>
            req.Email == @event.Email &&
            req.Name == @event.UserName)), Times.Once);

        _customerRepositoryMock.Verify(r => r.AddOrUpdateAsync(It.Is<Customer>(c =>
            c.SubscriberId == @event.Id &&
            c.ExternalId == gatewayCustomerId)), Times.Once);
    }

    [Fact]
    public async Task Should_NotPersistCustomer_When_PaymentGatewayFails()
    {
        // Arrange
        var @event = new SubscriberCreatedIntegrationEvent(Guid.NewGuid(), "test@example.com", "TestUser", Guid.NewGuid());

        _paymentGatewayMock
            .Setup(pg => pg.CreateCustomerAsync(It.IsAny<CreateCustomerRequest>()))
            .ThrowsAsync(new InvalidOperationException("gateway down"));

        // Act
        var act = () => _handler.Execute(@event, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _customerRepositoryMock.Verify(r => r.AddOrUpdateAsync(It.IsAny<Customer>()), Times.Never);
    }
}
