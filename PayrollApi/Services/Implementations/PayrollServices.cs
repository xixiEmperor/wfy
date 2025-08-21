using Microsoft.EntityFrameworkCore;
using PayrollApi.Common;
using PayrollApi.Data;
using PayrollApi.Domain.Entities;
using PayrollApi.Interceptors;

namespace PayrollApi.Services;

public class PayrollService : IPayrollService
{
	private readonly AppDbContext _db;
	private readonly ICacheVersionProvider _version;
	public PayrollService(AppDbContext db, ICacheVersionProvider version) { _db = db; _version = version; }

	[Cache(60)]
	public async Task<PagedResult<Payroll>> GetListAsync(PagedRequest req, int? departmentId, int? workshopId, string? month, CancellationToken ct)
	{
		var q = _db.Payrolls.AsNoTracking().Where(x => !x.IsDeleted);
		if (!string.IsNullOrWhiteSpace(month)) q = q.Where(x => x.Month == month);
		if (departmentId.HasValue) q = q.Where(x => x.Employee.DepartmentId == departmentId);
		if (workshopId.HasValue) q = q.Where(x => x.Employee.WorkshopId == workshopId);
		return await q.Include(x => x.Employee).ToPagedResultAsync(req, ct);
	}

	public async Task<Payroll> CreateAsync(Payroll entity, CancellationToken ct)
	{
		_db.Payrolls.Add(entity);
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(PayrollService));
		return entity;
	}

	public async Task<Payroll> UpdateAsync(int id, Payroll entity, CancellationToken ct)
	{
		var dbEntity = await _db.Payrolls.Include(x => x.Items).FirstOrDefaultAsync(x => x.PayrollId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Payroll not found", 404);
		if (dbEntity.Status != PayrollStatus.Draft) throw new BusinessException("Only Draft can be updated");
		dbEntity.GrossAmount = entity.GrossAmount;
		dbEntity.Deductions = entity.Deductions;
		dbEntity.NetAmount = entity.NetAmount;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(PayrollService));
		return dbEntity;
	}

	public async Task DeleteAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.Payrolls.FirstOrDefaultAsync(x => x.PayrollId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Payroll not found", 404);
		if (dbEntity.Status != PayrollStatus.Draft) throw new BusinessException("Only Draft can be deleted");
		dbEntity.IsDeleted = true;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(PayrollService));
	}

	public async Task<Payroll> ConfirmAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.Payrolls.Include(x => x.Items).FirstOrDefaultAsync(x => x.PayrollId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Payroll not found", 404);
		if (dbEntity.Status != PayrollStatus.Draft) throw new BusinessException("Only Draft can be confirmed");
		dbEntity.Status = PayrollStatus.Confirmed;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(PayrollService));
		return dbEntity;
	}

	public async Task<Payroll> PayAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.Payrolls.FirstOrDefaultAsync(x => x.PayrollId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Payroll not found", 404);
		if (dbEntity.Status != PayrollStatus.Confirmed) throw new BusinessException("Only Confirmed can be paid");
		dbEntity.Status = PayrollStatus.Paid;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(PayrollService));
		return dbEntity;
	}

	public async Task<IReadOnlyList<Payroll>> GenerateDraftsAsync(string month, int[]? employeeIds, decimal overtimeFactor, CancellationToken ct)
	{
		await using var trx = await _db.Database.BeginTransactionAsync(ct);
		try
		{
			var employeesQuery = _db.Employees.Where(e => !e.IsDeleted && e.IsActive);
			if (employeeIds != null && employeeIds.Length > 0)
				employeesQuery = employeesQuery.Where(e => employeeIds.Contains(e.EmployeeId));
			var employees = await employeesQuery.ToListAsync(ct);

			var result = new List<Payroll>();
			foreach (var e in employees)
			{
				var exists = await _db.Payrolls.AnyAsync(p => p.EmployeeId == e.EmployeeId && p.Month == month && !p.IsDeleted, ct);
				if (exists) continue;

				var attendance = await _db.Attendances.Where(a => a.EmployeeId == e.EmployeeId && a.WorkDate.ToString()!.StartsWith(month) && !a.IsDeleted)
					.ToListAsync(ct);
				var overtimePay = attendance.Sum(a => a.OvertimeHours) * (e.BaseSalary / 21.75m / 8m) * overtimeFactor;
				var absentDeduction = attendance.Sum(a => a.AbsentHours) * (e.BaseSalary / 21.75m / 8m);

				var logistics = await _db.LogisticsData.FirstOrDefaultAsync(l => l.EmployeeId == e.EmployeeId && l.Month == month && !l.IsDeleted, ct);
				var social = await _db.SocialSecurities.FirstOrDefaultAsync(s => s.EmployeeId == e.EmployeeId && s.Month == month && !s.IsDeleted, ct);

				var payroll = new Payroll
				{
					EmployeeId = e.EmployeeId,
					Month = month,
					Status = PayrollStatus.Draft,
					GrossAmount = e.BaseSalary + overtimePay + (logistics?.MealAllowance ?? 0),
					Deductions = absentDeduction + (logistics?.HousingDeduction ?? 0) + (logistics?.UtilitiesDeduction ?? 0)
						+ (social?.Pension ?? 0) + (social?.Medical ?? 0) + (social?.Unemployment ?? 0) + (social?.HousingFund ?? 0),
				};
				payroll.NetAmount = payroll.GrossAmount - payroll.Deductions;
				_db.Payrolls.Add(payroll);
				await _db.SaveChangesAsync(ct);

				var items = new List<PayrollItem>
				{
					new PayrollItem{ PayrollId = payroll.PayrollId, ItemType = "Fixed", ItemName = "BaseSalary", Amount = e.BaseSalary },
					new PayrollItem{ PayrollId = payroll.PayrollId, ItemType = "Attendance", ItemName = "Overtime", Amount = Math.Round(overtimePay,2) },
					new PayrollItem{ PayrollId = payroll.PayrollId, ItemType = "Attendance", ItemName = "AbsentDeduction", Amount = Math.Round(-absentDeduction,2) },
				};
				if (logistics != null)
				{
					items.Add(new PayrollItem{ PayrollId = payroll.PayrollId, ItemType = "Logistics", ItemName = "MealAllowance", Amount = logistics.MealAllowance });
					items.Add(new PayrollItem{ PayrollId = payroll.PayrollId, ItemType = "Logistics", ItemName = "HousingDeduction", Amount = -logistics.HousingDeduction });
					items.Add(new PayrollItem{ PayrollId = payroll.PayrollId, ItemType = "Logistics", ItemName = "UtilitiesDeduction", Amount = -logistics.UtilitiesDeduction });
				}
				if (social != null)
				{
					items.Add(new PayrollItem{ PayrollId = payroll.PayrollId, ItemType = "SocialSecurity", ItemName = "Pension", Amount = -social.Pension });
					items.Add(new PayrollItem{ PayrollId = payroll.PayrollId, ItemType = "SocialSecurity", ItemName = "Medical", Amount = -social.Medical });
					items.Add(new PayrollItem{ PayrollId = payroll.PayrollId, ItemType = "SocialSecurity", ItemName = "Unemployment", Amount = -social.Unemployment });
					items.Add(new PayrollItem{ PayrollId = payroll.PayrollId, ItemType = "SocialSecurity", ItemName = "HousingFund", Amount = -social.HousingFund });
				}
				_db.PayrollItems.AddRange(items);
				await _db.SaveChangesAsync(ct);
				_version.Bump(typeof(PayrollService));
				result.Add(payroll);
			}

			await trx.CommitAsync(ct);
			return result;
		}
		catch
		{
			await trx.RollbackAsync(ct);
			throw;
		}
	}
}

