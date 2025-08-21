using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace PayrollApi.Auth;

public interface IJwtService
{
	(string accessToken, DateTime expiresAt) GenerateToken(string userId, string userName, string[] roles, int? expireMinutes = null);
}

public class JwtService : IJwtService
{
	private readonly IConfiguration _configuration;
	public JwtService(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public (string accessToken, DateTime expiresAt) GenerateToken(string userId, string userName, string[] roles, int? expireMinutes = null)
	{
		var jwtSection = _configuration.GetSection("Jwt");
		var secret = jwtSection.GetValue<string>("Secret")!;
		var issuer = jwtSection.GetValue<string>("Issuer")!;
		var audience = jwtSection.GetValue<string>("Audience")!;
		var expire = TimeSpan.FromMinutes(expireMinutes ?? jwtSection.GetValue<int>("ExpireMinutes"));

		var claims = new List<Claim>
		{
			new Claim(JwtRegisteredClaimNames.Sub, userId),
			new Claim(JwtRegisteredClaimNames.UniqueName, userName),
			new Claim(ClaimTypes.NameIdentifier, userId),
			new Claim(ClaimTypes.Name, userName)
		};
		claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
		var now = DateTime.UtcNow;
		var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			claims: claims,
			notBefore: now,
			expires: now.Add(expire),
			signingCredentials: creds
		);
		var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
		return (accessToken, token.ValidTo);
	}
}

