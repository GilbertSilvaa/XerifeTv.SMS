using SharedKernel;

namespace Plans.API.Models.Request;

public sealed record AdjustPricePlanRequest(Guid Id, Money Price);