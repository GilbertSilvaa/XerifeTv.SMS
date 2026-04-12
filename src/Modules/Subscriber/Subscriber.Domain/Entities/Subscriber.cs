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

    private readonly List<Signature> _signatures = [];
    public IReadOnlyList<Signature> Signatures => _signatures;

    private Subscriber() { }

    private Subscriber(string userName, string email)
    {
        UserName = userName;
        Email = email;
    }

    public static Subscriber Create(string userName, string email)
    {
        if (!IsValidUserName(userName))
            throw new ValidationException("The username provided is invalid.");

        if (!IsValidEmail(email))
            throw new ValidationException("The E-mail provided is invalid.");

        var subscriber = new Subscriber(userName, email);

        subscriber.AddDomainEvent(new SubscriberCreatedDomainEvent(subscriber.Id, subscriber.Email, userName));

        return subscriber;
    }

    public override bool Delete()
    {
        if (Signatures.Where(s => s.Status != Enums.ESignatureStatus.CANCELLED).Any())
            throw new ActiveSignatureExistsException("The subscriber cannot be deleted because they have an active subscription.");

        bool isDeleted = base.Delete();

        if (isDeleted)
            AddDomainEvent(new SubscriberDeletedDomainEvent(Id, Email, UserName, DeletedAt ?? default));

        return isDeleted;
    }

    public void AddSignature(PlanSnapshot plan)
    {
        if (plan.PlanId == Guid.Empty)
            throw new ValidationException("The plan provided is invalid.");

        if (Signatures.Where(s => s.Status != Enums.ESignatureStatus.CANCELLED).Any())
            throw new ActiveSignatureExistsException(Id);

        var signature = Signature.Create(plan, subscriberId: Id);

        _signatures.Add(signature);
        AddDomainEvent(new SignatureAddedDomainEvent(signature.Id, signature.Plan.PlanId, Id));
    }

    public void CancelSignature()
    {
        var signatureActiveOrPending = Signatures
            .Where(s => s.Status != Enums.ESignatureStatus.CANCELLED)
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