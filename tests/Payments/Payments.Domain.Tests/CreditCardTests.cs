using FluentAssertions;
using Payments.Domain.Customer;
using Payments.Domain.Customer.Enums;
using SharedKernel.Exceptions;

namespace Payments.Domain.Tests;

public class CreditCardTests
{
    // ---------- Helper ----------

    private static CreditCard CreateCreditCard(
        ECardBrand brand = ECardBrand.VISA,
        string last4Digits = "1111",
        int? expiryMonth = null,
        int? expiryYear = null,
        string gatewayToken = "gateway_token",
        Guid? customerId = null) =>
        new(brand,
            last4Digits,
            expiryMonth ?? 12,
            expiryYear ?? DateTime.UtcNow.Year + 1,
            gatewayToken,
            customerId ?? Guid.NewGuid());

    private static ECardBrand AnotherBrand(ECardBrand than) =>
        Enum.GetValues<ECardBrand>().First(b => b != than);

    // ---------- Constructor: happy path ----------

    [Fact]
    public void Should_CreateCreditCard_When_DataIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var expiryYear = DateTime.UtcNow.Year + 1;

        // Act
        var creditCard = new CreditCard(ECardBrand.VISA, "1111", 12, expiryYear, "gateway_token", customerId);

        // Assert
        creditCard.Brand.Should().Be(ECardBrand.VISA);
        creditCard.Last4Digits.Should().Be("1111");
        creditCard.ExpiryMonth.Should().Be(12);
        creditCard.ExpiryYear.Should().Be(expiryYear);
        creditCard.GatewayToken.Should().Be("gateway_token");
        creditCard.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void Should_CreateCreditCard_When_CardExpiresInCurrentMonth()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var act = () => CreateCreditCard(expiryMonth: now.Month, expiryYear: now.Year);

        act.Should().NotThrow();
    }

    // ---------- Constructor: Last4Digits ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345")]
    public void Should_ThrowArgumentException_When_Last4DigitsIsInvalid(string? last4Digits)
    {
        // Arrange & Act
        var act = () => CreateCreditCard(last4Digits: last4Digits!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Last 4 digits are required and must be 4 characters long.*")
            .And.ParamName.Should().Be("Last4Digits");
    }

    // ---------- Constructor: ExpiryMonth ----------

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(13)]
    public void Should_ThrowArgumentException_When_ExpiryMonthIsOutOfRange(int expiryMonth)
    {
        // Arrange & Act
        var act = () => CreateCreditCard(expiryMonth: expiryMonth);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Expiry month must be between 1 and 12.*")
            .And.ParamName.Should().Be("ExpiryMonth");
    }

    // ---------- Constructor: GatewayToken ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_GatewayTokenIsNullOrWhiteSpace(string? gatewayToken)
    {
        // Arrange & Act
        var act = () => CreateCreditCard(gatewayToken: gatewayToken!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Gateway token is required.*")
            .And.ParamName.Should().Be("GatewayToken");
    }

    // ---------- Constructor: expiration ----------

    [Fact]
    public void Should_ThrowValidationException_When_CardExpiredInPreviousYear()
    {
        // Arrange & Act
        var act = () => CreateCreditCard(expiryYear: DateTime.UtcNow.Year - 1);

        // Assert
        act.Should().Throw<ValidationException>().WithMessage("Credit card is expired.");
    }

    [Fact]
    public void Should_ThrowValidationException_When_CardExpiredInPreviousMonth()
    {
        // Arrange
        var previousMonth = DateTime.UtcNow.AddMonths(-1);

        // Act
        var act = () => CreateCreditCard(expiryMonth: previousMonth.Month, expiryYear: previousMonth.Year);

        // Assert
        act.Should().Throw<ValidationException>().WithMessage("Credit card is expired.");
    }

    // ---------- Constructor: CustomerId ----------

    [Fact]
    public void Should_ThrowArgumentException_When_CustomerIdIsEmpty()
    {
        // Arrange & Act
        var act = () => CreateCreditCard(customerId: Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Customer ID is required.*")
            .And.ParamName.Should().Be("CustomerId");
    }

    // ---------- Equals ----------

    [Fact]
    public void Should_ReturnTrue_When_ComparingCardsWithSameBrandLast4DigitsAndExpiry()
    {
        // Arrange
        var first = CreateCreditCard();
        var second = CreateCreditCard();

        // Act & Assert
        first.Equals(second).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnTrue_When_ComparingCardsThatDifferOnlyByGatewayTokenAndCustomerId()
    {
        // Arrange
        var first = CreateCreditCard(gatewayToken: "token_1", customerId: Guid.NewGuid());
        var second = CreateCreditCard(gatewayToken: "token_2", customerId: Guid.NewGuid());

        // Act & Assert
        first.Equals(second).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_ComparingWithNull()
    {
        // Arrange
        var creditCard = CreateCreditCard();

        // Act & Assert
        creditCard.Equals(null!).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_BrandIsDifferent()
    {
        // Arrange
        var first = CreateCreditCard(brand: ECardBrand.VISA);
        var second = CreateCreditCard(brand: AnotherBrand(ECardBrand.VISA));

        // Act & Assert
        first.Equals(second).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_Last4DigitsAreDifferent()
    {
        // Arrange
        var first = CreateCreditCard(last4Digits: "1111");
        var second = CreateCreditCard(last4Digits: "2222");

        // Act & Assert
        first.Equals(second).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_ExpiryMonthIsDifferent()
    {
        // Arrange
        var first = CreateCreditCard(expiryMonth: 11);
        var second = CreateCreditCard(expiryMonth: 12);

        // Act & Assert
        first.Equals(second).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_ExpiryYearIsDifferent()
    {
        // Arrange
        var first = CreateCreditCard(expiryYear: DateTime.UtcNow.Year + 1);
        var second = CreateCreditCard(expiryYear: DateTime.UtcNow.Year + 2);

        // Act & Assert
        first.Equals(second).Should().BeFalse();
    }
}