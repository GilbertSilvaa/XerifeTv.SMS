using FluentAssertions;
using SharedKernel;
using SharedKernel.Exceptions;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Enums;
using Subscribers.Domain.Events;
using Subscribers.Domain.Exceptions;
using Subscribers.Domain.ValueObjects;
using Xunit;

namespace Subscribers.Domain.Tests;

public class PaymentMethodTests
{
    [Fact]
    public void Should_CreateCreditCard_When_CreatingCreditCardWithDataIsValid()
    {
        // Arrange
        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", Guid.NewGuid());

        var creditCard = new CreditCard(
            ECardBrand.VISA,
            last4Digits: "1111",
            expiryMonth: 12,
            expiryYear: DateTime.UtcNow.Year + 1,
            gatewayToken: "gateway_token");

        // Act
        subscriber.AddCreditCard(creditCard);

        // Assert
        subscriber.CreditCards.Should().ContainSingle(cc => cc.GatewayToken == creditCard.GatewayToken);
        subscriber.CreditCards.Should().HaveCount(1);
        subscriber.DomainEvents.Should().ContainSingle(de => de is CreditCardAddedDomainEvent);
    }

    [Fact]
    public void Should_ThrowValidationException_When_CreatingCreditCardIsExpired()
    {
        // Arrange & Act
        var act = () => new CreditCard(
            ECardBrand.VISA,
            last4Digits: "1111",
            expiryMonth: 12,
            expiryYear: DateTime.UtcNow.Year - 1, // Expired card
            gatewayToken: "gateway_token"); ;

        // Assert
        act.Should().Throw<ValidationException>().WithMessage("Credit card is expired.");
    }

    [Fact]
    public void Should_ThrowCreditCardAlreadyExistsException_When_AddingDuplicateCreditCard()
    {
        // Arrange
        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", Guid.NewGuid());

        var creditCard = new CreditCard(
            ECardBrand.VISA,
            last4Digits: "1111",
            expiryMonth: 12,
            expiryYear: DateTime.UtcNow.Year + 1,
            gatewayToken: "gateway_token");

        var duplicateCreditCard = new CreditCard(
            ECardBrand.VISA,
            last4Digits: "1111",
            expiryMonth: 12,
            expiryYear: DateTime.UtcNow.Year + 1,
            gatewayToken: "gateway_token_2");

        // Act
        var act = () =>
        {
            subscriber.AddCreditCard(creditCard);
            subscriber.AddCreditCard(duplicateCreditCard);
        };

        // Assert
        act.Should().Throw<CreditCardAlreadyExistsException>().WithMessage("Credit card already exists.");
        subscriber.CreditCards.Should().HaveCount(1);
    }

    [Fact]
    public void Should_RemoveCreditCard_When_RemovingExistingCreditCardNotLinkedToActiveSignature()
    {
        // Arrange
        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", Guid.NewGuid());

        var creditCard = new CreditCard(
            ECardBrand.VISA,
            last4Digits: "1111",
            expiryMonth: 12,
            expiryYear: DateTime.UtcNow.Year + 1,
            gatewayToken: "gateway_token");

        subscriber.AddCreditCard(creditCard);

        // Act
        subscriber.RemoveCreditCard(creditCard);

        // Assert
        subscriber.CreditCards.Should().HaveCount(0);
        subscriber.DomainEvents.Should().ContainSingle(de => de is CreditCardRemovedDomainEvent);
    }

    [Fact]
    public void Should_ThrowCreditCardLinkedToActiveSignatureException_When_RemovingCreditCardLinkedToActiveSignature()
    {
        // Arrange
        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", Guid.NewGuid());

        var creditCard = new CreditCard(
            ECardBrand.VISA,
            last4Digits: "1111",
            expiryMonth: 12,
            expiryYear: DateTime.UtcNow.Year + 1,
            gatewayToken: "gateway_token");

        subscriber.AddCreditCard(creditCard);

        var plan = new PlanSnapshot(Guid.NewGuid(), "Plan A", 2, Money.From(10.0m, "BRL"));
        var paymentMethod = new PaymentMethod(EPaymentMethodType.CREDIT, creditCardId: creditCard.Id);

        subscriber.AddSignature(plan, paymentMethod);

        // Act
        var act = () => subscriber.RemoveCreditCard(creditCard);

        // Assert
        act.Should()
            .Throw<CreditCardLinkedToActiveSignatureException>()
            .WithMessage("Cannot remove credit card linked to an active signature.");
        subscriber.CreditCards.Should().HaveCount(1);
        subscriber.CreditCards.Should().ContainSingle(cc => cc.GatewayToken == creditCard.GatewayToken);
    }

    [Fact]
    public void Should_CreateSignatureWithCreditCard_When_AddingSignatureWithValidCreditCard()
    {
        // Arrange
        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", Guid.NewGuid());

        var creditCard = new CreditCard(
            ECardBrand.VISA,
            last4Digits: "1111",
            expiryMonth: 12,
            expiryYear: DateTime.UtcNow.Year + 1,
            gatewayToken: "gateway_token");

        subscriber.AddCreditCard(creditCard);

        var plan = new PlanSnapshot(Guid.NewGuid(), "Plan A", 2, Money.From(10.0m, "BRL"));
        var paymentMethod = new PaymentMethod(EPaymentMethodType.CREDIT, creditCardId: creditCard.Id);

        // Act
        subscriber.AddSignature(plan, paymentMethod);

        // Assert
        subscriber.Signatures.Should().HaveCount(1);
        subscriber.Signatures.Should()
            .ContainSingle(sig => sig.PaymentMethod.CreditCardId == creditCard.Id && sig.PaymentMethod.Type == EPaymentMethodType.CREDIT);
    }

