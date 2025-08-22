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
		var method = $"{invocation.TargetType?.FullName}.{invocation.Method.Name}";
		
		// 过滤掉不可序列化的参数（如CancellationToken）
		var serializableArgs = invocation.Arguments
			.Where(arg => arg != null && IsSerializable(arg))
			.ToArray();
		
		var args = JsonSerializer.Serialize(serializableArgs);
		var ver = versionProvider.GetVersion(invocation.TargetType!);
		return CachePrefix + method + ":v" + ver + ":" + args;
	}
	
	private static bool IsSerializable(object obj)
	{
		var type = obj.GetType();
		
		// 排除已知的不可序列化类型
		if (type == typeof(CancellationToken) || 
		    type == typeof(CancellationTokenSource) ||
		    type.Name.Contains("Token"))
		{
			return false;
		}
		
		// 尝试序列化以验证
		try
		{
			JsonSerializer.Serialize(obj);
			return true;
		}
		catch
		{
			return false;
		}
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

