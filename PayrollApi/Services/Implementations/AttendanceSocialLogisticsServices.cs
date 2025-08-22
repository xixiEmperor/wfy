using Microsoft.EntityFrameworkCore;
using PayrollApi.Common;
using PayrollApi.Data;
using PayrollApi.Domain.Entities;
using PayrollApi.Interceptors;

namespace PayrollApi.Services;

public class AttendanceService : IAttendanceService
{
	private readonly AppDbContext _db;
	private readonly ICacheVersionProvider _version;
	public AttendanceService(AppDbContext db, ICacheVersionProvider version) { _db = db; _version = version; }

	[Cache(60)]
	public async Task<PagedResult<Attendance>> GetListAsync(PagedRequest req, int? employeeId, CancellationToken ct)
	{
		var q = _db.Attendances.AsNoTracking().Where(x => !x.IsDeleted);
		if (employeeId.HasValue) q = q.Where(x => x.EmployeeId == employeeId);
		if (req.DateFrom.HasValue) q = q.Where(x => x.WorkDate >= req.DateFrom);
		if (req.DateTo.HasValue) q = q.Where(x => x.WorkDate <= req.DateTo);
		return await q.Include(x => x.Employee).ToPagedResultAsync(req, ct);
	}

	public async Task<Attendance> CreateAsync(Attendance entity, CancellationToken ct)
	{
		_db.Attendances.Add(entity);
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(AttendanceService));
		return entity;
	}

	public async Task<Attendance> UpdateAsync(int id, Attendance entity, CancellationToken ct)
	{
		var dbEntity = await _db.Attendances.FirstOrDefaultAsync(x => x.AttendanceId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Attendance not found", 404);
		dbEntity.WorkDate = entity.WorkDate;
		dbEntity.HoursWorked = entity.HoursWorked;
		dbEntity.OvertimeHours = entity.OvertimeHours;
		dbEntity.AbsentHours = entity.AbsentHours;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(AttendanceService));
		return dbEntity;
	}

	public async Task DeleteAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.Attendances.FirstOrDefaultAsync(x => x.AttendanceId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Attendance not found", 404);
		dbEntity.IsDeleted = true;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(AttendanceService));
	}
}

public class LogisticsService : ILogisticsService
{
	private readonly AppDbContext _db;
	private readonly ICacheVersionProvider _version;
	public LogisticsService(AppDbContext db, ICacheVersionProvider version) { _db = db; _version = version; }

	[Cache(60)]
	public async Task<PagedResult<LogisticsData>> GetListAsync(PagedRequest req, int? employeeId, string? month, CancellationToken ct)
	{
		var q = _db.LogisticsData.AsNoTracking().Where(x => !x.IsDeleted);
		if (employeeId.HasValue) q = q.Where(x => x.EmployeeId == employeeId);
		if (!string.IsNullOrWhiteSpace(month)) q = q.Where(x => x.Month == month);
		return await q.Include(x => x.Employee).ToPagedResultAsync(req, ct);
	}

	public async Task<LogisticsData> CreateAsync(LogisticsData entity, CancellationToken ct)
	{
		_db.LogisticsData.Add(entity);
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(LogisticsService));
		return entity;
	}

	public async Task<LogisticsData> UpdateAsync(int id, LogisticsData entity, CancellationToken ct)
	{
		var dbEntity = await _db.LogisticsData.FirstOrDefaultAsync(x => x.LogisticsDataId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Logistics not found", 404);
		dbEntity.Month = entity.Month;
		dbEntity.HousingDeduction = entity.HousingDeduction;
		dbEntity.MealAllowance = entity.MealAllowance;
		dbEntity.UtilitiesDeduction = entity.UtilitiesDeduction;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(LogisticsService));
		return dbEntity;
	}

	public async Task DeleteAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.LogisticsData.FirstOrDefaultAsync(x => x.LogisticsDataId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Logistics not found", 404);
		dbEntity.IsDeleted = true;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(LogisticsService));
	}
}

public class SocialSecurityService : ISocialSecurityService
{
	private readonly AppDbContext _db;
	private readonly ICacheVersionProvider _version;
	public SocialSecurityService(AppDbContext db, ICacheVersionProvider version) { _db = db; _version = version; }

	[Cache(60)]
	public async Task<PagedResult<SocialSecurity>> GetListAsync(PagedRequest req, int? employeeId, string? month, CancellationToken ct)
	{
		var q = _db.SocialSecurities.AsNoTracking().Where(x => !x.IsDeleted);
		if (employeeId.HasValue) q = q.Where(x => x.EmployeeId == employeeId);
		if (!string.IsNullOrWhiteSpace(month)) q = q.Where(x => x.Month == month);
		return await q.Include(x => x.Employee).ToPagedResultAsync(req, ct);
	}

	public async Task<SocialSecurity> CreateAsync(SocialSecurity entity, CancellationToken ct)
	{
		_db.SocialSecurities.Add(entity);
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(SocialSecurityService));
		return entity;
	}

	public async Task<SocialSecurity> UpdateAsync(int id, SocialSecurity entity, CancellationToken ct)
	{
		var dbEntity = await _db.SocialSecurities.FirstOrDefaultAsync(x => x.SocialSecurityId == id && !x.IsDeleted, ct) ?? throw new BusinessException("SocialSecurity not found", 404);
		dbEntity.Month = entity.Month;
		dbEntity.Pension = entity.Pension;
		dbEntity.Medical = entity.Medical;
		dbEntity.Unemployment = entity.Unemployment;
		dbEntity.HousingFund = entity.HousingFund;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(SocialSecurityService));
		return dbEntity;
	}

	public async Task DeleteAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.SocialSecurities.FirstOrDefaultAsync(x => x.SocialSecurityId == id && !x.IsDeleted, ct) ?? throw new BusinessException("SocialSecurity not found", 404);
		dbEntity.IsDeleted = true;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(SocialSecurityService));
	}
}


