using FluentAssertions;
using SharedKernel.Exceptions;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Enums;
using Subscribers.Domain.Events;
using Subscribers.Domain.Exceptions;
using Xunit;

namespace Subscribers.Domain.Tests;

public class SubscriberTests
{
    [Fact]
    public void Should_CreateSubscriber_When_DataIsValid()
    {
        string username = "username_test";
        string email = "email@example.com";

        var subscriber = Subscriber.Create(username, email);

        subscriber.Should().NotBeNull();
        subscriber.Id.Should().NotBe(Guid.Empty);
        subscriber.UserName.Should().Be(username);
        subscriber.Email.Should().Be(email);
        subscriber.IsDeleted.Should().BeFalse();
        subscriber.Signatures.Should().BeEmpty();
        subscriber.DomainEvents.OfType<SubscriberCreatedDomainEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Shoud_ThrowValidationException_When_EmailIsInvalid()
    {
        string username = "username_test";
        string email = "invalid_email";

        Action act = () => Subscriber.Create(username, email);

        act.Should().Throw<ValidationException>()
            .WithMessage("The E-mail provided is invalid.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid username")]
    [InlineData("user@name")]
    public void Shoud_ThrowValidationException_When_UserNameIsInvalid(string username)
    {
        string email = "email@example.com";

        Action act = () => Subscriber.Create(username, email);

        act.Should().Throw<ValidationException>()
            .WithMessage("The username provided is invalid.");
    }

    [Fact]
    public void Shoud_AddSignature_When_SignatureIsValid()
    {
        string username = "username_test";
        string email = "email@example.com";
        Guid planId = Guid.NewGuid();
        var subscriber = Subscriber.Create(username, email);

        subscriber.AddSignature(planId);

        subscriber.Signatures.Should().ContainSingle(s => s.PlanId == planId);
        subscriber.DomainEvents.OfType<SignatureAddedDomainEvent>().Should().ContainSingle(e => e.PlanId == planId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Shoud_ThrowValidationException_When_PlanIdIsInvalid(string planIdStr)
    {
        string username = "username_test";
        string email = "email@example.com";
        Guid planId = Guid.TryParse(planIdStr, out var parsedPlanId) ? parsedPlanId : Guid.Empty;
        var subscriber = Subscriber.Create(username, email);

        Action act = () => subscriber.AddSignature(planId);

        act.Should().Throw<ValidationException>()
            .WithMessage("The plan provided is invalid.");
    }

    [Fact]
    public void Shoud_ThrowActiveSignatureExistsException_When_AddingSignatureWithActiveSignature()
    {
        string username = "username_test";
        string email = "email@example.com";
        var subscriber = Subscriber.Create(username, email);

        Action act = () =>
        {
            subscriber.AddSignature(Guid.NewGuid());
            subscriber.AddSignature(Guid.NewGuid());
        };

        act.Should().Throw<ActiveSignatureExistsException>()
            .WithMessage($"The subscriber '{subscriber.Id}' already has an active subscription.");
    }

    [Fact]
    public void Should_CancelSignature_When_SingleSignatureIsActive()
    {
        string username = "username_test";
        string email = "email@example.com";
        var subscriber = Subscriber.Create(username, email);
        subscriber.AddSignature(Guid.NewGuid());
        
        subscriber.CancelSignature();

        subscriber.Signatures.Should().ContainSingle(s => s.Status == ESignatureStatus.CANCELLED);
        subscriber.DomainEvents.OfType<SignatureCanceledDomainEvent>().Should().ContainSingle();
    }
}