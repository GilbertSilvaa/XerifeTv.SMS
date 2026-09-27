namespace SharedKernel;

public sealed record Address
{
    public string Street { get; }
    public string Number { get; }
    public string Neighborhood { get; }
    public string? Complement { get; }
    public string ZipCode { get; }
    public string City { get; }
    public string State { get; }
    public string Country { get; }

    private Address(
        string street,
        string number,
        string neighborhood,
        string? complement,
        string zipCode,
        string city,
        string state,
        string country)
    {
        Street = street;
        Number = number;
        Neighborhood = neighborhood;
        Complement = complement;
        ZipCode = zipCode;
        City = city;
        State = state;
        Country = country;
    }

    public static Address Create(
        string street,
        string number,
        string neighborhood,
        string zipCode,
        string city,
        string state,
        string country,
        string? complement = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        ArgumentException.ThrowIfNullOrWhiteSpace(neighborhood);
        ArgumentException.ThrowIfNullOrWhiteSpace(zipCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);

        return new Address(
            street.Trim(),
            number.Trim(),
            neighborhood.Trim(),
            complement?.Trim(),
            zipCode.Trim(),
            city.Trim(),
            state.Trim(),
            country.Trim());
    }
}