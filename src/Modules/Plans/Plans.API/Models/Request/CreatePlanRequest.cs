using SharedKernel;

namespace Plans.API.Models.Request;

public sealed record CreatePlanRequest(string Name, string Description, int Screens, Money Price);