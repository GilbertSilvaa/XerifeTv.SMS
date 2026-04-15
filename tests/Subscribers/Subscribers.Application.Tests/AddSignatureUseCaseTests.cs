using BuildingBlocks.Core;
using Moq;
using SharedKernel;
using Subscribers.Application.Commands.AddSignature;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;
using Xunit;
using FluentAssertions;
using Subscribers.Application.PlanCatalog;

namespace Subscribers.Application.Tests;

public class AddSignatureUseCaseTests
{
    private readonly Mock<ISubscribersRepository> _subscriberRepositoryMock;
    private readonly Mock<IPlanCatalogRepository> _planCatalogRepositoryMock;
    private readonly Mock<IUnitOfWork<Subscriber>> _unitOfWorkMock;
    private readonly AddSignatureCommandHandler _handler;

    public AddSignatureUseCaseTests()
    {
        _subscriberRepositoryMock = new Mock<ISubscribersRepository>();
        _planCatalogRepositoryMock = new Mock<IPlanCatalogRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork<Subscriber>>();

        _handler = new AddSignatureCommandHandler(
            _subscriberRepositoryMock.Object,
            _planCatalogRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_SubscriberExistsAndPlanFoundAndSignatureAdded()
    {
        // Arrange
        Guid identityUserId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        var command = new AddSignatureCommand(identityUserId, planId);

        var planItem = new PlanItemCatalog(planId, "Test Plan", 6, Money.From(9.99m, "USD"));
        _planCatalogRepositoryMock
            .Setup(x => x.GetByIdAsync(planId))
            .ReturnsAsync(planItem);

        var subscriberMock = Subscriber.Create("subscriber_test", "email@xample.com", identityUserId);

        _subscriberRepositoryMock
            .Setup(r => r.GetByIdentityUserIdAsync(identityUserId))
            .ReturnsAsync(subscriberMock);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ReturnFailure_When_SubscriberNotFound()
    {
        // Arrange
        Guid identityUserId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        var command = new AddSignatureCommand(identityUserId, planId);

        _subscriberRepositoryMock
            .Setup(r => r.GetByIdentityUserIdAsync(identityUserId))
            .ReturnsAsync((Subscriber?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("AddSignature.SubscriberNotFound");
    }

    [Fact]
    public async Task Should_ReturnFailure_When_PlanNotFound()
    {
        // Arrange
        var identityUserId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        var command = new AddSignatureCommand(identityUserId, planId);

        var subscriber = Subscriber.Create("subscriber_test", "email@xample.com", identityUserId);

        _subscriberRepositoryMock
            .Setup(r => r.GetByIdentityUserIdAsync(identityUserId))
            .ReturnsAsync(subscriber);

        _planCatalogRepositoryMock
            .Setup(p => p.GetByIdAsync(planId))
            .ReturnsAsync((PlanItemCatalog?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("AddSignature.PlanNotFound");
    }

    [Fact]
    public async Task Should_ReturnFailure_When_SubscriberThrowsDomainError()
    {
        // Arrange
        var identityUserId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        var command = new AddSignatureCommand(identityUserId, planId);

        var planItem = new PlanItemCatalog(planId, "Test Plan", 6, Money.From(9.99m, "USD"));

        _planCatalogRepositoryMock
            .Setup(x => x.GetByIdAsync(planId))
            .ReturnsAsync(planItem);

        var subscriberMock = Subscriber.Create("subscriber_test", "email@xample.com", identityUserId);
        subscriberMock.AddSignature(planItem.ToPlanSnapshot());

        _subscriberRepositoryMock
            .Setup(r => r.GetByIdentityUserIdAsync(identityUserId))
            .ReturnsAsync(subscriberMock);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Subscriber.SignatureActiveExists");
        result.Error.Description.Should().Contain("already has an active subscription");
    }
}
