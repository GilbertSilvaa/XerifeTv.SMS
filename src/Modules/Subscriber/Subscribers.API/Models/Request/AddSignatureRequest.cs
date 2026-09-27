using Subscribers.Domain.ValueObjects;

namespace Subscribers.API.Models.Request;

public sealed record AddSignatureRequest(Guid PlanId, PaymentMethod PaymentMethod);