public class PayrollItemService : IPayrollItemService
{
	private readonly AppDbContext _db;
	private readonly ICacheVersionProvider _version;
	public PayrollItemService(AppDbContext db, ICacheVersionProvider version) { _db = db; _version = version; }

	[Cache(60)]
	public async Task<IReadOnlyList<PayrollItem>> GetItemsAsync(int payrollId, CancellationToken ct)
	{
		return await _db.PayrollItems.AsNoTracking().Where(i => i.PayrollId == payrollId && !i.IsDeleted).ToListAsync(ct);
	}

	public async Task<PayrollItem> CreateAsync(PayrollItem entity, CancellationToken ct)
	{
		var payroll = await _db.Payrolls.FirstOrDefaultAsync(p => p.PayrollId == entity.PayrollId && !p.IsDeleted, ct) ?? throw new BusinessException("Payroll not found", 404);
		if (payroll.Status != PayrollStatus.Draft) throw new BusinessException("Items locked when not Draft");
		_db.PayrollItems.Add(entity);
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(PayrollItemService));
		return entity;
	}

	public async Task<PayrollItem> UpdateAsync(int id, PayrollItem entity, CancellationToken ct)
	{
		var dbEntity = await _db.PayrollItems.FirstOrDefaultAsync(x => x.PayrollItemId == id && !x.IsDeleted, ct) ?? throw new BusinessException("PayrollItem not found", 404);
		var payroll = await _db.Payrolls.FirstOrDefaultAsync(p => p.PayrollId == dbEntity.PayrollId && !p.IsDeleted, ct) ?? throw new BusinessException("Payroll not found", 404);
		if (payroll.Status != PayrollStatus.Draft) throw new BusinessException("Items locked when not Draft");
		dbEntity.ItemType = entity.ItemType;
		dbEntity.ItemName = entity.ItemName;
		dbEntity.Amount = entity.Amount;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(PayrollItemService));
		return dbEntity;
	}

	public async Task DeleteAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.PayrollItems.FirstOrDefaultAsync(x => x.PayrollItemId == id && !x.IsDeleted, ct) ?? throw new BusinessException("PayrollItem not found", 404);
		var payroll = await _db.Payrolls.FirstOrDefaultAsync(p => p.PayrollId == dbEntity.PayrollId && !p.IsDeleted, ct) ?? throw new BusinessException("Payroll not found", 404);
		if (payroll.Status != PayrollStatus.Draft) throw new BusinessException("Items locked when not Draft");
		dbEntity.IsDeleted = true;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(PayrollItemService));
	}
}

