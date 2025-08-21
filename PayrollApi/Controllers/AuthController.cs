using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PayrollApi.Auth;
using PayrollApi.Common;

namespace PayrollApi.Controllers;

/// <summary>
/// 认证登录（返回 JWT）
/// </summary>
[Route("api/[controller]")]
public class AuthController : BaseController
{
	private readonly IJwtService _jwt;
	public AuthController(IJwtService jwt) { _jwt = jwt; }

	/// <summary>
/// 账号密码登录，返回 accessToken 与过期时间
/// </summary>
	[HttpPost("login")]
	[ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
	public ActionResult<ApiResponse<LoginResponse>> Login([FromBody] LoginRequest req)
	{
		// 演示用途：固定账户
		if ((req.UserName == "admin" && req.Password == "admin123") || (req.UserName == "hr" && req.Password == "hr123") || (req.UserName == "user" && req.Password == "user123"))
		{
			var roles = req.UserName switch { "admin" => new[] { "Admin" }, "hr" => new[] { "HR" }, _ => new[] { "User" } };
			var (token, expires) = _jwt.GenerateToken(req.UserName, req.UserName, roles);
			return OkResponse(new LoginResponse { AccessToken = token, ExpiresAt = expires });
		}
		return FailResponse<LoginResponse>("用户名或密码错误", 401);
	}
}

public class LoginRequest
{
	[Required]
	public string UserName { get; set; } = null!;
	[Required]
	public string Password { get; set; } = null!;
}

public class LoginResponse
{
	public string AccessToken { get; set; } = string.Empty;
	public DateTime ExpiresAt { get; set; }
}

