using FluentAssertions;
using SharedKernel.Exceptions;
using Subscribers.Domain.Entities;
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
    }

    [Fact]
    public void Shoud_ThrowValidationException_When_EmailIsInvalid()
    {
        string username = "username_test";
        string email = "invalid_email";

        Action act = () => Subscriber.Create(username, email);

        act.Should().Throw<ValidationException>()
            .WithMessage("O email fornecido é inválido.");
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
            .WithMessage("O username fornecido é inválido.");
    }
}