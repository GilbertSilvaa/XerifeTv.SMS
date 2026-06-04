using FluentAssertions;

namespace SharedKernel.Tests;

public class ResultTests
{
    [Fact]
    public void Should_HaveEmptyCode_When_ErrorNoneIsUsed()
    {
        // Arrange & Act
        var error = Error.None;

        // Assert
        error.Code.Should().BeEmpty();
    }

    [Fact]
    public void Should_HaveNullDescription_When_ErrorNoneIsUsed()
    {
        // Arrange & Act
        var error = Error.None;

        // Assert
        error.Description.Should().BeNull();
    }

    [Fact]
    public void Should_CreateErrorWithNullDescription_When_OnlyCodeIsProvided()
    {
        // Arrange & Act
        var error = new Error("User.NotFound");

        // Assert
        error.Code.Should().Be("User.NotFound");
        error.Description.Should().BeNull();
    }

    [Fact]
    public void Should_CreateErrorWithDescription_When_CodeAndDescriptionAreProvided()
    {
        // Arrange & Act
        var error = new Error("User.NotFound", "The user was not found.");

        // Assert
        error.Code.Should().Be("User.NotFound");
        error.Description.Should().Be("The user was not found.");
    }

    [Fact]
    public void Should_PopulateCallerMemberName_When_ErrorIsCreated()
    {
        // Arrange & Act
        var error = new Error("Some.Error");

        // Assert
        error.MemberName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Should_PopulateCallerFilePath_When_ErrorIsCreated()
    {
        // Arrange & Act
        var error = new Error("Some.Error");

        // Assert
        error.FilePath.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Should_PopulateCallerLineNumber_When_ErrorIsCreated()
    {
        // Arrange & Act
        var error = new Error("Some.Error");

        // Assert
        error.LineNumber.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Should_BeEqual_When_TwoErrorsHaveSameCodeAndDescription()
    {
        // Arrange
        var a = new Error("User.NotFound", "Not found", MemberName: "", FilePath: "", LineNumber: 0);
        var b = new Error("User.NotFound", "Not found", MemberName: "", FilePath: "", LineNumber: 0);

        // Act & Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Should_NotBeEqual_When_ErrorCodesAreDifferent()
    {
        // Arrange
        var a = new Error("User.NotFound", Description: null, MemberName: "", FilePath: "", LineNumber: 0);
        var b = new Error("User.Forbidden", Description: null, MemberName: "", FilePath: "", LineNumber: 0);

        // Act & Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void Should_NotBeEqualToNone_When_ErrorHasNonEmptyCode()
    {
        // Arrange
        var error = new Error("Validation.Required", MemberName: "", FilePath: "", LineNumber: 0);

        // Act & Assert
        error.Should().NotBe(Error.None);
    }

    [Fact]
    public void Should_BeSuccess_When_ResultSuccessIsCreated()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveErrorNone_When_ResultSuccessIsCreated()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Should_BeFailure_When_ResultFailureIsCreated()
    {
        // Arrange
        var error = new Error("General.Error");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Should_PreserveError_When_ResultFailureIsCreated()
    {
        // Arrange
        var error = new Error("General.Error", "Something went wrong.");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Should_ThrowArgumentException_When_ResultFailureIsCreatedWithErrorNone()
    {
        // Arrange
        var act = () => Result.Failure(Error.None);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid error*");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_ResultSuccessIsCreatedWithNonNoneError()
    {
        // Arrange
        var error = new Error("Some.Error");
        var act = () =>
        {
            var ctor = typeof(Result)
                .GetConstructor(
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    [typeof(bool), typeof(Error)])!;
            ctor.Invoke([true, error]);
        };

        // Assert
        act.Should().Throw<Exception>()
            .WithInnerException<ArgumentException>()
            .WithMessage("*Invalid error*");
    }

    [Fact]
    public void Should_BeSuccess_When_TypedResultSuccessIsCreated()
    {
        // Arrange & Act
        var result = Result<string>.Success("ok");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveErrorNone_When_TypedResultSuccessIsCreated()
    {
        // Arrange & Act
        var result = Result<int>.Success(42);

        // Assert
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Should_ExposeData_When_TypedResultSuccessIsCreated()
    {
        // Arrange & Act
        var result = Result<string>.Success("hello");

        // Assert
        result.Data.Should().Be("hello");
    }

    [Fact]
    public void Should_ExposeComplexObjectData_When_TypedResultSuccessIsCreatedWithObject()
    {
        // Arrange
        var payload = new { Id = 1, Name = "Alice" };

        // Act
        var result = Result<object>.Success(payload);

        // Assert
        result.Data.Should().Be(payload);
    }

    [Fact]
    public void Should_AcceptNullData_When_TypedResultSuccessIsCreatedWithNullableType()
    {
        // Arrange & Act
        var result = Result<string?>.Success(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Should_BeFailure_When_TypedResultFailureIsCreated()
    {
        // Arrange
        var error = new Error("Resource.NotFound");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Should_PreserveError_When_TypedResultFailureIsCreated()
    {
        // Arrange
        var error = new Error("Resource.NotFound", "The resource was not found.");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Should_HaveNullData_When_TypedResultFailureIsCreatedForReferenceType()
    {
        // Arrange
        var error = new Error("Resource.NotFound");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Should_HaveDefaultData_When_TypedResultFailureIsCreatedForValueType()
    {
        // Arrange
        var error = new Error("Calc.Error");

        // Act
        var result = Result<int>.Failure(error);

        // Assert
        result.Data.Should().Be(default(int));
    }

    [Fact]
    public void Should_ThrowArgumentException_When_TypedResultFailureIsCreatedWithErrorNone()
    {
        // Arrange
        var act = () => Result<string>.Failure(Error.None);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid error*");
    }

    [Fact]
    public void Should_HaveIsFailureAsInverseOfIsSuccess_When_ResultIsSuccess()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.IsFailure.Should().Be(!result.IsSuccess);
    }

    [Fact]
    public void Should_HaveIsFailureAsInverseOfIsSuccess_When_ResultIsFailure()
    {
        // Arrange & Act
        var result = Result.Failure(new Error("X.Error"));

        // Assert
        result.IsFailure.Should().Be(!result.IsSuccess);
    }

    [Fact]
    public void Should_HaveIsFailureAsInverseOfIsSuccess_When_TypedResultIsSuccess()
    {
        // Arrange & Act
        var result = Result<int>.Success(1);

        // Assert
        result.IsFailure.Should().Be(!result.IsSuccess);
    }

    [Fact]
    public void Should_HaveIsFailureAsInverseOfIsSuccess_When_TypedResultIsFailure()
    {
        // Arrange & Act
        var result = Result<int>.Failure(new Error("X.Error"));

        // Assert
        result.IsFailure.Should().Be(!result.IsSuccess);
    }
}