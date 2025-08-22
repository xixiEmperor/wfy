using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Common;
using PayrollApi.Data;
using PayrollApi.Domain.Entities;

namespace PayrollApi.Controllers;

/// <summary>
/// 开发辅助接口（仅开发环境使用）
/// </summary>
[Route("api/[controller]")]
public class DevController : BaseController
{
	private readonly AppDbContext _db;
	
	public DevController(AppDbContext db)
	{
		_db = db;
	}

	/// <summary>
	/// 清理并重新初始化示例数据
	/// </summary>
	[HttpPost("reset-data")]
	public async Task<ActionResult<ApiResponse<string>>> ResetData()
	{
		await using var transaction = await _db.Database.BeginTransactionAsync();
		
		try
		{
			// 清理所有数据
			await _db.Database.ExecuteSqlRawAsync("DELETE FROM PayrollItem");
			await _db.Database.ExecuteSqlRawAsync("DELETE FROM Payroll");
			await _db.Database.ExecuteSqlRawAsync("DELETE FROM SocialSecurity");
			await _db.Database.ExecuteSqlRawAsync("DELETE FROM LogisticsData");
			await _db.Database.ExecuteSqlRawAsync("DELETE FROM Attendance");
			await _db.Database.ExecuteSqlRawAsync("DELETE FROM YearEndBonus");
			await _db.Database.ExecuteSqlRawAsync("DELETE FROM SalaryChange");
			await _db.Database.ExecuteSqlRawAsync("DELETE FROM Employee");
			await _db.Database.ExecuteSqlRawAsync("DELETE FROM Workshop");
			await _db.Database.ExecuteSqlRawAsync("DELETE FROM Department");

			// 重置自增ID
			await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Department', RESEED, 0)");
			await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Workshop', RESEED, 0)");
			await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Employee', RESEED, 0)");
			await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Attendance', RESEED, 0)");
			await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('LogisticsData', RESEED, 0)");
			await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('SocialSecurity', RESEED, 0)");
			await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Payroll', RESEED, 0)");
			await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('PayrollItem', RESEED, 0)");
			await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('SalaryChange', RESEED, 0)");
			await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('YearEndBonus', RESEED, 0)");

			// 插入新的中文数据
			// 部门
			var departments = new[]
			{
				new Department { Name = "人事部", Description = "人力资源管理" },
				new Department { Name = "生产部", Description = "产品生产制造" },
				new Department { Name = "后勤部", Description = "后勤保障服务" },
				new Department { Name = "财务部", Description = "财务管理核算" },
				new Department { Name = "质检部", Description = "质量检验控制" }
			};
			_db.Departments.AddRange(departments);
			await _db.SaveChangesAsync();

			// 车间
			var workshops = new[]
			{
				new Workshop { Name = "一车间", DepartmentId = 2 },
				new Workshop { Name = "二车间", DepartmentId = 2 },
				new Workshop { Name = "三车间", DepartmentId = 2 },
				new Workshop { Name = "仓库", DepartmentId = 3 },
				new Workshop { Name = "食堂", DepartmentId = 3 }
			};
			_db.Workshops.AddRange(workshops);
			await _db.SaveChangesAsync();

			// 员工
			var employees = new[]
			{
				new Employee { EmployeeNo = "E001", FullName = "张小花", Gender = "女", DepartmentId = 2, WorkshopId = 1, HireDate = new DateOnly(2020, 1, 15), BaseSalary = 8000 },
				new Employee { EmployeeNo = "E002", FullName = "李明华", Gender = "男", DepartmentId = 2, WorkshopId = 2, HireDate = new DateOnly(2019, 3, 10), BaseSalary = 7800 },
				new Employee { EmployeeNo = "E003", FullName = "王建军", Gender = "男", DepartmentId = 3, WorkshopId = 4, HireDate = new DateOnly(2021, 5, 1), BaseSalary = 6000 },
				new Employee { EmployeeNo = "E004", FullName = "刘美丽", Gender = "女", DepartmentId = 1, WorkshopId = null, HireDate = new DateOnly(2018, 8, 20), BaseSalary = 7000 },
				new Employee { EmployeeNo = "E005", FullName = "陈志强", Gender = "男", DepartmentId = 5, WorkshopId = null, HireDate = new DateOnly(2022, 2, 11), BaseSalary = 6500 },
				new Employee { EmployeeNo = "E006", FullName = "赵小刚", Gender = "男", DepartmentId = 2, WorkshopId = 1, HireDate = new DateOnly(2020, 6, 1), BaseSalary = 7500 },
				new Employee { EmployeeNo = "E007", FullName = "孙丽华", Gender = "女", DepartmentId = 2, WorkshopId = 3, HireDate = new DateOnly(2021, 3, 15), BaseSalary = 7200 },
				new Employee { EmployeeNo = "E008", FullName = "周建国", Gender = "男", DepartmentId = 3, WorkshopId = 5, HireDate = new DateOnly(2019, 9, 20), BaseSalary = 6800 },
				new Employee { EmployeeNo = "E009", FullName = "吴小芳", Gender = "女", DepartmentId = 4, WorkshopId = null, HireDate = new DateOnly(2020, 12, 10), BaseSalary = 7300 },
				new Employee { EmployeeNo = "E010", FullName = "郑大勇", Gender = "男", DepartmentId = 5, WorkshopId = null, HireDate = new DateOnly(2021, 8, 5), BaseSalary = 6900 }
			};
			_db.Employees.AddRange(employees);
			await _db.SaveChangesAsync();

			// 添加年终奖数据
			var yearEndBonuses = new[]
			{
				new YearEndBonus { EmployeeId = 1, Year = 2024, Amount = 12000, Remark = "表现优秀" },
				new YearEndBonus { EmployeeId = 2, Year = 2024, Amount = 10000, Remark = "工作出色" },
				new YearEndBonus { EmployeeId = 3, Year = 2024, Amount = 8000, Remark = "表现良好" },
				new YearEndBonus { EmployeeId = 4, Year = 2024, Amount = 9000, Remark = "工作认真" },
				new YearEndBonus { EmployeeId = 5, Year = 2024, Amount = 7000, Remark = "新人奖励" }
			};
			_db.YearEndBonuses.AddRange(yearEndBonuses);

			// 添加考勤数据
			var attendances = new[]
			{
				new Attendance { EmployeeId = 1, WorkDate = new DateOnly(2025, 7, 1), HoursWorked = 8, OvertimeHours = 2, AbsentHours = 0 },
				new Attendance { EmployeeId = 1, WorkDate = new DateOnly(2025, 7, 2), HoursWorked = 8, OvertimeHours = 0, AbsentHours = 1 },
				new Attendance { EmployeeId = 2, WorkDate = new DateOnly(2025, 7, 1), HoursWorked = 8, OvertimeHours = 1, AbsentHours = 0 },
				new Attendance { EmployeeId = 3, WorkDate = new DateOnly(2025, 7, 1), HoursWorked = 8, OvertimeHours = 0, AbsentHours = 0 },
				new Attendance { EmployeeId = 4, WorkDate = new DateOnly(2025, 7, 1), HoursWorked = 8, OvertimeHours = 0.5m, AbsentHours = 0 }
			};
			_db.Attendances.AddRange(attendances);

			// 添加后勤数据
			var logisticsData = new[]
			{
				new LogisticsData { EmployeeId = 1, Month = "2025-07", HousingDeduction = 300, MealAllowance = 200, UtilitiesDeduction = 120 },
				new LogisticsData { EmployeeId = 2, Month = "2025-07", HousingDeduction = 200, MealAllowance = 150, UtilitiesDeduction = 100 },
				new LogisticsData { EmployeeId = 3, Month = "2025-07", HousingDeduction = 0, MealAllowance = 300, UtilitiesDeduction = 0 },
				new LogisticsData { EmployeeId = 4, Month = "2025-07", HousingDeduction = 100, MealAllowance = 0, UtilitiesDeduction = 80 },
				new LogisticsData { EmployeeId = 5, Month = "2025-07", HousingDeduction = 250, MealAllowance = 180, UtilitiesDeduction = 110 }
			};
			_db.LogisticsData.AddRange(logisticsData);

			// 添加社保数据
			var socialSecurities = new[]
			{
				new SocialSecurity { EmployeeId = 1, Month = "2025-07", Pension = 800, Medical = 300, Unemployment = 50, HousingFund = 600 },
				new SocialSecurity { EmployeeId = 2, Month = "2025-07", Pension = 780, Medical = 280, Unemployment = 50, HousingFund = 580 },
				new SocialSecurity { EmployeeId = 3, Month = "2025-07", Pension = 600, Medical = 220, Unemployment = 40, HousingFund = 450 },
				new SocialSecurity { EmployeeId = 4, Month = "2025-07", Pension = 700, Medical = 260, Unemployment = 45, HousingFund = 520 },
				new SocialSecurity { EmployeeId = 5, Month = "2025-07", Pension = 650, Medical = 230, Unemployment = 40, HousingFund = 480 }
			};
			_db.SocialSecurities.AddRange(socialSecurities);

			await _db.SaveChangesAsync();
			await transaction.CommitAsync();
			
			return OkResponse("数据清理和重置完成，已使用中文员工姓名！");
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return FailResponse<string>($"数据重置失败: {ex.Message}");
		}
	}
}