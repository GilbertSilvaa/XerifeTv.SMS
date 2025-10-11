namespace Identity.API.Models.Request;

public sealed record LoginUserRequest(string UserName, string Password);
