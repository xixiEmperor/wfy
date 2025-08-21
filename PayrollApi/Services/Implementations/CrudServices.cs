using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayrollApi.Common;
using PayrollApi.Data;
using PayrollApi.Domain.Entities;
using PayrollApi.Interceptors;

namespace PayrollApi.Services;

public class DepartmentService : IDepartmentService
{
	private readonly AppDbContext _db;
	private readonly IMemoryCache _cache;
	private readonly ICacheVersionProvider _version;
	public DepartmentService(AppDbContext db, IMemoryCache cache, ICacheVersionProvider version)
	{
		_db = db; _cache = cache; _version = version;
	}

	[Cache(60)]
	public async Task<PagedResult<Department>> GetListAsync(PagedRequest req, CancellationToken ct)
	{
		var query = _db.Departments.AsNoTracking().Where(x => !x.IsDeleted);
		if (!string.IsNullOrWhiteSpace(req.Keyword))
			query = query.Where(x => x.Name.Contains(req.Keyword!));
		return await query.ToPagedResultAsync(req, ct);
	}

	public async Task<Department> CreateAsync(Department entity, CancellationToken ct)
	{
		if (await _db.Departments.AnyAsync(x => x.Name == entity.Name && !x.IsDeleted, ct))
			throw new BusinessException("Department name already exists");
		_db.Departments.Add(entity);
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(DepartmentService));
		return entity;
	}

	public async Task<Department> UpdateAsync(int id, Department entity, CancellationToken ct)
	{
		var dbEntity = await _db.Departments.FirstOrDefaultAsync(x => x.DepartmentId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Department not found", 404);
		dbEntity.Name = entity.Name;
		dbEntity.Description = entity.Description;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(DepartmentService));
		return dbEntity;
	}

	public async Task DeleteAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.Departments.FirstOrDefaultAsync(x => x.DepartmentId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Department not found", 404);
		// 业务关键表禁用级联删除，改为软删
		dbEntity.IsDeleted = true;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(DepartmentService));
	}
}

public class WorkshopService : IWorkshopService
{
	private readonly AppDbContext _db;
	private readonly ICacheVersionProvider _version;
	public WorkshopService(AppDbContext db, ICacheVersionProvider version) { _db = db; _version = version; }

	[Cache(60)]
	public async Task<PagedResult<Workshop>> GetListAsync(PagedRequest req, CancellationToken ct)
	{
		var q = _db.Workshops.AsNoTracking().Where(x => !x.IsDeleted);
		if (req.DepartmentId.HasValue) q = q.Where(x => x.DepartmentId == req.DepartmentId);
		if (!string.IsNullOrWhiteSpace(req.Keyword)) q = q.Where(x => x.Name.Contains(req.Keyword!));
		return await q.ToPagedResultAsync(req, ct);
	}

	public async Task<Workshop> CreateAsync(Workshop entity, CancellationToken ct)
	{
		_db.Workshops.Add(entity);
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(WorkshopService));
		return entity;
	}

	public async Task<Workshop> UpdateAsync(int id, Workshop entity, CancellationToken ct)
	{
		var dbEntity = await _db.Workshops.FirstOrDefaultAsync(x => x.WorkshopId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Workshop not found", 404);
		dbEntity.Name = entity.Name;
		dbEntity.DepartmentId = entity.DepartmentId;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(WorkshopService));
		return dbEntity;
	}

	public async Task DeleteAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.Workshops.FirstOrDefaultAsync(x => x.WorkshopId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Workshop not found", 404);
		dbEntity.IsDeleted = true;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(WorkshopService));
	}
}

public class EmployeeService : IEmployeeService
{
	private readonly AppDbContext _db;
	private readonly ICacheVersionProvider _version;
	public EmployeeService(AppDbContext db, ICacheVersionProvider version) { _db = db; _version = version; }

	[Cache(60)]
	public async Task<PagedResult<Employee>> GetListAsync(PagedRequest req, CancellationToken ct)
	{
		var q = _db.Employees.AsNoTracking().Where(x => !x.IsDeleted);
		if (req.DepartmentId.HasValue) q = q.Where(x => x.DepartmentId == req.DepartmentId);
		if (req.WorkshopId.HasValue) q = q.Where(x => x.WorkshopId == req.WorkshopId);
		if (!string.IsNullOrWhiteSpace(req.Keyword)) q = q.Where(x => x.EmployeeNo.Contains(req.Keyword!) || x.FullName.Contains(req.Keyword!));
		return await q.ToPagedResultAsync(req, ct);
	}

	public async Task<Employee> CreateAsync(Employee entity, CancellationToken ct)
	{
		if (await _db.Employees.AnyAsync(x => x.EmployeeNo == entity.EmployeeNo && !x.IsDeleted, ct))
			throw new BusinessException("EmployeeNo exists");
		_db.Employees.Add(entity);
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(EmployeeService));
		return entity;
	}

	public async Task<Employee> UpdateAsync(int id, Employee entity, CancellationToken ct)
	{
		var dbEntity = await _db.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Employee not found", 404);
		dbEntity.FullName = entity.FullName;
		dbEntity.DepartmentId = entity.DepartmentId;
		dbEntity.WorkshopId = entity.WorkshopId;
		dbEntity.Gender = entity.Gender;
		dbEntity.HireDate = entity.HireDate;
		dbEntity.BaseSalary = entity.BaseSalary;
		dbEntity.IsActive = entity.IsActive;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(EmployeeService));
		return dbEntity;
	}

	public async Task DeleteAsync(int id, CancellationToken ct)
	{
		var dbEntity = await _db.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id && !x.IsDeleted, ct) ?? throw new BusinessException("Employee not found", 404);
		// 禁止物理删除
		dbEntity.IsDeleted = true;
		dbEntity.IsActive = false;
		dbEntity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync(ct);
		_version.Bump(typeof(EmployeeService));
	}
}


