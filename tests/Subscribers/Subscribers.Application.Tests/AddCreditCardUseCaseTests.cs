using BuildingBlocks.Core;
using FluentAssertions;
using Moq;
using SharedKernel;
using Subscribers.Application.Abstractions;
using Subscribers.Application.Commands.AddCreditCard;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Enums;
using Subscribers.Domain.Repositories;
using Xunit;

namespace Subscribers.Application.Tests;

public class AddCreditCardUseCaseTests
{
    private readonly Mock<ISubscribersRepository> _subscriberRepositoryMock;
    private readonly Mock<IUnitOfWork<Subscriber>> _unitOfWorkMock;
    private readonly Mock<ICreditCardGateway> _creditCardGatewayMock;
    private readonly AddCreditCardCommandHandler _handler;

    public AddCreditCardUseCaseTests()
    {
        _subscriberRepositoryMock = new Mock<ISubscribersRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork<Subscriber>>();
        _creditCardGatewayMock = new Mock<ICreditCardGateway>();

        _handler = new AddCreditCardCommandHandler(
            _subscriberRepositoryMock.Object,
            _creditCardGatewayMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Should_ReturnSucess_When_AddingCreditCardValidatedByThePaymentGateway()
    {
        // Arrange
        Guid identityUserId = Guid.NewGuid();
        var command = new AddCreditCardCommand(
            identityUserId,
            CardholderName: "John Doe",
            CardNumber: "4111111111111111",
            ExpirationMonth: 12,
            ExpirationYear: DateTime.UtcNow.Year + 2,
            Cvv: "123",
            HolderDocument: "999.999.999.-99",
            BillingAddress: CreateValidAddress()
        );

        var subscriberMock = Subscriber.Create("subscriber_test", "email@example.com", identityUserId);

        _subscriberRepositoryMock
            .Setup(r => r.GetByIdentityUserIdAsync(identityUserId))
            .ReturnsAsync(subscriberMock);

        var creditCardResult = new CreditCardValidationResponse(
            GatewayToken: Guid.CreateVersion7().ToString(),
            Last4Digits: "1111",
            Brand: ECardBrand.ELO,
            ExpiryMonth: 12,
            ExpiryYear: 2028,
            GatewayCustomerId: Guid.NewGuid().ToString());

        _creditCardGatewayMock
            .Setup(s => s.ValidateAsync(
                It.IsAny<CreditCardValidationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CreditCardValidationResponse>.Success(creditCardResult));

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        subscriberMock.GatewayCustomerId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_ReturnFailure_When_AddingCreditCardNotValidatedByTheCreditCardGateway()
    {
        // Arrange
        Guid identityUserId = Guid.NewGuid();
        var command = new AddCreditCardCommand(
            identityUserId,
            CardholderName: "John Doe",
            CardNumber: "4111111111111111",
            ExpirationMonth: 12,
            ExpirationYear: DateTime.UtcNow.Year + 2,
            Cvv: "123",
            HolderDocument: "999.999.999.-99",
            BillingAddress: CreateValidAddress()
        );

        var subscriberMock = Subscriber.Create("subscriber_test", "email@example.com", identityUserId);

        _subscriberRepositoryMock
            .Setup(r => r.GetByIdentityUserIdAsync(identityUserId))
            .ReturnsAsync(subscriberMock);

        _creditCardGatewayMock
            .Setup(s => s.ValidateAsync(
                It.IsAny<CreditCardValidationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CreditCardValidationResponse>.Failure(new Error("CreditCardGateway.ValidationFailed")));

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CreditCardGateway.ValidationFailed");
    }

    private static Address CreateValidAddress() =>
        Address.Create(
            street: "Av. Paulista",
            number: "1000",
            neighborhood: "Bela Vista",
            zipCode: "01310100",
            city: "São Paulo",
            state: "SP",
            country: "BR");
}
