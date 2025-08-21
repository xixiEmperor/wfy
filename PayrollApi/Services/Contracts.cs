using PayrollApi.Common;
using PayrollApi.Domain.Entities;
using PayrollApi.Interceptors;

namespace PayrollApi.Services;

public interface IDepartmentService
{
	[Cache(60)]
	Task<PagedResult<Department>> GetListAsync(PagedRequest req, CancellationToken ct);
	[Log]
	Task<Department> CreateAsync(Department entity, CancellationToken ct);
	[Log]
	Task<Department> UpdateAsync(int id, Department entity, CancellationToken ct);
	[Log]
	Task DeleteAsync(int id, CancellationToken ct);
}

public interface IWorkshopService
{
	[Cache(60)]
	Task<PagedResult<Workshop>> GetListAsync(PagedRequest req, CancellationToken ct);
	[Log]
	Task<Workshop> CreateAsync(Workshop entity, CancellationToken ct);
	[Log]
	Task<Workshop> UpdateAsync(int id, Workshop entity, CancellationToken ct);
	[Log]
	Task DeleteAsync(int id, CancellationToken ct);
}

public interface IEmployeeService
{
	[Cache(60)]
	Task<PagedResult<Employee>> GetListAsync(PagedRequest req, CancellationToken ct);
	[Log]
	Task<Employee> CreateAsync(Employee entity, CancellationToken ct);
	[Log]
	Task<Employee> UpdateAsync(int id, Employee entity, CancellationToken ct);
	[Log]
	Task DeleteAsync(int id, CancellationToken ct);
}

public interface IAttendanceService
{
	[Cache(60)]
	Task<PagedResult<Attendance>> GetListAsync(PagedRequest req, int? employeeId, CancellationToken ct);
	[Log]
	Task<Attendance> CreateAsync(Attendance entity, CancellationToken ct);
	[Log]
	Task<Attendance> UpdateAsync(int id, Attendance entity, CancellationToken ct);
	[Log]
	Task DeleteAsync(int id, CancellationToken ct);
}

public interface ILogisticsService
{
	[Cache(60)]
	Task<PagedResult<LogisticsData>> GetListAsync(PagedRequest req, int? employeeId, string? month, CancellationToken ct);
	[Log]
	Task<LogisticsData> CreateAsync(LogisticsData entity, CancellationToken ct);
	[Log]
	Task<LogisticsData> UpdateAsync(int id, LogisticsData entity, CancellationToken ct);
	[Log]
	Task DeleteAsync(int id, CancellationToken ct);
}

public interface ISocialSecurityService
{
	[Cache(60)]
	Task<PagedResult<SocialSecurity>> GetListAsync(PagedRequest req, int? employeeId, string? month, CancellationToken ct);
	[Log]
	Task<SocialSecurity> CreateAsync(SocialSecurity entity, CancellationToken ct);
	[Log]
	Task<SocialSecurity> UpdateAsync(int id, SocialSecurity entity, CancellationToken ct);
	[Log]
	Task DeleteAsync(int id, CancellationToken ct);
}

public interface IPayrollService
{
	[Cache(60)]
	Task<PagedResult<Payroll>> GetListAsync(PagedRequest req, int? departmentId, int? workshopId, string? month, CancellationToken ct);
	[Log]
	Task<Payroll> CreateAsync(Payroll entity, CancellationToken ct);
	[Log]
	Task<Payroll> UpdateAsync(int id, Payroll entity, CancellationToken ct);
	[Log]
	Task DeleteAsync(int id, CancellationToken ct);
	[Log]
	Task<Payroll> ConfirmAsync(int id, CancellationToken ct);
	[Log]
	Task<Payroll> PayAsync(int id, CancellationToken ct);
	[Log]
	Task<IReadOnlyList<Payroll>> GenerateDraftsAsync(string month, int[]? employeeIds, decimal overtimeFactor, CancellationToken ct);
}

public interface IPayrollItemService
{
	[Cache(60)]
	Task<IReadOnlyList<PayrollItem>> GetItemsAsync(int payrollId, CancellationToken ct);
	[Log]
	Task<PayrollItem> CreateAsync(PayrollItem entity, CancellationToken ct);
	[Log]
	Task<PayrollItem> UpdateAsync(int id, PayrollItem entity, CancellationToken ct);
	[Log]
	Task DeleteAsync(int id, CancellationToken ct);
}

public interface ISalaryChangeService
{
	[Cache(60)]
	Task<PagedResult<SalaryChange>> GetListAsync(PagedRequest req, int? employeeId, CancellationToken ct);
	[Log]
	Task<SalaryChange> CreateAsync(SalaryChange entity, CancellationToken ct);
	[Log]
	Task<SalaryChange> UpdateAsync(int id, SalaryChange entity, CancellationToken ct);
	[Log]
	Task DeleteAsync(int id, CancellationToken ct);
}

public interface IYearEndBonusService
{
	[Cache(60)]
	Task<PagedResult<YearEndBonus>> GetListAsync(PagedRequest req, int? employeeId, CancellationToken ct);
	[Log]
	Task<YearEndBonus> CreateAsync(YearEndBonus entity, CancellationToken ct);
	[Log]
	Task<YearEndBonus> UpdateAsync(int id, YearEndBonus entity, CancellationToken ct);
	[Log]
	Task DeleteAsync(int id, CancellationToken ct);
}

public interface IReportService
{
	[Cache(300)]
	Task<object> SummaryAsync(int? departmentId, int? workshopId, string? month, CancellationToken ct);
	[Cache(300)]
	Task<object> EmployeeHistoryAsync(int employeeId, CancellationToken ct);
}

