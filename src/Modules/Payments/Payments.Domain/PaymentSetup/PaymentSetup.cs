using SharedKernel;
using SharedKernel.Exceptions;

namespace Payments.Domain.PaymentSetup;

public sealed class PaymentSetup : AggregateRoot
{
    public string SessionId { get; private set; } = default!;
    public DateTimeOffset ExpiresAt { get; }
    public EPaymentSetupStatus Status { get; private set; }
    public Guid CustomerId { get; private set; }

    public PaymentSetup(string sessionId, DateTimeOffset expiresAt, Guid customerId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));

        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));

        SessionId = sessionId;
        ExpiresAt = expiresAt;
        CustomerId = customerId;

        Status = EPaymentSetupStatus.PENDING;
    }

    public void MarkAsCompleted()
    {
        if (Status != EPaymentSetupStatus.PENDING)
            throw new ValidationException("Payment setup can only be marked as completed if it is pending.");

        Status = EPaymentSetupStatus.COMPLETED;
    }

    public void MarkAsExpired()
    {
        if (Status != EPaymentSetupStatus.PENDING)
            throw new ValidationException("Payment setup can only be marked as expired if it is pending.");

        Status = EPaymentSetupStatus.EXPIRED;
    }

    public bool CanBeReused(DateTimeOffset now) =>
        Status == EPaymentSetupStatus.PENDING && ExpiresAt > now;
}
