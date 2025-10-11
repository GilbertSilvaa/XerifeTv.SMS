namespace Plans.API.Models.Request;

public sealed record AdjustSimultaneousScreensPlanRequest(Guid Id, int MaxSimultaneousScreens);