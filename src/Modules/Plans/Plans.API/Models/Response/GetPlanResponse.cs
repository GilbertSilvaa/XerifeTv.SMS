using SharedKernel;

namespace Plans.API.Models.Response;

public sealed record GetPlanResponse(Guid Id, string Name, string Description, int Screens, Money Price);