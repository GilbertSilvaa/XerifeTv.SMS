namespace Payments.Application.Abstractions;

public sealed record CreateCustomerRequest
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}
