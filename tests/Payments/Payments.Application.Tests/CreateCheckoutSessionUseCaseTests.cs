using BuildingBlocks.Core;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
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
    private readonly FakeTimeProvider _timeProvider;
    private readonly CreateCheckoutSessionCommandHandler _handler;

    public CreateCheckoutSessionUseCaseTests()
    {
        _paymentGatewayMock = new Mock<IPaymentGateway>();
        _paymentSetupRepositoryMock = new Mock<IPaymentSetupRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork<PaymentSetup>>();
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero));

        _handler = new CreateCheckoutSessionCommandHandler(
            _paymentGatewayMock.Object,
            _paymentSetupRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _timeProvider);
    }

    [Fact]
    public async Task Should_ReturnSuccessWithCheckoutSession_When_CustomerExistsAndHasNoPendingSetup()
    {
        // Arrange
        var (command, customer) = ArrangeExistingCustomer();
        ArrangeNoPendingSetup(customer);
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

        _paymentSetupRepositoryMock.Verify(
            r => r.GetPendingByCustomerIdAsync(It.IsAny<Guid>()), Times.Never);

        _paymentGatewayMock.Verify(
            pg => pg.CreateCheckoutSessionAsync(It.IsAny<CreateCheckoutSessionRequest>()), Times.Never);

        _paymentSetupRepositoryMock.Verify(
            r => r.AddOrUpdateAsync(It.IsAny<PaymentSetup>()), Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_CallGatewayWithExternalCustomerId_When_CustomerExistsAndHasNoPendingSetup()
    {
        // Arrange
        var (command, customer) = ArrangeExistingCustomer();
        ArrangeNoPendingSetup(customer);
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
        var (command, customer) = ArrangeExistingCustomer();
        ArrangeNoPendingSetup(customer);
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
        saved!.SessionId.Should().Be(response.SessionId);
        saved.ExpiresAt.Should().Be(response.ExpiresAt);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_NotPersist_When_GatewayThrowsException()
    {
        // Arrange
        var (command, customer) = ArrangeExistingCustomer();
        ArrangeNoPendingSetup(customer);

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

    [Fact]
    public async Task Should_SearchPendingSetupByCustomerId_When_CustomerExists()
    {
        // Arrange
        var (command, customer) = ArrangeExistingCustomer();
        ArrangeNoPendingSetup(customer);
        ArrangeGatewaySuccess();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentSetupRepositoryMock.Verify(
            r => r.GetPendingByCustomerIdAsync(customer.Id), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnExistingCheckoutSession_When_PendingSetupIsNotExpired()
    {
        // Arrange
        var (command, customer) = ArrangeExistingCustomer();
        var pending = ArrangePendingSetup(customer, expiresAt: _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(10));
        var existing = ArrangeGatewayGetSession(pending.SessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.CheckoutSessionUrl.Should().Be(existing.CheckoutUrl);
    }

    [Fact]
    public async Task Should_RetrieveSessionFromGatewayBySessionId_When_PendingSetupIsNotExpired()
    {
        // Arrange
        var (command, customer) = ArrangeExistingCustomer();
        var pending = ArrangePendingSetup(customer, expiresAt: _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(10));
        ArrangeGatewayGetSession(pending.SessionId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentGatewayMock.Verify(
            pg => pg.GetCheckoutSessionBySessionIdAsync(pending.SessionId), Times.Once);
    }

    [Fact]
    public async Task Should_NotCreateNewSession_When_PendingSetupIsNotExpired()
    {
        // Arrange
        var (command, customer) = ArrangeExistingCustomer();
        var pending = ArrangePendingSetup(customer, expiresAt: _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(10));
        ArrangeGatewayGetSession(pending.SessionId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentGatewayMock.Verify(
            pg => pg.CreateCheckoutSessionAsync(It.IsAny<CreateCheckoutSessionRequest>()), Times.Never);

        _paymentSetupRepositoryMock.Verify(
            r => r.AddOrUpdateAsync(It.IsAny<PaymentSetup>()), Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_CreateNewSession_When_GatewaySessionIsNoLongerOpen()
    {
        // Arrange
        var (command, customer) = ArrangeExistingCustomer();
        var pending = ArrangePendingSetup(customer, expiresAt: _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(10));

        _paymentGatewayMock
            .Setup(pg => pg.GetCheckoutSessionBySessionIdAsync(pending.SessionId))
            .ReturnsAsync((GetCheckoutSessionResponse?)null);

        var response = ArrangeGatewaySuccess();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.CheckoutSessionUrl.Should().Be(response.CheckoutUrl);

        _paymentGatewayMock.Verify(
            pg => pg.CreateCheckoutSessionAsync(It.IsAny<CreateCheckoutSessionRequest>()), Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_CreateNewSessionWithoutCallingGetSession_When_PendingSetupIsExpired()
    {
        // Arrange
        var (command, customer) = ArrangeExistingCustomer();
        ArrangePendingSetup(customer, expiresAt: _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(-1));
        var response = ArrangeGatewaySuccess();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.CheckoutSessionUrl.Should().Be(response.CheckoutUrl);

        _paymentGatewayMock.Verify(
            pg => pg.GetCheckoutSessionBySessionIdAsync(It.IsAny<string>()), Times.Never);

        _paymentGatewayMock.Verify(
            pg => pg.CreateCheckoutSessionAsync(It.IsAny<CreateCheckoutSessionRequest>()), Times.Once);

        _paymentSetupRepositoryMock.Verify(
            r => r.AddOrUpdateAsync(It.IsAny<PaymentSetup>()), Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_CreateNewSession_When_PendingSetupExpiresExactlyNow()
    {
        // Arrange
        var (command, customer) = ArrangeExistingCustomer();
        ArrangePendingSetup(customer, expiresAt: _timeProvider.GetUtcNow().UtcDateTime);
        ArrangeGatewaySuccess();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentGatewayMock.Verify(
            pg => pg.CreateCheckoutSessionAsync(It.IsAny<CreateCheckoutSessionRequest>()), Times.Once);
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

    private void ArrangeNoPendingSetup(Customer customer)
    {
        _paymentSetupRepositoryMock
            .Setup(r => r.GetPendingByCustomerIdAsync(customer.Id))
            .ReturnsAsync((PaymentSetup?)null);
    }

    private PaymentSetup ArrangePendingSetup(Customer customer, DateTime expiresAt)
    {
        var pending = new PaymentSetup("pending-session-id", expiresAt, customer.Id);

        _paymentSetupRepositoryMock
            .Setup(r => r.GetPendingByCustomerIdAsync(customer.Id))
            .ReturnsAsync(pending);

        return pending;
    }

    private GetCheckoutSessionResponse ArrangeGatewayGetSession(string sessionId)
    {
        var response = new GetCheckoutSessionResponse("https://checkout.example.com/pending-session", "client-secret");

        _paymentGatewayMock
            .Setup(pg => pg.GetCheckoutSessionBySessionIdAsync(sessionId))
            .ReturnsAsync(response);

        return response;
    }

    private CreateCheckoutSessionResponse ArrangeGatewaySuccess()
    {
        var response = new CreateCheckoutSessionResponse(
            "https://checkout.example.com/session",
            "session-id",
            Guid.CreateVersion7().ToString(),
            _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(30));

        _paymentGatewayMock
            .Setup(pg => pg.CreateCheckoutSessionAsync(It.IsAny<CreateCheckoutSessionRequest>()))
            .ReturnsAsync(response);

        return response;
    }
}