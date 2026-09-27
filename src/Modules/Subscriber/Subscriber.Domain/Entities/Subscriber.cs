using SharedKernel;
using SharedKernel.Exceptions;
using Subscribers.Domain.Events;
using Subscribers.Domain.Exceptions;
using Subscribers.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace Subscribers.Domain.Entities;

public sealed class Subscriber : AggregateRoot
{
    public string UserName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public Guid IdentityUserId { get; private set; }
    public string? GatewayCustomerId { get; private set; }

    private readonly List<Signature> _signatures = [];
    public IReadOnlyList<Signature> Signatures => _signatures;

    private readonly List<CreditCard> _creditCards = [];
    public IReadOnlyList<CreditCard> CreditCards => [.. _creditCards.Where(cc => !cc.IsDeleted)];

    public Signature? ActiveSignature => _signatures
        .Where(s => s.IsActiveOrPending())
        .FirstOrDefault();

    private Subscriber() { }

    private Subscriber(string userName, string email, Guid identityUserId)
    {
        UserName = userName;
        Email = email;
        IdentityUserId = identityUserId;
    }

    public static Subscriber Create(string userName, string email, Guid identityUserId)
    {
        if (!IsValidUserName(userName))
            throw new ValidationException("The username provided is invalid.");

        if (!IsValidEmail(email))
            throw new ValidationException("The E-mail provided is invalid.");

        if (identityUserId == Guid.Empty)
            throw new ValidationException("The Identity User ID provided is invalid.");

        var subscriber = new Subscriber(userName, email, identityUserId);

        subscriber.AddDomainEvent(new SubscriberCreatedDomainEvent(subscriber.Id, subscriber.Email, userName));

        return subscriber;
    }

    public override bool Delete()
    {
        if (Signatures.Where(s => s.IsActiveOrPending()).Any())
            throw new ActiveSignatureExistsException("The subscriber cannot be deleted because they have an active/pending subscription.");

        bool isDeleted = base.Delete();

        if (isDeleted)
            AddDomainEvent(new SubscriberDeletedDomainEvent(Id, Email, UserName, DeletedAt ?? default));

        return isDeleted;
    }

    public void AddSignature(PlanSnapshot plan, PaymentMethod paymentMethod)
    {
        if (plan.PlanId == Guid.Empty)
            throw new ValidationException("The plan provided is invalid.");

        if (Signatures.Where(s => s.IsActiveOrPending()).Any())
            throw new ActiveSignatureExistsException();

        if (paymentMethod.CreditCardId != null && !CreditCards.Any(cc => cc.Id == paymentMethod.CreditCardId))
            throw new ValidationException("Credit card not found.");

        var signature = Signature.Create(plan, paymentMethod, subscriberId: Id);

        _signatures.Add(signature);
        AddDomainEvent(new SignatureAddedDomainEvent(signature.Id, signature.Plan.PlanId, Id));
    }

    public void CancelSignature()
    {
        var signatureActiveOrPending = Signatures
            .Where(s => s.IsActiveOrPending())
            .FirstOrDefault();

        if (signatureActiveOrPending == null)
            return;

        signatureActiveOrPending.Cancel();

        AddDomainEvent(new SignatureCanceledDomainEvent(
            signatureActiveOrPending.Id,
            signatureActiveOrPending.Plan.PlanId,
            SubscriberId: Id,
            signatureActiveOrPending.StartDate ?? default,
            signatureActiveOrPending.EndDate ?? default));
    }

    public void SetGatewayCustomerId(string gatewayCustomerId)
    {
        if (string.IsNullOrWhiteSpace(gatewayCustomerId))
            throw new ValidationException("The gateway customer ID provided is invalid.");

        GatewayCustomerId = gatewayCustomerId;
    }

    public void AddCreditCard(CreditCard creditCard)
    {
        if (creditCard == null)
            throw new ValidationException("The credit card provided is invalid.");

        if (CreditCards.Any(cc => cc.Equals(creditCard)))
            throw new CreditCardAlreadyExistsException("Credit card already exists.");

        _creditCards.Add(creditCard);

        AddDomainEvent(new CreditCardAddedDomainEvent(creditCard.Id, Id));
    }

    public void RemoveCreditCard(CreditCard creditCard)
    {
        if (creditCard == null)
            throw new ValidationException("The credit card provided is invalid.");

        if (!CreditCards.Any(cc => cc.Id == creditCard.Id))
            return;

        if (ActiveSignature?.PaymentMethod.CreditCardId == creditCard.Id)
            throw new CreditCardLinkedToActiveSignatureException("Cannot remove credit card linked to an active signature.");

        var creditCardToRemove = _creditCards.First(cc => cc.Id == creditCard.Id);
        creditCardToRemove.Delete();

        AddDomainEvent(new CreditCardRemovedDomainEvent(creditCard.Id, Id));
    }

    public void UpdatePaymentMethodSignature(PaymentMethod paymentMethod)
    {
        if (ActiveSignature == null)
            throw new ValidationException("No active signature found.");

        if (paymentMethod.CreditCardId != null && !CreditCards.Any(cc => cc.Id == paymentMethod.CreditCardId))
            throw new ValidationException("Credit card not found.");

        ActiveSignature.UpdatePaymentMethod(paymentMethod);

        AddDomainEvent(new SignaturePaymentMethodUpdatedDomainEvent(
            ActiveSignature.Id,
            SubscriberId: Id,
            paymentMethod.Type,
            paymentMethod.CreditCardId));
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return ValidationsRegex.EmailRegex().IsMatch(email);
    }

    private static bool IsValidUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return false;

        return ValidationsRegex.UserNameRegex().IsMatch(userName);
    }
}

public static partial class ValidationsRegex
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    public static partial Regex EmailRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled)]
    public static partial Regex UserNameRegex();
}