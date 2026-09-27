using SharedKernel;
using SharedKernel.Exceptions;
using Subscribers.Domain.Enums;

namespace Subscribers.Domain.Entities;

public sealed class CreditCard : Entity
{
    public ECardBrand Brand { get; private set; }
    public string Last4Digits { get; private set; } = default!;
    public int ExpiryMonth { get; private set; }
    public int ExpiryYear { get; private set; }
    public string GatewayToken { get; private set; } = default!;

    public CreditCard(
        ECardBrand brand,
        string last4Digits,
        int expiryMonth,
        int expiryYear,
        string gatewayToken)
    {
        Brand = brand;
        Last4Digits = last4Digits;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        GatewayToken = gatewayToken;

        Validate();
    }

    public bool Equals(CreditCard other)
    {
        if (other is null) return false;

        return Brand == other.Brand &&
               Last4Digits == other.Last4Digits &&
               ExpiryMonth == other.ExpiryMonth &&
               ExpiryYear == other.ExpiryYear;
    }

    private CreditCard() { }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Last4Digits) || Last4Digits.Length != 4)
            throw new ArgumentException("Last 4 digits are required and must be 4 characters long.", nameof(Last4Digits));

        if (ExpiryMonth < 1 || ExpiryMonth > 12)
            throw new ArgumentException("Expiry month must be between 1 and 12.", nameof(ExpiryMonth));

        if (string.IsNullOrWhiteSpace(GatewayToken))
            throw new ArgumentException("Gateway token is required.", nameof(GatewayToken));

        var expirationLimit = new DateTime(ExpiryYear, ExpiryMonth, 1).AddMonths(1);

        if (expirationLimit <= DateTime.UtcNow)
            throw new ValidationException("Credit card is expired.");
    }
}
