using BuildingBlocks.Core;
using FluentAssertions;
using Moq;
using Payments.Application.Abstractions;
using Payments.Application.Commands.CreateCheckoutSession;
using Payments.Domain.Customer;
using Payments.Domain.PaymentSetup;

namespace Payments.Application.Tests;

public class CreateCheckoutSessionUseCaseTests
{
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly Mock<IPaymentSetupRepository> _paymentSetupRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IUnitOfWork<PaymentSetup>> _unitOfWorkMock;
    private readonly CreateCheckoutSessionCommandHandler _handler;

    public CreateCheckoutSessionUseCaseTests()
    {
        _paymentGatewayMock = new Mock<IPaymentGateway>();
        _paymentSetupRepositoryMock = new Mock<IPaymentSetupRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork<PaymentSetup>>();

        _handler = new CreateCheckoutSessionCommandHandler(
            _paymentGatewayMock.Object,
            _paymentSetupRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Should_ReturnSuccessWithCheckoutSession_When_CustomerExists()
    {
        // Arrange
        var (command, _) = ArrangeExistingCustomer();
        var response = ArrangeGatewaySuccess();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.CheckoutSessionUrl.Should().Be(response.CheckoutUrl);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_CustomerDoesNotExist()
    {
        // Arrange
        var command = new CreateCheckoutSessionCommand(Guid.NewGuid());

        _customerRepositoryMock
            .Setup(r => r.GetByIdentityIdAsync(command.CustomerIdentityId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("CreateCheckoutSession.CustomerNotFound");

        _paymentGatewayMock.Verify(
            pg => pg.CreateCheckoutSessionAsync(It.IsAny<CreateCheckoutSessionRequest>()), Times.Never);

        _paymentSetupRepositoryMock.Verify(
            r => r.AddOrUpdateAsync(It.IsAny<PaymentSetup>()), Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_CallGatewayWithExternalCustomerId_When_CustomerExists()
    {
        // Arrange
        var (command, customer) = ArrangeExistingCustomer();
        ArrangeGatewaySuccess();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentGatewayMock.Verify(
            pg => pg.CreateCheckoutSessionAsync(
                It.Is<CreateCheckoutSessionRequest>(r =>
                    r.CustomerExternalId == customer.ExternalId)), Times.Once);
    }

    [Fact]
    public async Task Should_PersistPaymentSetup_When_CheckoutSessionIsCreated()
    {
        // Arrange
        var (command, _) = ArrangeExistingCustomer();
        var response = ArrangeGatewaySuccess();

        PaymentSetup? saved = null;
        _paymentSetupRepositoryMock
            .Setup(r => r.AddOrUpdateAsync(It.IsAny<PaymentSetup>()))
            .Callback<PaymentSetup>(p => saved = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        saved.Should().NotBeNull();
        saved.SessionId.Should().Be(response.SessionId);
        saved.ExpiresAt.Should().Be(response.ExpiresAt);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_NotPersist_When_GatewayThrowsException()
    {
        // Arrange
        var (command, _) = ArrangeExistingCustomer();

        _paymentGatewayMock
            .Setup(pg => pg.CreateCheckoutSessionAsync(It.IsAny<CreateCheckoutSessionRequest>()))
            .ThrowsAsync(new InvalidOperationException("gateway down"));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _paymentSetupRepositoryMock.Verify(
            r => r.AddOrUpdateAsync(It.IsAny<PaymentSetup>()), Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- Helpers ----------

    private (CreateCheckoutSessionCommand command, Customer customer) ArrangeExistingCustomer()
    {
        var identityId = Guid.NewGuid();
        var customer = new Customer(Guid.NewGuid(), identityId, "external-customer-id");

        _customerRepositoryMock
            .Setup(r => r.GetByIdentityIdAsync(identityId))
            .ReturnsAsync(customer);

        return (new CreateCheckoutSessionCommand(identityId), customer);
    }

    private CreateCheckoutSessionResponse ArrangeGatewaySuccess()
    {
        var response = new CreateCheckoutSessionResponse(
            "https://checkout.example.com/session",
            "session-id",
            Guid.CreateVersion7().ToString(),
            DateTime.UtcNow.AddMinutes(30));

        _paymentGatewayMock
            .Setup(pg => pg.CreateCheckoutSessionAsync(It.IsAny<CreateCheckoutSessionRequest>()))
            .ReturnsAsync(response);

        return response;
    }
}