namespace PayrollApi.Domain.Entities;

public abstract class BaseEntity
{
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }
}

public class Department : BaseEntity
{
	public int DepartmentId { get; set; }
	public string Name { get; set; } = null!;
	public string? Description { get; set; }
	public ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();
	public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

public class Workshop : BaseEntity
{
	public int WorkshopId { get; set; }
	public string Name { get; set; } = null!;
	public int DepartmentId { get; set; }
	public Department Department { get; set; } = null!;
	public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

public class Employee : BaseEntity
{
	public int EmployeeId { get; set; }
	public string EmployeeNo { get; set; } = null!;
	public string FullName { get; set; } = null!;
	public string Gender { get; set; } = null!; // tinyint or nvarchar(10) -> use string for simplicity
	public int DepartmentId { get; set; }
	public int? WorkshopId { get; set; }
	public DateOnly HireDate { get; set; }
	public decimal BaseSalary { get; set; }
	public bool IsActive { get; set; } = true;

	public Department Department { get; set; } = null!;
	public Workshop? Workshop { get; set; }
	public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
	public ICollection<LogisticsData> Logistics { get; set; } = new List<LogisticsData>();
	public ICollection<SocialSecurity> SocialSecurities { get; set; } = new List<SocialSecurity>();
	public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
	public ICollection<SalaryChange> SalaryChanges { get; set; } = new List<SalaryChange>();
	public ICollection<YearEndBonus> YearEndBonuses { get; set; } = new List<YearEndBonus>();
}

public class Attendance : BaseEntity
{
	public int AttendanceId { get; set; }
	public int EmployeeId { get; set; }
	public DateOnly WorkDate { get; set; }
	public decimal HoursWorked { get; set; }
	public decimal OvertimeHours { get; set; }
	public decimal AbsentHours { get; set; }
	public Employee Employee { get; set; } = null!;
}

public class LogisticsData : BaseEntity
{
	public int LogisticsDataId { get; set; }
	public int EmployeeId { get; set; }
	public string Month { get; set; } = null!; // yyyy-MM
	public decimal HousingDeduction { get; set; }
	public decimal MealAllowance { get; set; }
	public decimal UtilitiesDeduction { get; set; }
	public Employee Employee { get; set; } = null!;
}

public class SocialSecurity : BaseEntity
{
	public int SocialSecurityId { get; set; }
	public int EmployeeId { get; set; }
	public string Month { get; set; } = null!; // yyyy-MM
	public decimal Pension { get; set; }
	public decimal Medical { get; set; }
	public decimal Unemployment { get; set; }
	public decimal HousingFund { get; set; }
	public Employee Employee { get; set; } = null!;
}

public enum PayrollStatus : byte
{
	Draft = 0,
	Confirmed = 1,
	Paid = 2
}

public class Payroll : BaseEntity
{
	public int PayrollId { get; set; }
	public int EmployeeId { get; set; }
	public string Month { get; set; } = null!;
	public decimal GrossAmount { get; set; }
	public decimal Deductions { get; set; }
	public decimal NetAmount { get; set; }
	public PayrollStatus Status { get; set; }
	public Employee Employee { get; set; } = null!;
	public ICollection<PayrollItem> Items { get; set; } = new List<PayrollItem>();
}

public class PayrollItem : BaseEntity
{
	public int PayrollItemId { get; set; }
	public int PayrollId { get; set; }
	public string ItemType { get; set; } = null!; // Fixed, Attendance, Bonus, Penalty, Allowance, SocialSecurity, Logistics, Other
	public string ItemName { get; set; } = null!;
	public decimal Amount { get; set; }
	public Payroll Payroll { get; set; } = null!;
}

public class SalaryChange : BaseEntity
{
	public int SalaryChangeId { get; set; }
	public int EmployeeId { get; set; }
	public DateOnly ChangeDate { get; set; }
	public decimal OldBaseSalary { get; set; }
	public decimal NewBaseSalary { get; set; }
	public string Reason { get; set; } = null!;
	public Employee Employee { get; set; } = null!;
}

public class YearEndBonus : BaseEntity
{
	public int YearEndBonusId { get; set; }
	public int EmployeeId { get; set; }
	public int Year { get; set; }
	public decimal Amount { get; set; }
	public string? Remark { get; set; }
	public Employee Employee { get; set; } = null!;
}

