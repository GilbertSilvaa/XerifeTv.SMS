using BuildingBlocks.Behaviors;
using BuildingBlocks.Integration.Tests.Fakes.Validation;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace BuildingBlocks.Integration.Tests.BehaviorsTests;

public class ValidationPipelineBehaviorTests
{
    private readonly IMediator _mediator;

    public ValidationPipelineBehaviorTests()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<FakeValidationCommand>();
        });

        services.AddTransient<
            IPipelineBehavior<FakeValidationCommand, Result>,
            ValidationPipelineBehavior<FakeValidationCommand, Result>>();

        services.AddValidatorsFromAssemblyContaining<FakeValidationCommandValidator>();

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Should_ReturnValidationFailure_When_RequestIsInvalid()
    {
        // Arrange
        var command = new FakeValidationCommand("");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Code.Should().Be("ValidationResult.Errors");
    }

    [Fact]
    public async Task Should_ExecuteHandler_When_RequestIsValid()
    {
        // Arrange
        var command = new FakeValidationCommand("John");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}