namespace Identity.API.Models.Request;

public sealed record RegisterUserSubscriberRequest(string Email, string UserName, string Password);