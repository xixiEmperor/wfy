using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace PayrollApi.Common;

public class ApiResponse<T>
{
	[JsonPropertyName("success")] public bool Success { get; set; }
	[JsonPropertyName("message")] public string Message { get; set; } = "OK";
	[JsonPropertyName("data")] public T? Data { get; set; }
	[JsonPropertyName("meta")] public PaginationMeta? Meta { get; set; }

	public static ApiResponse<T> Ok(T? data, PaginationMeta? meta = null, string message = "OK")
	{
		return new ApiResponse<T> { Success = true, Message = message, Data = data, Meta = meta };
	}

	public static ApiResponse<T> Fail(string message)
	{
		return new ApiResponse<T> { Success = false, Message = message };
	}
}

public class PaginationMeta
{
	[JsonPropertyName("page")] public int Page { get; set; }
	[JsonPropertyName("pageSize")] public int PageSize { get; set; }
	[JsonPropertyName("total")] public long Total { get; set; }
	[JsonPropertyName("sortBy")] public string? SortBy { get; set; }
	[JsonPropertyName("sortDir")] public string? SortDir { get; set; }
}

public class PagedResult<T>
{
	public required IReadOnlyList<T> Items { get; init; }
	public required PaginationMeta Meta { get; init; }
}

public class PagedRequest
{
	public int Page { get; set; } = 1;
	public int PageSize { get; set; } = 20;
	public string? SortBy { get; set; }
	public string? SortDir { get; set; } = "desc";
	public string? Keyword { get; set; }
	public int? DepartmentId { get; set; }
	public int? WorkshopId { get; set; }
	public string? Month { get; set; }
	public DateOnly? DateFrom { get; set; }
	public DateOnly? DateTo { get; set; }
}

public static class QueryablePagingExtensions
{
	public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, PagedRequest req, CancellationToken ct = default)
	{
		var total = await query.LongCountAsync(ct);
		if (!string.IsNullOrWhiteSpace(req.SortBy))
		{
			query = ApplySort(query, req.SortBy!, req.SortDir);
		}
		var skip = Math.Max(0, (req.Page - 1) * req.PageSize);
		var items = await query.Skip(skip).Take(req.PageSize).ToListAsync(ct);
		return new PagedResult<T>
		{
			Items = items,
			Meta = new PaginationMeta
			{
				Page = req.Page,
				PageSize = req.PageSize,
				Total = total,
				SortBy = req.SortBy,
				SortDir = req.SortDir
			}
		};
	}

	private static IQueryable<T> ApplySort<T>(IQueryable<T> source, string sortBy, string? sortDir)
	{
		var param = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
		var property = System.Linq.Expressions.Expression.PropertyOrField(param, sortBy);
		var lambda = System.Linq.Expressions.Expression.Lambda(property, param);
		var method = (sortDir?.Equals("asc", StringComparison.OrdinalIgnoreCase) ?? false) ? "OrderBy" : "OrderByDescending";
		var call = System.Linq.Expressions.Expression.Call(typeof(Queryable), method, new[] { typeof(T), property.Type }, source.Expression, System.Linq.Expressions.Expression.Quote(lambda));
		return source.Provider.CreateQuery<T>(call);
	}
}

public class BusinessException : Exception
{
	public int StatusCode { get; }
	public BusinessException(string message, int statusCode = 400) : base(message)
	{
		StatusCode = statusCode;
	}
}

