using Payments.Domain.Customer.Events;
using Payments.Domain.Customer.Exceptions;
using SharedKernel;
using SharedKernel.Exceptions;

namespace Payments.Domain.Customer;

public sealed class Customer : AggregateRoot
{
    public Guid SubscriberId { get; private set; }
    public string ExternalId { get; private set; } = default!;
    public IReadOnlyList<CreditCard> CreditCards => _creditCards;
    private readonly List<CreditCard> _creditCards = [];

    public Customer(Guid subscriberId, string externalId)
    {
        if (subscriberId == Guid.Empty)
            throw new ArgumentException("Subscriber ID cannot be empty.", nameof(subscriberId));

        if (string.IsNullOrEmpty(externalId))
            throw new ArgumentException("External ID cannot be null or empty.", nameof(externalId));

        SubscriberId = subscriberId;
        ExternalId = externalId;
    }

    public void AddCreditCard(CreditCard creditCard)
    {
        if (creditCard == null)
            throw new ValidationException("The credit card provided is invalid.");

        if (CreditCards.Any(cc => cc.Equals(creditCard)))
            throw new CreditCardAlreadyExistsException("Credit card already exists.");

        _creditCards.Add(creditCard);

        AddDomainEvent(new CreditCardAddedDomainEvent(
            creditCard.Id,
            creditCard.Last4Digits,
            creditCard.Brand,
            SubscriberId));
    }

    public void RemoveCreditCard(CreditCard creditCard)
    {
        if (creditCard == null)
            throw new ValidationException("The credit card provided is invalid.");

        if (!CreditCards.Any(cc => cc.Id == creditCard.Id))
            return;

        var creditCardToRemove = _creditCards.First(cc => cc.Id == creditCard.Id);
        creditCardToRemove.Delete();

        AddDomainEvent(new CreditCardRemovedDomainEvent(creditCard.Id, SubscriberId));
    }
}
