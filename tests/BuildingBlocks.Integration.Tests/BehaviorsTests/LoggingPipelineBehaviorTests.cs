using BuildingBlocks.Behaviors;
using BuildingBlocks.Integration.Tests.Fakes.Validation;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel;

namespace BuildingBlocks.Integration.Tests.BehaviorsTests;

public class LoggingPipelineBehaviorTests
{
    private readonly IMediator _mediator;
    private readonly Mock<ILogger<LoggingPipelineBehavior<FakeValidationCommand, Result>>> _loggerMock;

    public LoggingPipelineBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<LoggingPipelineBehavior<FakeValidationCommand, Result>>>();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddSingleton(_loggerMock.Object);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<FakeValidationCommand>();
        });

        services.AddTransient<
            IPipelineBehavior<FakeValidationCommand, Result>,
            LoggingPipelineBehavior<FakeValidationCommand, Result>>();

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Should_LogHandlingRequest_When_CommandIsSent()
    {
        // Arrange
        var command = new FakeValidationCommand("John");

        // Act
        await _mediator.Send(command);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Handling request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_LogRequestName_When_CommandIsSent()
    {
        // Arrange
        var command = new FakeValidationCommand("John");

        // Act
        await _mediator.Send(command);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(nameof(FakeValidationCommand))),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Should_LogCompletedSuccessfully_When_HandlerSucceeds()
    {
        // Arrange
        var command = new FakeValidationCommand("John");

        // Act
        await _mediator.Send(command);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("completed successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_HandlerSucceeds()
    {
        // Arrange
        var command = new FakeValidationCommand("John");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}