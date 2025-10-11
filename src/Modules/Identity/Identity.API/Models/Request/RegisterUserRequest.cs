using Identity.API.Enums;

namespace Identity.API.Models.Request;

public sealed record RegisterUserRequest(string Email, string UserName, string Password, EUserRole Role);