using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollApi.Common;
using PayrollApi.Domain.Entities;
using PayrollApi.Interceptors;
using PayrollApi.Services;

namespace PayrollApi.Controllers;

[Route("api/[controller]")]
public class DepartmentsController : BaseController
{
	private readonly IDepartmentService _svc;
	public DepartmentsController(IDepartmentService svc) { _svc = svc; }

	[HttpGet]
	[Authorize]
	[ProducesResponseType(typeof(ApiResponse<PagedResult<Department>>), 200)]
	public async Task<ActionResult<ApiResponse<PagedResult<Department>>>> Get([FromQuery] PagedRequest req, CancellationToken ct)
		=> OkResponse(await _svc.GetListAsync(req, ct));

	[HttpPost]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Department>>> Post([FromBody] Department entity, CancellationToken ct)
		=> OkResponse(await _svc.CreateAsync(entity, ct));

	[HttpPut("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Department>>> Put(int id, [FromBody] Department entity, CancellationToken ct)
		=> OkResponse(await _svc.UpdateAsync(id, entity, ct));

	[HttpDelete("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin")]
	public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct)
	{
		await _svc.DeleteAsync(id, ct);
		return OkResponse("OK");
	}
}

[Route("api/[controller]")]
public class WorkshopsController : BaseController
{
	private readonly IWorkshopService _svc;
	public WorkshopsController(IWorkshopService svc) { _svc = svc; }

	[HttpGet]
	[Authorize]
	public async Task<ActionResult<ApiResponse<PagedResult<Workshop>>>> Get([FromQuery] PagedRequest req, CancellationToken ct)
		=> OkResponse(await _svc.GetListAsync(req, ct));

	[HttpPost]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Workshop>>> Post([FromBody] Workshop entity, CancellationToken ct)
		=> OkResponse(await _svc.CreateAsync(entity, ct));

	[HttpPut("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Workshop>>> Put(int id, [FromBody] Workshop entity, CancellationToken ct)
		=> OkResponse(await _svc.UpdateAsync(id, entity, ct));

	[HttpDelete("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin")]
	public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct)
	{
		await _svc.DeleteAsync(id, ct);
		return OkResponse("OK");
	}
}

[Route("api/[controller]")]
public class EmployeesController : BaseController
{
	private readonly IEmployeeService _svc;
	public EmployeesController(IEmployeeService svc) { _svc = svc; }

	[HttpGet]
	[Authorize]
	public async Task<ActionResult<ApiResponse<PagedResult<Employee>>>> Get([FromQuery] PagedRequest req, CancellationToken ct)
		=> OkResponse(await _svc.GetListAsync(req, ct));

	[HttpPost]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Employee>>> Post([FromBody] Employee entity, CancellationToken ct)
		=> OkResponse(await _svc.CreateAsync(entity, ct));

	[HttpPut("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Employee>>> Put(int id, [FromBody] Employee entity, CancellationToken ct)
		=> OkResponse(await _svc.UpdateAsync(id, entity, ct));

	[HttpDelete("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin")]
	public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct)
	{
		await _svc.DeleteAsync(id, ct);
		return OkResponse("OK");
	}
}

[Route("api/[controller]")]
public class AttendanceController : BaseController
{
	private readonly IAttendanceService _svc;
	public AttendanceController(IAttendanceService svc) { _svc = svc; }

	[HttpGet]
	[Authorize]
	public async Task<ActionResult<ApiResponse<PagedResult<Attendance>>>> Get([FromQuery] PagedRequest req, [FromQuery] int? employeeId, CancellationToken ct)
		=> OkResponse(await _svc.GetListAsync(req, employeeId, ct));

	[HttpPost]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Attendance>>> Post([FromBody] Attendance entity, CancellationToken ct)
		=> OkResponse(await _svc.CreateAsync(entity, ct));

	[HttpPut("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Attendance>>> Put(int id, [FromBody] Attendance entity, CancellationToken ct)
		=> OkResponse(await _svc.UpdateAsync(id, entity, ct));

	[HttpDelete("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin")]
	public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct)
	{
		await _svc.DeleteAsync(id, ct);
		return OkResponse("OK");
	}
}

[Route("api/[controller]")]
public class LogisticsController : BaseController
{
	private readonly ILogisticsService _svc;
	public LogisticsController(ILogisticsService svc) { _svc = svc; }

