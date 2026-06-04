using System.Text.Json;
using FluentAssertions;

namespace SharedKernel.Tests;

public class MoneyTests
{
    [Fact]
    public void Should_CreateMoney_When_ValidAmountAndCurrencyAreProvided()
    {
        // Arrange
        var amount = 100.50m;
        var currency = "BRL";

        // Act
        var money = Money.From(amount, currency);

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Should_NormalizeCurrencyToUpperCase_When_LowerCaseCurrencyIsProvided()
    {
        // Arrange
        var amount = 10m;
        var currency = "usd";

        // Act
        var money = Money.From(amount, currency);

        // Assert
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Should_NormalizeCurrencyToUpperCase_When_MixedCaseCurrencyIsProvided()
    {
        // Arrange
        var amount = 10m;
        var currency = "eUr";

        // Act
        var money = Money.From(amount, currency);

        // Assert
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Should_AcceptZeroAmount_When_ZeroIsProvided()
    {
        // Arrange
        var amount = 0m;
        var currency = "USD";

        // Act
        var money = Money.From(amount, currency);

        // Assert
        money.Amount.Should().Be(0m);
    }

    [Fact]
    public void Should_RoundAmountToTwoDecimalPlaces_When_AmountHasMoreThanTwoDecimalDigits()
    {
        // Arrange
        var amount = 10.999m;
        var currency = "USD";

        // Act
        var money = Money.From(amount, currency);

        // Assert
        money.Amount.Should().Be(10.99m);
    }

    [Fact]
    public void Should_TruncateTowardZero_When_AmountIsPositiveWithExcessDecimals()
    {
        // Arrange — MidpointRounding.ToZero truncates (floor for positive)
        var amount = 1.555m;
        var currency = "USD";

        // Act
        var money = Money.From(amount, currency);

        // Assert
        money.Amount.Should().Be(1.55m);
    }

    [Fact]
    public void Should_PreserveExactAmount_When_AmountHasTwoOrFewerDecimalPlaces()
    {
        // Arrange
        var amount = 99.99m;
        var currency = "USD";

        // Act
        var money = Money.From(amount, currency);

        // Assert
        money.Amount.Should().Be(99.99m);
    }

    [Fact]
    public void Should_ThrowArgumentException_When_AmountIsNegative()
    {
        // Arrange
        var negativeAmount = -0.01m;
        var currency = "USD";

        // Act
        var act = () => Money.From(negativeAmount, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Amount cannot be negative*");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_AmountIsLargeNegativeValue()
    {
        // Arrange
        var negativeAmount = -1000m;
        var currency = "USD";

        // Act
        var act = () => Money.From(negativeAmount, currency);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_ThrowArgumentException_When_CurrencyIsNull()
    {
        // Arrange
        var amount = 10m;
        string? currency = null;

        // Act
        var act = () => Money.From(amount, currency!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Currency must be a 3-letter ISO code*");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_CurrencyIsEmpty()
    {
        // Arrange
        var amount = 10m;
        var currency = "";

        // Act
        var act = () => Money.From(amount, currency);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_ThrowArgumentException_When_CurrencyIsWhiteSpace()
    {
        // Arrange
        var amount = 10m;
        var currency = "   ";

        // Act
        var act = () => Money.From(amount, currency);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_ThrowArgumentException_When_CurrencyHasFewerThanThreeLetters()
    {
        // Arrange
        var amount = 10m;
        var currency = "US";

        // Act
        var act = () => Money.From(amount, currency);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_ThrowArgumentException_When_CurrencyHasMoreThanThreeLetters()
    {
        // Arrange
        var amount = 10m;
        var currency = "USDT";

        // Act
        var act = () => Money.From(amount, currency);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_ReturnSum_When_AddingTwoMoneyInstancesWithSameCurrency()
    {
        // Arrange
        var a = Money.From(10.00m, "BRL");
        var b = Money.From(5.50m, "BRL");

        // Act
        var result = a.Add(b);

        // Assert
        result.Amount.Should().Be(15.50m);
        result.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Should_ReturnNewInstance_When_AddingMoney()
    {
        // Arrange
        var a = Money.From(10m, "USD");
        var b = Money.From(5m, "USD");

        // Act
        var result = a.Add(b);

        // Assert
        result.Should().NotBeSameAs(a);
        result.Should().NotBeSameAs(b);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_AddingMoneyWithDifferentCurrencies()
    {
        // Arrange
        var a = Money.From(10m, "USD");
        var b = Money.From(5m, "BRL");

        // Act
        var act = () => a.Add(b);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Currency mismatch*");
    }

    [Fact]
    public void Should_ReturnDifference_When_SubtractingMoneyWithSameCurrency()
    {
        // Arrange
        var a = Money.From(10.00m, "USD");
        var b = Money.From(3.50m, "USD");

        // Act
        var result = a.Subtract(b);

        // Assert
        result.Amount.Should().Be(6.50m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_SubtractResultIsNegative()
    {
        // Arrange
        var a = Money.From(5m, "USD");
        var b = Money.From(10m, "USD");

        // Act
        var act = () => a.Subtract(b);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_SubtractingMoneyWithDifferentCurrencies()
    {
        // Arrange
        var a = Money.From(10m, "USD");
        var b = Money.From(5m, "EUR");

        // Act
        var act = () => a.Subtract(b);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Currency mismatch*");
    }

    [Fact]
    public void Should_ReturnZero_When_SubtractingEqualMoneyValues()
    {
        // Arrange
        var a = Money.From(10m, "USD");
        var b = Money.From(10m, "USD");

        // Act
        var result = a.Subtract(b);

        // Assert
        result.Amount.Should().Be(0m);
    }

    [Fact]
    public void Should_ReturnScaledAmount_When_MultiplyingByPositiveFactor()
    {
        // Arrange
        var money = Money.From(10.00m, "USD");
        var factor = 3m;

        // Act
        var result = money.Multiply(factor);

        // Assert
        result.Amount.Should().Be(30.00m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Should_ReturnZero_When_MultiplyingByZero()
    {
        // Arrange
        var money = Money.From(50m, "BRL");
        var factor = 0m;

        // Act
        var result = money.Multiply(factor);

        // Assert
        result.Amount.Should().Be(0m);
    }

    [Fact]
    public void Should_RoundMultiplicationResult_When_ProductHasMoreThanTwoDecimals()
    {
        // Arrange
        var money = Money.From(10.00m, "USD");
        var factor = 0.333m; // 3.33... → truncated to 3.33

        // Act
        var result = money.Multiply(factor);

        // Assert
        result.Amount.Should().Be(3.33m);
    }

    [Fact]
    public void Should_ThrowArgumentException_When_MultiplyingByNegativeFactor()
    {
        // Arrange
        var money = Money.From(10m, "USD");
        var negativeFactor = -1m;

        // Act
        var act = () => money.Multiply(negativeFactor);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_ReturnTrue_When_AmountIsGreaterThanOther()
    {
        // Arrange
        var a = Money.From(20m, "USD");
        var b = Money.From(10m, "USD");

        // Act
        var result = a.GreaterThan(b);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_AmountIsLessThanOther()
    {
        // Arrange
        var a = Money.From(5m, "USD");
        var b = Money.From(10m, "USD");

        // Act
        var result = a.GreaterThan(b);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_AmountsAreEqual()
    {
        // Arrange
        var a = Money.From(10m, "USD");
        var b = Money.From(10m, "USD");

        // Act
        var result = a.GreaterThan(b);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_ComparingMoneyWithDifferentCurrencies()
    {
        // Arrange
        var a = Money.From(10m, "USD");
        var b = Money.From(5m, "EUR");

        // Act
        var act = () => a.GreaterThan(b);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Currency mismatch*");
    }

    [Fact]
    public void Should_BeEqual_When_TwoMoneyInstancesHaveSameAmountAndCurrency()
    {
        // Arrange
        var a = Money.From(100m, "BRL");
        var b = Money.From(100m, "BRL");

        // Act & Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Should_NotBeEqual_When_AmountsDiffer()
    {
        // Arrange
        var a = Money.From(100m, "BRL");
        var b = Money.From(200m, "BRL");

        // Act & Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void Should_NotBeEqual_When_CurrenciesDiffer()
    {
        // Arrange
        var a = Money.From(100m, "USD");
        var b = Money.From(100m, "BRL");

        // Act & Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void Should_ReturnFormattedString_When_ToStringIsCalled()
    {
        // Arrange
        var money = Money.From(1234.50m, "BRL");
        var expected = $"BRL {1234.50m:N2}";

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Should_ReturnZeroFormatted_When_AmountIsZero()
    {
        // Arrange
        var money = Money.From(0m, "USD");
        var expected = $"USD {0m:N2}";

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Should_DeserializeCorrectly_When_ValidJsonIsProvided()
    {
        // Arrange
        var json = """{"Amount":99.99,"Currency":"USD"}""";

        // Act
        var money = JsonSerializer.Deserialize<Money>(json);

        // Assert
        money.Should().NotBeNull();
        money!.Amount.Should().Be(99.99m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Should_SerializeAndDeserialize_When_RoundTrippingMoneyValue()
    {
        // Arrange
        var original = Money.From(42.00m, "EUR");

        // Act
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<Money>(json);

        // Assert
        restored.Should().Be(original);
    }
}