namespace Subscribers.Application.Queries.ReadModels;

public sealed record SubscriberDto(string UserName, string Email, IReadOnlyList<SignatureDto> Signatures);