	[HttpGet]
	[Authorize]
	public async Task<ActionResult<ApiResponse<PagedResult<LogisticsData>>>> Get([FromQuery] PagedRequest req, [FromQuery] int? employeeId, [FromQuery] string? month, CancellationToken ct)
		=> OkResponse(await _svc.GetListAsync(req, employeeId, month, ct));

	[HttpPost]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<LogisticsData>>> Post([FromBody] LogisticsData entity, CancellationToken ct)
		=> OkResponse(await _svc.CreateAsync(entity, ct));

	[HttpPut("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<LogisticsData>>> Put(int id, [FromBody] LogisticsData entity, CancellationToken ct)
		=> OkResponse(await _svc.UpdateAsync(id, entity, ct));

	[HttpDelete("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin")]
	public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct)
	{
		await _svc.DeleteAsync(id, ct);
		return OkResponse("OK");
	}
}

[Route("api/[controller]")]
public class SocialSecurityController : BaseController
{
	private readonly ISocialSecurityService _svc;
	public SocialSecurityController(ISocialSecurityService svc) { _svc = svc; }

	[HttpGet]
	[Authorize]
	public async Task<ActionResult<ApiResponse<PagedResult<SocialSecurity>>>> Get([FromQuery] PagedRequest req, [FromQuery] int? employeeId, [FromQuery] string? month, CancellationToken ct)
		=> OkResponse(await _svc.GetListAsync(req, employeeId, month, ct));

	[HttpPost]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<SocialSecurity>>> Post([FromBody] SocialSecurity entity, CancellationToken ct)
		=> OkResponse(await _svc.CreateAsync(entity, ct));

	[HttpPut("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<SocialSecurity>>> Put(int id, [FromBody] SocialSecurity entity, CancellationToken ct)
		=> OkResponse(await _svc.UpdateAsync(id, entity, ct));

	[HttpDelete("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin")]
	public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct)
	{
		await _svc.DeleteAsync(id, ct);
		return OkResponse("OK");
	}
}

[Route("api/[controller]")]
public class PayrollsController : BaseController
{
	private readonly IPayrollService _svc;
	private readonly IPayrollItemService _itemSvc;
	public PayrollsController(IPayrollService svc, IPayrollItemService itemSvc) { _svc = svc; _itemSvc = itemSvc; }

	[HttpGet]
	[Authorize]
	public async Task<ActionResult<ApiResponse<PagedResult<Payroll>>>> Get([FromQuery] PagedRequest req, [FromQuery] int? departmentId, [FromQuery] int? workshopId, [FromQuery] string? month, CancellationToken ct)
		=> OkResponse(await _svc.GetListAsync(req, departmentId, workshopId, month, ct));

