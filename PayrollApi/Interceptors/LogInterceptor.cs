using System.Diagnostics;
using System.Text.Json;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace PayrollApi.Interceptors;

public class LogInterceptor : IInterceptor
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	public LogInterceptor(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public void Intercept(IInvocation invocation)
	{
		var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
		var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "anonymous";
		var method = $"{invocation.TargetType.Name}.{invocation.Method.Name}";
		var argsJson = SafeSerialize(invocation.Arguments);
		var sw = Stopwatch.StartNew();
		try
		{
			invocation.Proceed();
			if (invocation.Method.ReturnType == typeof(Task) || (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
			{
				var task = (Task)invocation.ReturnValue!;
				task.GetAwaiter().GetResult();
			}
			sw.Stop();
			var returnValue = invocation.ReturnValue;
			if (returnValue is Task taskRet)
			{
				var result = GetTaskResult(taskRet);
				Log.Information("[LOG] OK {CorrelationId} {User} {Method} args={Args} took={Elapsed}ms return={Return}", correlationId, userId, method, argsJson, sw.ElapsedMilliseconds, SafeSerialize(result));
			}
			else
			{
				Log.Information("[LOG] OK {CorrelationId} {User} {Method} args={Args} took={Elapsed}ms return={Return}", correlationId, userId, method, argsJson, sw.ElapsedMilliseconds, SafeSerialize(returnValue));
			}
		}
		catch (Exception ex)
		{
			sw.Stop();
			Log.Error(ex, "[LOG] ERR {CorrelationId} {User} {Method} args={Args} took={Elapsed}ms", correlationId, userId, method, argsJson, sw.ElapsedMilliseconds);
			throw;
		}
	}

	private static string SafeSerialize(object? obj)
	{
		try { return JsonSerializer.Serialize(obj); } catch { return "<non-serializable>"; }
	}

	private static object? GetTaskResult(Task task)
	{
		var taskType = task.GetType();
		if (taskType.IsGenericType)
		{
			return taskType.GetProperty("Result")?.GetValue(task);
		}
		return null;
	}
}

