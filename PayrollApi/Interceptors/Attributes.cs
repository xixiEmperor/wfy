using System;

namespace PayrollApi.Interceptors;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class LogAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class CacheAttribute : Attribute
{
	public int TtlSeconds { get; }
	public CacheAttribute(int ttlSeconds)
	{
		TtlSeconds = ttlSeconds;
	}
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRoleAttribute : Attribute
{
	public string[] Roles { get; }
	public AuthorizeRoleAttribute(params string[] roles)
	{
		Roles = roles;
	}
}