public class SalaryChangeService : ISalaryChangeService
{
	private readonly AppDbContext _db;
	private readonly ICacheVersionProvider _version;
	public SalaryChangeService(AppDbContext db, ICacheVersionProvider version) { _db = db; _version = version; }

	[Cache(60)]
	public async Task<PagedResult<SalaryChange>> GetListAsync(PagedRequest req, int? employeeId, CancellationToken ct)
	{
		var q = _db.SalaryChanges.AsNoTracking().Where(x => !x.IsDeleted);
		if (employeeId.HasValue) q = q.Where(x => x.EmployeeId == employeeId);
		if (req.DateFrom.HasValue) q = q.Where(x => x.ChangeDate >= req.DateFrom);
		if (req.DateTo.HasValue) q = q.Where(x => x.ChangeDate <= req.DateTo);
		return await q.ToPagedResultAsync(req, ct);
	}

	public async Task<SalaryChange> CreateAsync(SalaryChange entity, CancellationToken ct)
	{
		_db.SalaryChanges.Add(entity);
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(SalaryChangeService));
		// 同步更新员工基础工资
		var emp = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == entity.EmployeeId && !e.IsDeleted, ct);
		if (emp != null)
		{
			emp.BaseSalary = entity.NewBaseSalary;
			emp.UpdatedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync(ct);
			_version.Bump(typeof(EmployeeService));
		}
		return entity;
	}

	public async Task<SalaryChange> UpdateAsync(int id, SalaryChange entity, CancellationToken ct)
	{
		var dbEntity = await _db.SalaryChanges.FirstOrDefaultAsync(x => x.SalaryChangeId == id && !x.IsDeleted, ct) ?? throw new BusinessException("SalaryChange not found", 404);
		dbEntity.ChangeDate = entity.ChangeDate;
		dbEntity.OldBaseSalary = entity.OldBaseSalary;
		dbEntity.NewBaseSalary = entity.NewBaseSalary;
		dbEntity.Reason = entity.Reason;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(SalaryChangeService));
		return dbEntity;
	}

	public async Task DeleteAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.SalaryChanges.FirstOrDefaultAsync(x => x.SalaryChangeId == id && !x.IsDeleted, ct) ?? throw new BusinessException("SalaryChange not found", 404);
		dbEntity.IsDeleted = true;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(SalaryChangeService));
	}
}

