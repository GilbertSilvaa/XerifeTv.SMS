using FluentAssertions;
using SharedKernel;

namespace SharedKernel.Tests;

public class AddressTests
{
    [Fact]
    public void Should_CreateAddress_When_AllRequiredFieldsAreProvided()
    {
        // Arrange
        var street = "Av. Paulista";
        var number = "1000";
        var neighborhood = "Bela Vista";
        var zipCode = "01310100";
        var city = "São Paulo";
        var state = "SP";
        var country = "BR";

        // Act
        var address = Address.Create(street, number, neighborhood, zipCode, city, state, country);

        // Assert
        address.Street.Should().Be("Av. Paulista");
        address.Number.Should().Be("1000");
        address.Neighborhood.Should().Be("Bela Vista");
        address.ZipCode.Should().Be("01310100");
        address.City.Should().Be("São Paulo");
        address.State.Should().Be("SP");
        address.Country.Should().Be("BR");
        address.Complement.Should().BeNull();
    }

    [Fact]
    public void Should_SetComplement_When_ComplementIsProvided()
    {
        // Arrange
        var complement = "Apto 52";

        // Act
        var address = Address.Create(
            "Av. Paulista", "1000", "Bela Vista", "01310100", "São Paulo", "SP", "BR",
            complement: complement);

        // Assert
        address.Complement.Should().Be("Apto 52");
    }

    [Fact]
    public void Should_TrimWhitespace_When_FieldsHaveLeadingOrTrailingSpaces()
    {
        // Arrange
        var street = "  Av. Paulista  ";
        var zipCode = " 01310100 ";

        // Act
        var address = Address.Create(street, "1000", "Bela Vista", zipCode, "São Paulo", "SP", "BR");

        // Assert
        address.Street.Should().Be("Av. Paulista");
        address.ZipCode.Should().Be("01310100");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_StreetIsNullOrWhiteSpace(string? invalidStreet)
    {
        // Act
        Action act = () => Address.Create(invalidStreet!, "1000", "Bela Vista", "01310100", "São Paulo", "SP", "BR");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_NumberIsNullOrWhiteSpace(string? invalidNumber)
    {
        // Act
        Action act = () => Address.Create("Av. Paulista", invalidNumber!, "Bela Vista", "01310100", "São Paulo", "SP", "BR");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_NeighborhoodIsNullOrWhiteSpace(string? invalidNeighborhood)
    {
        // Act
        Action act = () => Address.Create("Av. Paulista", "1000", invalidNeighborhood!, "01310100", "São Paulo", "SP", "BR");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_ZipCodeIsNullOrWhiteSpace(string? invalidZipCode)
    {
        // Act
        Action act = () => Address.Create("Av. Paulista", "1000", "Bela Vista", invalidZipCode!, "São Paulo", "SP", "BR");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_CityIsNullOrWhiteSpace(string? invalidCity)
    {
        // Act
        Action act = () => Address.Create("Av. Paulista", "1000", "Bela Vista", "01310100", invalidCity!, "SP", "BR");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_StateIsNullOrWhiteSpace(string? invalidState)
    {
        // Act
        Action act = () => Address.Create("Av. Paulista", "1000", "Bela Vista", "01310100", "São Paulo", invalidState!, "BR");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_CountryIsNullOrWhiteSpace(string? invalidCountry)
    {
        // Act
        Action act = () => Address.Create("Av. Paulista", "1000", "Bela Vista", "01310100", "São Paulo", "SP", invalidCountry!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_BeEqual_When_AllPropertiesAreTheSame()
    {
        // Arrange
        var address1 = Address.Create("Av. Paulista", "1000", "Bela Vista", "01310100", "São Paulo", "SP", "BR");
        var address2 = Address.Create("Av. Paulista", "1000", "Bela Vista", "01310100", "São Paulo", "SP", "BR");

        // Act & Assert
        address1.Should().Be(address2);
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void Should_NotBeEqual_When_AnyPropertyDiffers()
    {
        // Arrange
        var address1 = Address.Create("Av. Paulista", "1000", "Bela Vista", "01310100", "São Paulo", "SP", "BR");
        var address2 = Address.Create("Av. Paulista", "1001", "Bela Vista", "01310100", "São Paulo", "SP", "BR");

        // Act & Assert
        address1.Should().NotBe(address2);
    }
}