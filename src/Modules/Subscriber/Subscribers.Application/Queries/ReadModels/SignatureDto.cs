using Subscribers.Domain.Enums;
using Subscribers.Domain.ValueObjects;

namespace Subscribers.Application.Queries.ReadModels;

public sealed record SignatureDto(
    PlanSnapshot Plan,
    ESignatureStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? RenewalDate);