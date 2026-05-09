using FluentAssertions;
using SharedKernel;
using SharedKernel.Exceptions;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Enums;
using Subscribers.Domain.Events;
using Subscribers.Domain.Exceptions;
using Subscribers.Domain.ValueObjects;
using Xunit;

namespace Subscribers.Domain.Tests;

public class SubscriberTests
{
    [Fact]
    public void Should_CreateSubscriber_When_CreatingUserWithDataIsValid()
    {
        // Arrange
        string username = "username_test";
        string email = "email@example.com";
        Guid identityUserId = Guid.NewGuid();

        // Act
        var subscriber = Subscriber.Create(username, email, identityUserId);

        // Assert
        subscriber.Should().NotBeNull();
        subscriber.Id.Should().NotBe(Guid.Empty);
        subscriber.UserName.Should().Be(username);
        subscriber.Email.Should().Be(email);
        subscriber.IsDeleted.Should().BeFalse();
        subscriber.Signatures.Should().BeEmpty();
        subscriber.DomainEvents.OfType<SubscriberCreatedDomainEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Shoud_ThrowValidationException_When_CreatingUserWithEmailIsInvalid()
    {
        // Arrange
        string username = "username_test";
        string email = "invalid_email";
        Guid identityUserId = Guid.NewGuid();

        // Act
        Action act = () => Subscriber.Create(username, email, identityUserId);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("The E-mail provided is invalid.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid username")]
    [InlineData("user@name")]
    public void Shoud_ThrowValidationException_When_CreatingUserWithUserNameIsInvalid(string username)
    {
        // Arrange
        string email = "email@example.com";
        Guid identityUserId = Guid.NewGuid();

        // Act
        Action act = () => Subscriber.Create(username, email, identityUserId);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("The username provided is invalid.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Shoud_ThrowValidationException_When_CreatingUserWithIdentityUserIdIsInvalid(string identityUserIdStr)
    {
        // Arrange
        string username = "username_test";
        string email = "email@example.com";
        Guid identityUserId = Guid.TryParse(identityUserIdStr, out var parsedIdentityUserId) ? parsedIdentityUserId : Guid.Empty;
        
        // Act
        Action act = () => Subscriber.Create(username, email, identityUserId);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("The Identity User ID provided is invalid.");
    }

    [Fact]
    public void Shoud_AddSignature_When_AddingSignatureWithDataIsValid()
    {
        // Arrange
        string username = "username_test";
        string email = "email@example.com";
        Guid identityUserId = Guid.NewGuid();

        Guid planId = Guid.NewGuid();
        string planName = "Test Plan";
        int planMaxSimultaneousScreens = 4;
        Money planPrice = Money.From(9.99m, "USD");

        var plan = new PlanSnapshot(planId, planName, planMaxSimultaneousScreens, planPrice);
        var subscriber = Subscriber.Create(username, email, identityUserId);

        // Act
        subscriber.AddSignature(plan);

        // Assert
        subscriber.Signatures.Should().ContainSingle(s => s.Plan.PlanId == planId);
        subscriber.DomainEvents.OfType<SignatureAddedDomainEvent>().Should().ContainSingle(e => e.PlanId == planId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Shoud_ThrowValidationException_When_AddingSignatureWithPlanIdIsInvalid(string planIdStr)
    {
        // Arrange
        string username = "username_test";
        string email = "email@example.com";
        Guid identityUserId = Guid.NewGuid();

        Guid planId = Guid.TryParse(planIdStr, out var parsedPlanId) ? parsedPlanId : Guid.Empty;
        string planName = "Test Plan";
        int planMaxSimultaneousScreens = 4;
        Money planPrice = Money.From(9.99m, "USD");

        var plan = new PlanSnapshot(planId, planName, planMaxSimultaneousScreens, planPrice);
        var subscriber = Subscriber.Create(username, email, identityUserId);

        // Act
        Action act = () => subscriber.AddSignature(plan);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("The plan provided is invalid.");
    }

    [Fact]
    public void Shoud_ThrowActiveSignatureExistsException_When_AddingSignatureWithActiveSignature()
    {
        // Arrange
        string username = "username_test";
        string email = "email@example.com";
        Guid identityUserId = Guid.NewGuid();

        var subscriber = Subscriber.Create(username, email, identityUserId);

        Guid planId = Guid.NewGuid();
        string planName = "Test Plan";
        int planMaxSimultaneousScreens = 4;
        Money planPrice = Money.From(9.99m, "USD");

        var plan1 = new PlanSnapshot(planId, planName, planMaxSimultaneousScreens, planPrice);
        var plan2 = new PlanSnapshot(Guid.NewGuid(), "Another Plan", 2, Money.From(4.99m, "USD"));

        // Act
        Action act = () =>
        {
            subscriber.AddSignature(plan1);
            subscriber.AddSignature(plan2);
        };

        // Assert
        act.Should().Throw<ActiveSignatureExistsException>()
            .WithMessage($"The subscriber already has an active subscription.");
    }

    [Fact]
    public void Should_CancelSignature_When_SingleSignatureIsActive()
    {
        // Arrange
        string username = "username_test";
        string email = "email@example.com";
        Guid identityUserId = Guid.NewGuid();

        Guid planId = Guid.NewGuid();
        string planName = "Test Plan";
        int planMaxSimultaneousScreens = 4;
        Money planPrice = Money.From(9.99m, "USD");

        var plan = new PlanSnapshot(planId, planName, planMaxSimultaneousScreens, planPrice);

        var subscriber = Subscriber.Create(username, email, identityUserId);
        subscriber.AddSignature(plan);

        // Act
        subscriber.CancelSignature();

        // Assert
        subscriber.Signatures.Should().ContainSingle(s => s.Status == ESignatureStatus.CANCELLED);
        subscriber.DomainEvents.OfType<SignatureCanceledDomainEvent>().Should().ContainSingle();
    }
}