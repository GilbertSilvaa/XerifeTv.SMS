using System.Text.Json.Serialization;

namespace SharedKernel;

public sealed record Money
{
	public decimal Amount { get; private set; }
	public string Currency { get; private set; }

	[JsonConstructor]
	private Money(decimal amount, string currency)
	{
		if (amount < 0)
			throw new ArgumentException("Amount cannot be negative.", nameof(amount));

		if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
			throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

		Amount = decimal.Round(amount, 2, MidpointRounding.ToZero);
		Currency = currency.ToUpperInvariant();
	}

	public static Money From(decimal amount, string currency) => new(amount, currency);

	public Money Add(Money other)
	{
		EnsureSameCurrency(other);
		return new Money(Amount + other.Amount, Currency);
	}

	public Money Subtract(Money other)
	{
		EnsureSameCurrency(other);
		return new Money(Amount - other.Amount, Currency);
	}

	public Money Multiply(decimal factor) =>
		new(Amount * factor, Currency);

	public bool GreaterThan(Money other)
	{
		EnsureSameCurrency(other);
		return Amount > other.Amount;
	}

	private void EnsureSameCurrency(Money other)
	{
		if (Currency != other.Currency)
			throw new InvalidOperationException($"Currency mismatch: {Currency} vs {other.Currency}");
	}

	public override string ToString() => $"{Currency} {Amount:N2}";
}
