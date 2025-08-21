using System.Net;
using System.Text.Json;
using PayrollApi.Common;

namespace PayrollApi.Controllers;

public class ErrorHandlingMiddleware
{
	private readonly RequestDelegate _next;
	public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

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
			context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			context.Response.ContentType = "application/json";
			var payload = ApiResponse<object>.Fail("服务器异常");
			await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
		}
	}
}

