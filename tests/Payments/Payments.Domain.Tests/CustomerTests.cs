using FluentAssertions;
using Payments.Domain.Customer;
using Payments.Domain.Customer.Enums;
using Payments.Domain.Customer.Events;
using Payments.Domain.Customer.Exceptions;
using SharedKernel.Exceptions;

namespace Payments.Domain.Tests;

public class CustomerTests
{
    private static Customer.Customer CreateCustomer() =>
        new(Guid.NewGuid(), "external_id_test");

    private static CreditCard CreateCreditCard(
        string last4Digits = "1111",
        string gatewayToken = "gateway_token") =>
        new(ECardBrand.VISA,
            last4Digits: last4Digits,
            expiryMonth: 12,
            expiryYear: DateTime.UtcNow.Year + 1,
            gatewayToken: gatewayToken,
            customerId: Guid.NewGuid());

    [Fact]
    public void Should_CreateCustomer_When_DataIsValid()
    {
        // Arrange
        var subscriberId = Guid.NewGuid();

        // Act
        var customer = new Customer.Customer(subscriberId, "external_id_test");

        // Assert
        customer.SubscriberId.Should().Be(subscriberId);
        customer.ExternalId.Should().Be("external_id_test");
        customer.CreditCards.Should().BeEmpty();
        customer.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Should_ThrowArgumentException_When_SubscriberIdIsEmpty()
    {
        // Arrange & Act
        var act = () => new Customer.Customer(Guid.Empty, "external_id_test");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Subscriber ID cannot be empty.*")
            .And.ParamName.Should().Be("subscriberId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_ThrowArgumentException_When_ExternalIdIsNullOrEmpty(string? externalId)
    {
        // Arrange & Act
        var act = () => new Customer.Customer(Guid.NewGuid(), externalId!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("External ID cannot be null or empty.*")
            .And.ParamName.Should().Be("externalId");
    }

    // ---------- AddCreditCard ----------

    [Fact]
    public void Should_AddCreditCard_When_CreditCardIsValid()
    {
        // Arrange
        var customer = CreateCustomer();
        var creditCard = CreateCreditCard();

        // Act
        customer.AddCreditCard(creditCard);

        // Assert
        customer.CreditCards.Should().ContainSingle(cc => cc.Id == creditCard.Id);
        customer.DomainEvents.Should().ContainSingle(de => de is CreditCardAddedDomainEvent);
    }

    [Fact]
    public void Should_RaiseCreditCardAddedDomainEventWithCorrectData_When_AddingCreditCard()
    {
        // Arrange
        var customer = CreateCustomer();
        var creditCard = CreateCreditCard();

        // Act
        customer.AddCreditCard(creditCard);

        // Assert
        var domainEvent = customer.DomainEvents.OfType<CreditCardAddedDomainEvent>().Single();
        domainEvent.CreditCardId.Should().Be(creditCard.Id);
        domainEvent.CardLast4Digits.Should().Be(creditCard.Last4Digits);
        domainEvent.CardBrand.Should().Be(creditCard.Brand);
        domainEvent.SubscriberId.Should().Be(customer.SubscriberId);
    }

    [Fact]
    public void Should_AddAllCreditCards_When_AddingDistinctCreditCards()
    {
        // Arrange
        var customer = CreateCustomer();
        var first = CreateCreditCard(last4Digits: "1111", gatewayToken: "token_1");
        var second = CreateCreditCard(last4Digits: "2222", gatewayToken: "token_2");

        // Act
        customer.AddCreditCard(first);
        customer.AddCreditCard(second);

        // Assert
        customer.CreditCards.Should().HaveCount(2);
        customer.DomainEvents.OfType<CreditCardAddedDomainEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Should_ThrowValidationException_When_AddingNullCreditCard()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act
        var act = () => customer.AddCreditCard(null!);

        // Assert
        act.Should().Throw<ValidationException>().WithMessage("The credit card provided is invalid.");
        customer.CreditCards.Should().BeEmpty();
        customer.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Should_ThrowCreditCardAlreadyExistsException_When_AddingDuplicateCreditCard()
    {
        // Arrange
        var customer = CreateCustomer();
        var creditCard = CreateCreditCard();
        var duplicate = CreateCreditCard();

        customer.AddCreditCard(creditCard);

        // Act
        var act = () => customer.AddCreditCard(duplicate);

        // Assert
        act.Should().Throw<CreditCardAlreadyExistsException>().WithMessage("Credit card already exists.");
        customer.CreditCards.Should().HaveCount(1);
        customer.DomainEvents.OfType<CreditCardAddedDomainEvent>().Should().HaveCount(1);
    }

    // ---------- RemoveCreditCard ----------

    [Fact]
    public void Should_DeleteCreditCardAndRaiseEvent_When_RemovingExistingCreditCard()
    {
        // Arrange
        var customer = CreateCustomer();
        var creditCard = CreateCreditCard();
        customer.AddCreditCard(creditCard);

        // Act
        customer.RemoveCreditCard(creditCard);

        // Assert
        creditCard.IsDeleted.Should().BeTrue();
        customer.DomainEvents.Should().ContainSingle(de => de is CreditCardRemovedDomainEvent);
    }

    [Fact]
    public void Should_RaiseCreditCardRemovedDomainEventWithCorrectData_When_RemovingCreditCard()
    {
        // Arrange
        var customer = CreateCustomer();
        var creditCard = CreateCreditCard();
        customer.AddCreditCard(creditCard);

        // Act
        customer.RemoveCreditCard(creditCard);

        // Assert
        var domainEvent = customer.DomainEvents.OfType<CreditCardRemovedDomainEvent>().Single();
        domainEvent.CreditCardId.Should().Be(creditCard.Id);
        domainEvent.SubscriberId.Should().Be(customer.SubscriberId);
    }

    [Fact]
    public void Should_ThrowValidationException_When_RemovingNullCreditCard()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act
        var act = () => customer.RemoveCreditCard(null!);

        // Assert
        act.Should().Throw<ValidationException>().WithMessage("The credit card provided is invalid.");
        customer.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Should_DoNothing_When_RemovingCreditCardNotInCustomer()
    {
        // Arrange
        var customer = CreateCustomer();
        var existing = CreateCreditCard(last4Digits: "1111", gatewayToken: "token_1");
        var unknown = CreateCreditCard(last4Digits: "2222", gatewayToken: "token_2");
        customer.AddCreditCard(existing);

        // Act
        var act = () => customer.RemoveCreditCard(unknown);

        // Assert
        act.Should().NotThrow();
        existing.IsDeleted.Should().BeFalse();
        unknown.IsDeleted.Should().BeFalse();
        customer.DomainEvents.OfType<CreditCardRemovedDomainEvent>().Should().BeEmpty();
    }
}