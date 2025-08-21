using System.Security.Claims;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using PayrollApi.Common;

namespace PayrollApi.Interceptors;

public class AuthorizeRoleInterceptor : IInterceptor
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	public AuthorizeRoleInterceptor(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public void Intercept(IInvocation invocation)
	{
		var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;
		var attr = (AuthorizeRoleAttribute?)methodInfo
			.GetCustomAttributes(typeof(AuthorizeRoleAttribute), true)
			.FirstOrDefault() ?? (AuthorizeRoleAttribute?)invocation.TargetType
			.GetCustomAttributes(typeof(AuthorizeRoleAttribute), true)
			.FirstOrDefault();
		if (attr == null)
		{
			invocation.Proceed();
			return;
		}

		var user = _httpContextAccessor.HttpContext?.User;
		if (user == null || !user.Identity?.IsAuthenticated == true)
		{
			throw new BusinessException("Unauthorized", 401);
		}
		var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
		if (!attr.Roles.Any(r => roles.Contains(r)))
		{
			throw new BusinessException("Forbidden: role not allowed", 403);
		}

		invocation.Proceed();
	}
}

