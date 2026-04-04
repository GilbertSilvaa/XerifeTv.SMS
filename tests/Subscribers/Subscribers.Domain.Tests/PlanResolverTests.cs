using Moq;
using SharedKernel;
using Xunit;
using FluentAssertions;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;
using Subscribers.Domain.Services;

namespace Subscribers.Domain.Tests;

public class PlanResolverTests
{
    [Fact]
    public async Task Should_ReturnPlan_When_PlanIsValid()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        string planName = "Test Plan";
        int planMaxSimultaneousScreens = 4;
        Money planPrice = Money.From(9.99m, "USD");

        var plan = new PlanItemCatalog(planId, planName, planMaxSimultaneousScreens, planPrice);

        var repoMock = new Mock<IPlanCatalogRepository>();
        repoMock.Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        var planResolver = new PlanResolver(repoMock.Object);

        // Act
        var result = await planResolver.ResolveActivePlanAsync(planId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(plan);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_PlanNotFound()
    {
        // Arrange
        Guid planId = Guid.NewGuid();

        var repoMock = new Mock<IPlanCatalogRepository>();
        repoMock.Setup(r => r.GetByIdAsync(planId))
                .ReturnsAsync((PlanItemCatalog?)null);

        var planResolver = new PlanResolver(repoMock.Object);

        // Act
        var result = await planResolver.ResolveActivePlanAsync(planId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Code.Should().Be("PlanResolver.PlanNotFound");
        result.Error.Description.Should().Be("Plan not found.");
    }

    [Fact]
    public async Task Should_ReturnFailure_When_PlanIsInactive()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        string planName = "Test Plan";
        int planMaxSimultaneousScreens = 4;
        Money planPrice = Money.From(9.99m, "USD");

        var plan = new PlanItemCatalog(planId, planName, planMaxSimultaneousScreens, planPrice);
        plan.Disable();

        var repoMock = new Mock<IPlanCatalogRepository>();
        repoMock.Setup(r => r.GetByIdAsync(planId))
                .ReturnsAsync(plan);

        var planResolver = new PlanResolver(repoMock.Object);

        // Act
        var result = await planResolver.ResolveActivePlanAsync(planId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Code.Should().Be("PlanResolver.PlanIsDisaled");
        result.Error.Description.Should().Be("Plan is not active.");
    }
}