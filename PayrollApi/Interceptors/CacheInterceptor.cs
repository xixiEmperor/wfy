using System.Text.Json;
using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace PayrollApi.Interceptors;

public class CacheInterceptor : IInterceptor
{
	private readonly IMemoryCache _cache;
    private readonly ICacheVersionProvider _versionProvider;
	public static readonly string CachePrefix = "svc:";

	public CacheInterceptor(IMemoryCache cache, ICacheVersionProvider versionProvider)
	{
		_cache = cache;
		_versionProvider = versionProvider;
	}

	public void Intercept(IInvocation invocation)
	{
		var cacheAttr = (invocation.MethodInvocationTarget ?? invocation.Method)
			.GetCustomAttributes(typeof(CacheAttribute), true)
			.FirstOrDefault() as CacheAttribute;
		if (cacheAttr == null)
		{
			invocation.Proceed();
			return;
		}

		var key = BuildKey(invocation, _versionProvider);
		if (_cache.TryGetValue(key, out object? value))
		{
			Log.Debug("[CACHE] hit {Key}", key);
			invocation.ReturnValue = value;
			return;
		}

		invocation.Proceed();
		if (invocation.Method.ReturnType == typeof(Task) || (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
		{
			var task = (Task)invocation.ReturnValue!;
			task.GetAwaiter().GetResult();
			object? result = GetTaskResult(task);
			_cache.Set(key, invocation.ReturnValue, TimeSpan.FromSeconds(cacheAttr.TtlSeconds));
			Log.Debug("[CACHE] set {Key}", key);
		}
		else
		{
			_cache.Set(key, invocation.ReturnValue, TimeSpan.FromSeconds(cacheAttr.TtlSeconds));
			Log.Debug("[CACHE] set {Key}", key);
		}
	}

	public static string BuildKey(IInvocation invocation, ICacheVersionProvider versionProvider)
	{
		var method = $"{invocation.TargetType.FullName}.{invocation.Method.Name}";
		var args = JsonSerializer.Serialize(invocation.Arguments);
		var ver = versionProvider.GetVersion(invocation.TargetType);
		return CachePrefix + method + ":v" + ver + ":" + args;
	}

	private static object? GetTaskResult(Task task)
	{
		var t = task.GetType();
		if (t.IsGenericType)
		{
			return t.GetProperty("Result")?.GetValue(task);
		}
		return null;
	}
}

public static class CacheInvalidator
{
	public static void InvalidateByPrefix(IMemoryCache cache, string prefix)
	{
		// IMemoryCache 无法遍历键，这里提供一个策略接口；实际调用处用带版本号的前缀实现
	}
}

