using Microsoft.EntityFrameworkCore;
using PayrollApi.Domain.Entities;

namespace PayrollApi.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	public DbSet<Department> Departments => Set<Department>();
	public DbSet<Workshop> Workshops => Set<Workshop>();
	public DbSet<Employee> Employees => Set<Employee>();
	public DbSet<Attendance> Attendances => Set<Attendance>();
	public DbSet<LogisticsData> LogisticsData => Set<LogisticsData>();
	public DbSet<SocialSecurity> SocialSecurities => Set<SocialSecurity>();
	public DbSet<Payroll> Payrolls => Set<Payroll>();
	public DbSet<PayrollItem> PayrollItems => Set<PayrollItem>();
	public DbSet<SalaryChange> SalaryChanges => Set<SalaryChange>();
	public DbSet<YearEndBonus> YearEndBonuses => Set<YearEndBonus>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}

