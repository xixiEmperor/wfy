using System.Net;
using System.Text.Json;
using PayrollApi.Common;

namespace PayrollApi.Controllers;

public class ErrorHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ErrorHandlingMiddleware> _logger;
	
	public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task Invoke(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (BusinessException bex)
		{
			context.Response.StatusCode = bex.StatusCode;
			context.Response.ContentType = "application/json";
			var payload = ApiResponse<object>.Fail(bex.Message);
			await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "未处理的异常: {Message}", ex.Message);
			context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			context.Response.ContentType = "application/json";
			var payload = ApiResponse<object>.Fail("服务器异常");
			await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
		}
	}
}