public class YearEndBonusService : IYearEndBonusService
{
	private readonly AppDbContext _db;
	private readonly ICacheVersionProvider _version;
	public YearEndBonusService(AppDbContext db, ICacheVersionProvider version) { _db = db; _version = version; }

	[Cache(60)]
	public async Task<PagedResult<YearEndBonus>> GetListAsync(PagedRequest req, int? employeeId, CancellationToken ct)
	{
		var q = _db.YearEndBonuses.AsNoTracking().Where(x => !x.IsDeleted);
		if (employeeId.HasValue) q = q.Where(x => x.EmployeeId == employeeId);
		return await q.ToPagedResultAsync(req, ct);
	}

	public async Task<YearEndBonus> CreateAsync(YearEndBonus entity, CancellationToken ct)
	{
		_db.YearEndBonuses.Add(entity);
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(YearEndBonusService));
		return entity;
	}

	public async Task<YearEndBonus> UpdateAsync(int id, YearEndBonus entity, CancellationToken ct)
	{
		var dbEntity = await _db.YearEndBonuses.FirstOrDefaultAsync(x => x.YearEndBonusId == id && !x.IsDeleted, ct) ?? throw new BusinessException("YearEndBonus not found", 404);
		dbEntity.Year = entity.Year;
		dbEntity.Amount = entity.Amount;
		dbEntity.Remark = entity.Remark;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(YearEndBonusService));
		return dbEntity;
	}

	public async Task DeleteAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.YearEndBonuses.FirstOrDefaultAsync(x => x.YearEndBonusId == id && !x.IsDeleted, ct) ?? throw new BusinessException("YearEndBonus not found", 404);
		dbEntity.IsDeleted = true;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(YearEndBonusService));
	}
}

public class ReportService : IReportService
{
	private readonly AppDbContext _db;
	public ReportService(AppDbContext db) { _db = db; }

	[Cache(300)]
	public async Task<object> SummaryAsync(int? departmentId, int? workshopId, string? month, CancellationToken ct)
	{
		var q = _db.Payrolls.AsNoTracking().Where(p => !p.IsDeleted);
		if (!string.IsNullOrWhiteSpace(month)) q = q.Where(p => p.Month == month);
		if (departmentId.HasValue) q = q.Where(p => p.Employee.DepartmentId == departmentId);
		if (workshopId.HasValue) q = q.Where(p => p.Employee.WorkshopId == workshopId);
		var data = await q.GroupBy(p => new { p.Month, p.Employee.DepartmentId, p.Employee.WorkshopId })
			.Select(g => new {
				g.Key.Month,
				DepartmentId = g.Key.DepartmentId,
				WorkshopId = g.Key.WorkshopId,
				Count = g.Count(),
				Gross = g.Sum(x => x.GrossAmount),
				Deductions = g.Sum(x => x.Deductions),
				Net = g.Sum(x => x.NetAmount)
			})
			.ToListAsync(ct);
		return data;
	}

	[Cache(300)]
	public async Task<object> EmployeeHistoryAsync(int employeeId, CancellationToken ct)
	{
		var payrolls = await _db.Payrolls.AsNoTracking().Where(p => p.EmployeeId == employeeId && !p.IsDeleted).OrderBy(p => p.Month).ToListAsync(ct);
		var bonuses = await _db.YearEndBonuses.AsNoTracking().Where(y => y.EmployeeId == employeeId && !y.IsDeleted).OrderBy(y => y.Year).ToListAsync(ct);
		return new { payrolls, bonuses };
	}
}


