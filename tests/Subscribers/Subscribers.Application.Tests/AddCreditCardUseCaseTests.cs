using BuildingBlocks.Core;
using FluentAssertions;
using Moq;
using SharedKernel;
using Subscribers.Application.Abstractions;
using Subscribers.Application.Commands.AddCreditCard;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;
using Xunit;

namespace Subscribers.Application.Tests;

public class AddCreditCardCommandHandlerTests
{
    private readonly Mock<ISubscribersRepository> _subscriberRepositoryMock;
    private readonly Mock<IUnitOfWork<Subscriber>> _unitOfWorkMock;
    private readonly Mock<ICreditCardGateway> _creditCardGatewayMock;
    private readonly AddCreditCardCommandHandler _handler;

    public AddCreditCardCommandHandlerTests()
    {
        _subscriberRepositoryMock = new Mock<ISubscribersRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork<Subscriber>>();
        _creditCardGatewayMock = new Mock<ICreditCardGateway>();

        _handler = new AddCreditCardCommandHandler(
            _subscriberRepositoryMock.Object,
            _creditCardGatewayMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Should_ReturnCheckoutUrl_When_GatewayInitiatesCardSetupSuccessfully()
    {
        // Arrange
        var identityUserId = Guid.NewGuid();
        var command = new AddCreditCardCommand(identityUserId);

        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", identityUserId);

        _subscriberRepositoryMock
            .Setup(r => r.GetByIdentityUserIdAsync(identityUserId))
            .ReturnsAsync(subscriber);

        var cardSetupResponse = new InitiateCardSetupResponse(
            CheckoutUrl: "https://teste.io",
            SessionId: Guid.NewGuid().ToString(),
            GatewayCustomerId: Guid.NewGuid().ToString());

        _creditCardGatewayMock
            .Setup(g => g.InitiateCardSetupAsync(
                It.IsAny<InitiateCardSetupRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InitiateCardSetupResponse>.Success(cardSetupResponse));

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Data?.Url.Should().Be(cardSetupResponse.CheckoutUrl);
        subscriber.GatewayCustomerId.Should().Be(cardSetupResponse.GatewayCustomerId);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_GatewayFailsToInitiateCardSetup()
    {
        // Arrange
        var identityUserId = Guid.NewGuid();
        var command = new AddCreditCardCommand(identityUserId);

        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", identityUserId);

        _subscriberRepositoryMock
            .Setup(r => r.GetByIdentityUserIdAsync(identityUserId))
            .ReturnsAsync(subscriber);

        var gatewayError = new Error("CreditCardGateway.ValidationFailed");

        _creditCardGatewayMock
            .Setup(g => g.InitiateCardSetupAsync(
                It.IsAny<InitiateCardSetupRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InitiateCardSetupResponse>.Failure(gatewayError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(gatewayError.Code);
        subscriber.GatewayCustomerId.Should().BeNullOrEmpty();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}