	[HttpPost("generate")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<IReadOnlyList<Payroll>>>> Generate([FromBody] GeneratePayrollRequest req, CancellationToken ct)
		=> OkResponse(await _svc.GenerateDraftsAsync(req.Month, req.EmployeeIds, req.OvertimeFactor, ct));

	[HttpPost]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Payroll>>> Post([FromBody] Payroll entity, CancellationToken ct)
		=> OkResponse(await _svc.CreateAsync(entity, ct));

	[HttpPut("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Payroll>>> Put(int id, [FromBody] Payroll entity, CancellationToken ct)
		=> OkResponse(await _svc.UpdateAsync(id, entity, ct));

	[HttpDelete("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin")]
	public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct)
	{
		await _svc.DeleteAsync(id, ct);
		return OkResponse<object>(new object());
	}

	[HttpPost("{id:int}/confirm")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Payroll>>> Confirm(int id, CancellationToken ct)
		=> OkResponse(await _svc.ConfirmAsync(id, ct));

	[HttpPost("{id:int}/pay")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<Payroll>>> Pay(int id, CancellationToken ct)
		=> OkResponse(await _svc.PayAsync(id, ct));

	[HttpGet("{id:int}/items")]
	[Authorize]
	public async Task<ActionResult<ApiResponse<IReadOnlyList<PayrollItem>>>> Items(int id, CancellationToken ct)
		=> OkResponse(await _itemSvc.GetItemsAsync(id, ct));

	[HttpPost("{id:int}/items")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<PayrollItem>>> CreateItem(int id, [FromBody] PayrollItem item, CancellationToken ct)
	{
		item.PayrollId = id;
		return OkResponse(await _itemSvc.CreateAsync(item, ct));
	}

	[HttpPut("{id:int}/items/{itemId:int}")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<PayrollItem>>> UpdateItem(int id, int itemId, [FromBody] PayrollItem item, CancellationToken ct)
	{
		item.PayrollItemId = itemId;
		return OkResponse(await _itemSvc.UpdateAsync(itemId, item, ct));
	}

	[HttpDelete("{id:int}/items/{itemId:int}")]
	[Authorize]
	[AuthorizeRole("Admin")]
	public async Task<ActionResult<ApiResponse<string>>> DeleteItem(int id, int itemId, CancellationToken ct)
	{
		await _itemSvc.DeleteAsync(itemId, ct);
		return OkResponse("OK");
	}
}

public class GeneratePayrollRequest
{
	public string Month { get; set; } = null!;
	public int[]? EmployeeIds { get; set; }
	public decimal OvertimeFactor { get; set; } = 1.5m;
}

[Route("api/[controller]")]
public class SalaryChangesController : BaseController
{
	private readonly ISalaryChangeService _svc;
	public SalaryChangesController(ISalaryChangeService svc) { _svc = svc; }

	[HttpGet]
	[Authorize]
	public async Task<ActionResult<ApiResponse<PagedResult<SalaryChange>>>> Get([FromQuery] PagedRequest req, [FromQuery] int? employeeId, CancellationToken ct)
		=> OkResponse(await _svc.GetListAsync(req, employeeId, ct));

	[HttpPost]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<SalaryChange>>> Post([FromBody] SalaryChange entity, CancellationToken ct)
		=> OkResponse(await _svc.CreateAsync(entity, ct));

	[HttpPut("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<SalaryChange>>> Put(int id, [FromBody] SalaryChange entity, CancellationToken ct)
		=> OkResponse(await _svc.UpdateAsync(id, entity, ct));

	[HttpDelete("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin")]
	public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct)
	{
		await _svc.DeleteAsync(id, ct);
		return OkResponse<object>(new object());
	}
}

[Route("api/[controller]")]
public class YearEndBonusesController : BaseController
{
	private readonly IYearEndBonusService _svc;
	public YearEndBonusesController(IYearEndBonusService svc) { _svc = svc; }

	[HttpGet]
	[Authorize]
	public async Task<ActionResult<ApiResponse<PagedResult<YearEndBonus>>>> Get([FromQuery] PagedRequest req, [FromQuery] int? employeeId, CancellationToken ct)
		=> OkResponse(await _svc.GetListAsync(req, employeeId, ct));

	[HttpPost]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<YearEndBonus>>> Post([FromBody] YearEndBonus entity, CancellationToken ct)
		=> OkResponse(await _svc.CreateAsync(entity, ct));

	[HttpPut("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin", "HR")]
	public async Task<ActionResult<ApiResponse<YearEndBonus>>> Put(int id, [FromBody] YearEndBonus entity, CancellationToken ct)
		=> OkResponse(await _svc.UpdateAsync(id, entity, ct));

	[HttpDelete("{id:int}")]
	[Authorize]
	[AuthorizeRole("Admin")]
	public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct)
	{
		await _svc.DeleteAsync(id, ct);
		return OkResponse<object>(new object());
	}
}

[Route("api/[controller]")]
public class ReportsController : BaseController
{
	private readonly IReportService _svc;
	public ReportsController(IReportService svc) { _svc = svc; }

	[HttpGet("summary")]
	[Authorize]
	public async Task<ActionResult<ApiResponse<object>>> Summary([FromQuery] int? departmentId, [FromQuery] int? workshopId, [FromQuery] string? month, CancellationToken ct)
		=> OkResponse(await _svc.SummaryAsync(departmentId, workshopId, month, ct));

	[HttpGet("employee/{employeeId:int}")]
	[Authorize]
	public async Task<ActionResult<ApiResponse<object>>> EmployeeHistory(int employeeId, CancellationToken ct)
		=> OkResponse(await _svc.EmployeeHistoryAsync(employeeId, ct));
}