    [Fact]
    public void Should_ThrowValidationException_When_CreatingPaymentMethodCreditTypeWithoutCreditCard()
    {
        // Arrange & Act
        var act = () => new PaymentMethod(EPaymentMethodType.CREDIT, creditCardId: null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Credit card ID is required for credit payment method.");
    }

    [Fact]
    public void Should_ThrowValidationException_When_AddingSignatureWithNonExistentCreditCard()
    {
        // Arrange
        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", Guid.NewGuid());

        var plan = new PlanSnapshot(Guid.NewGuid(), "Plan A", 2, Money.From(10.0m, "BRL"));
        var paymentMethod = new PaymentMethod(EPaymentMethodType.CREDIT, creditCardId: Guid.NewGuid());

        // Act
        var act = () => subscriber.AddSignature(plan, paymentMethod);

        // Assert
        act.Should().Throw<ValidationException>().WithMessage("Credit card not found.");
        subscriber.Signatures.Should().HaveCount(0);
    }

    [Fact]
    public void Should_UpdateSignaturePaymentMethod_When_ChangingPaymentMethodToTypePix()
    {
        // Arrange
        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", Guid.NewGuid());

        var creditCard = new CreditCard(
            ECardBrand.VISA,
            last4Digits: "1111",
            expiryMonth: 12,
            expiryYear: DateTime.UtcNow.Year + 1,
            gatewayToken: "gateway_token");

        subscriber.AddCreditCard(creditCard);

        var plan = new PlanSnapshot(Guid.NewGuid(), "Plan A", 2, Money.From(10.0m, "BRL"));
        var paymentMethodCredit = new PaymentMethod(EPaymentMethodType.CREDIT, creditCardId: creditCard.Id);

        subscriber.AddSignature(plan, paymentMethodCredit);

        var paymentMethodPix = new PaymentMethod(EPaymentMethodType.PIX);

        // Act
        subscriber.UpdatePaymentMethodSignature(paymentMethodPix);

        // Assert
        subscriber.ActiveSignature?.PaymentMethod.Type.Should().Be(EPaymentMethodType.PIX);
        subscriber.ActiveSignature?.PaymentMethod.CreditCardId.Should().BeNull();
        subscriber.DomainEvents.Should().ContainSingle(de => de is SignaturePaymentMethodUpdatedDomainEvent);
    }

    [Fact]
    public void Should_UpdateSignaturePaymentMethod_When_ChangingPaymentMethodToTypeBoleto()
    {
        // Arrange
        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", Guid.NewGuid());

        var plan = new PlanSnapshot(Guid.NewGuid(), "Plan A", 2, Money.From(10.0m, "BRL"));

        subscriber.AddSignature(plan, new PaymentMethod(EPaymentMethodType.PIX));

        var paymentMethodBoleto = new PaymentMethod(EPaymentMethodType.BOLETO);

        // Act
        subscriber.UpdatePaymentMethodSignature(paymentMethodBoleto);

        // Assert
        subscriber.ActiveSignature?.PaymentMethod.Type.Should().Be(EPaymentMethodType.BOLETO);
        subscriber.ActiveSignature?.PaymentMethod.CreditCardId.Should().BeNull();
        subscriber.DomainEvents.Should().ContainSingle(de => de is SignaturePaymentMethodUpdatedDomainEvent);
    }

    [Fact]
    public void Should_UpdateSignaturePaymentMethod_When_ChangingPaymentMethodToTypeCreditWithValidCreditCard()
    {
        // Arrange
        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", Guid.NewGuid());

        var plan = new PlanSnapshot(Guid.NewGuid(), "Plan A", 2, Money.From(10.0m, "BRL"));

        subscriber.AddSignature(plan, new PaymentMethod(EPaymentMethodType.PIX));

        var creditCard = new CreditCard(
            ECardBrand.VISA,
            last4Digits: "1111",
            expiryMonth: 12,
            expiryYear: DateTime.UtcNow.Year + 1,
            gatewayToken: "gateway_token");

        subscriber.AddCreditCard(creditCard);

        var paymentMethodCredit = new PaymentMethod(EPaymentMethodType.CREDIT, creditCardId: creditCard.Id);

        // Act
        subscriber.UpdatePaymentMethodSignature(paymentMethodCredit);

        // Assert
        subscriber.ActiveSignature?.PaymentMethod.Type.Should().Be(EPaymentMethodType.CREDIT);
        subscriber.ActiveSignature?.PaymentMethod.CreditCardId.Should().Be(creditCard.Id);
        subscriber.DomainEvents.Should().ContainSingle(de => de is SignaturePaymentMethodUpdatedDomainEvent);
    }

    [Fact]
    public void Should_ThrowValidationException_When_ChangingPaymentMethodToTypeCreditWithNonExistentCreditCard()
    {
        // Arrange
        var subscriber = Subscriber.Create("subscriber_test", "email@example.com", Guid.NewGuid());

        var plan = new PlanSnapshot(Guid.NewGuid(), "Plan A", 2, Money.From(10.0m, "BRL"));

        subscriber.AddSignature(plan, new PaymentMethod(EPaymentMethodType.PIX));

        var paymentMethodCredit = new PaymentMethod(EPaymentMethodType.CREDIT, creditCardId: Guid.NewGuid());

        // Act
        var act = () => subscriber.UpdatePaymentMethodSignature(paymentMethodCredit);

        // Assert
        act.Should().Throw<ValidationException>().WithMessage("Credit card not found.");
        subscriber.ActiveSignature?.PaymentMethod.Type.Should().Be(EPaymentMethodType.PIX);
        subscriber.ActiveSignature?.PaymentMethod.CreditCardId.Should().BeNull();
    }
}