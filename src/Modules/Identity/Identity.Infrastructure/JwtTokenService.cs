using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Infrastructure;

public class JwtTokenService
{
	private readonly IConfiguration _configuration;

	public JwtTokenService(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public string GenerateToken(IdentityUser user, IList<string> roles, int expireTimeInMinutes)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id),
			new(ClaimTypes.Name, user.UserName!),
			new(ClaimTypes.Email, user.Email!)
		};

		foreach (var role in roles)
			claims.Add(new Claim(ClaimTypes.Role, role));

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _configuration["Jwt:Issuer"],
			audience: _configuration["Jwt:Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(expireTimeInMinutes),
			signingCredentials: creds);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public async Task<(bool isValid, string? userName)> ValidateTokenAsync(string token)
	{
		if (string.IsNullOrWhiteSpace(token))
			return (false, null);

		var tokenValidationParams = GetTokenValidationParameters(_configuration);
		var validTokenResult = await new JwtSecurityTokenHandler().ValidateTokenAsync(token, tokenValidationParams);

		if (!validTokenResult.IsValid)
			return (false, null);

		var userName = validTokenResult.Claims
		  .FirstOrDefault(x => x.Key == ClaimTypes.Name).Value as string;

		return (true, userName);
	}

	public static TokenValidationParameters GetTokenValidationParameters(IConfiguration configuration)
	{
		var jwtSettings = configuration.GetSection("Jwt");
		byte[] key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

		return new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtSettings["Issuer"],
			ValidAudience = jwtSettings["Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(key),
			ClockSkew = TimeSpan.FromMinutes(5)
		};
	}
}
