using Microsoft.AspNetCore.Mvc;
using PayrollApi.Common;

namespace PayrollApi.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
	protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, PaginationMeta? meta = null)
		=> Ok(ApiResponse<T>.Ok(data, meta));

	protected ActionResult<ApiResponse<T>> FailResponse<T>(string message, int statusCode = 400)
		=> StatusCode(statusCode, ApiResponse<T>.Fail(message));
}

