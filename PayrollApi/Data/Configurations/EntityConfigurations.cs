using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollApi.Domain.Entities;

namespace PayrollApi.Data.Configurations;

public class DepartmentConfig : IEntityTypeConfiguration<Department>
{
	public void Configure(EntityTypeBuilder<Department> builder)
	{
		builder.ToTable("Department");
		builder.HasKey(x => x.DepartmentId);
		builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
		builder.Property(x => x.Description).HasMaxLength(200);
		builder.Property(x => x.IsDeleted).HasDefaultValue(false);
		builder.HasIndex(x => x.Name).IsUnique();
	}
}

public class WorkshopConfig : IEntityTypeConfiguration<Workshop>
{
	public void Configure(EntityTypeBuilder<Workshop> builder)
	{
		builder.ToTable("Workshop");
		builder.HasKey(x => x.WorkshopId);
		builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
		builder.Property(x => x.IsDeleted).HasDefaultValue(false);
		builder.HasOne(x => x.Department)
			.WithMany(d => d.Workshops)
			.HasForeignKey(x => x.DepartmentId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasIndex(x => new { x.DepartmentId, x.Name });
	}
}

public class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
	public void Configure(EntityTypeBuilder<Employee> builder)
	{
		builder.ToTable("Employee");
		builder.HasKey(x => x.EmployeeId);
		builder.Property(x => x.EmployeeNo).HasMaxLength(30).IsRequired();
		builder.Property(x => x.FullName).HasMaxLength(100).IsRequired();
		builder.Property(x => x.Gender).HasMaxLength(10).IsRequired();
		builder.Property(x => x.BaseSalary).HasColumnType("decimal(18,2)");
		builder.Property(x => x.IsActive).HasDefaultValue(true);
		builder.Property(x => x.IsDeleted).HasDefaultValue(false);
		builder.HasIndex(x => x.EmployeeNo).IsUnique();
		builder.HasOne(x => x.Department)
			.WithMany(d => d.Employees)
			.HasForeignKey(x => x.DepartmentId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne(x => x.Workshop)
			.WithMany(w => w.Employees)
			.HasForeignKey(x => x.WorkshopId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasIndex(x => new { x.DepartmentId, x.WorkshopId });
	}
}

public class AttendanceConfig : IEntityTypeConfiguration<Attendance>
{
	public void Configure(EntityTypeBuilder<Attendance> builder)
	{
		builder.ToTable("Attendance");
		builder.HasKey(x => x.AttendanceId);
		builder.Property(x => x.HoursWorked).HasColumnType("decimal(5,2)");
		builder.Property(x => x.OvertimeHours).HasColumnType("decimal(5,2)");
		builder.Property(x => x.AbsentHours).HasColumnType("decimal(5,2)");
		builder.Property(x => x.IsDeleted).HasDefaultValue(false);
		builder.HasOne(x => x.Employee)
			.WithMany(e => e.Attendances)
			.HasForeignKey(x => x.EmployeeId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasIndex(x => new { x.EmployeeId, x.WorkDate }).IsUnique();
	}
}

public class LogisticsDataConfig : IEntityTypeConfiguration<LogisticsData>
{
	public void Configure(EntityTypeBuilder<LogisticsData> builder)
	{
		builder.ToTable("LogisticsData");
		builder.HasKey(x => x.LogisticsDataId);
		builder.Property(x => x.Month).HasMaxLength(7).IsRequired();
		builder.Property(x => x.HousingDeduction).HasColumnType("decimal(18,2)");
		builder.Property(x => x.MealAllowance).HasColumnType("decimal(18,2)");
		builder.Property(x => x.UtilitiesDeduction).HasColumnType("decimal(18,2)");
		builder.Property(x => x.IsDeleted).HasDefaultValue(false);
		builder.HasOne(x => x.Employee)
			.WithMany(e => e.Logistics)
			.HasForeignKey(x => x.EmployeeId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasIndex(x => new { x.EmployeeId, x.Month }).IsUnique();
	}
}

public class SocialSecurityConfig : IEntityTypeConfiguration<SocialSecurity>
{
	public void Configure(EntityTypeBuilder<SocialSecurity> builder)
	{
		builder.ToTable("SocialSecurity");
		builder.HasKey(x => x.SocialSecurityId);
		builder.Property(x => x.Month).HasMaxLength(7).IsRequired();
		builder.Property(x => x.Pension).HasColumnType("decimal(18,2)");
		builder.Property(x => x.Medical).HasColumnType("decimal(18,2)");
		builder.Property(x => x.Unemployment).HasColumnType("decimal(18,2)");
		builder.Property(x => x.HousingFund).HasColumnType("decimal(18,2)");
		builder.Property(x => x.IsDeleted).HasDefaultValue(false);
		builder.HasOne(x => x.Employee)
			.WithMany(e => e.SocialSecurities)
			.HasForeignKey(x => x.EmployeeId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasIndex(x => new { x.EmployeeId, x.Month }).IsUnique();
	}
}

public class PayrollConfig : IEntityTypeConfiguration<Payroll>
{
	public void Configure(EntityTypeBuilder<Payroll> builder)
	{
		builder.ToTable("Payroll");
		builder.HasKey(x => x.PayrollId);
		builder.Property(x => x.Month).HasMaxLength(7).IsRequired();
		builder.Property(x => x.GrossAmount).HasColumnType("decimal(18,2)");
		builder.Property(x => x.Deductions).HasColumnType("decimal(18,2)");
		builder.Property(x => x.NetAmount).HasColumnType("decimal(18,2)");
		builder.Property(x => x.Status).HasConversion<byte>().HasDefaultValue((byte)PayrollStatus.Draft);
		builder.Property(x => x.IsDeleted).HasDefaultValue(false);
		builder.HasOne(x => x.Employee)
			.WithMany(e => e.Payrolls)
			.HasForeignKey(x => x.EmployeeId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasIndex(x => new { x.EmployeeId, x.Month }).IsUnique();
	}
}

public class PayrollItemConfig : IEntityTypeConfiguration<PayrollItem>
{
	public void Configure(EntityTypeBuilder<PayrollItem> builder)
	{
		builder.ToTable("PayrollItem");
		builder.HasKey(x => x.PayrollItemId);
		builder.Property(x => x.ItemType).HasMaxLength(30).IsRequired();
		builder.Property(x => x.ItemName).HasMaxLength(100).IsRequired();
		builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
		builder.Property(x => x.IsDeleted).HasDefaultValue(false);
		builder.HasOne(x => x.Payroll)
			.WithMany(p => p.Items)
			.HasForeignKey(x => x.PayrollId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasIndex(x => new { x.PayrollId, x.ItemType });
	}
}

public class SalaryChangeConfig : IEntityTypeConfiguration<SalaryChange>
{
	public void Configure(EntityTypeBuilder<SalaryChange> builder)
	{
		builder.ToTable("SalaryChange");
		builder.HasKey(x => x.SalaryChangeId);
		builder.Property(x => x.OldBaseSalary).HasColumnType("decimal(18,2)");
		builder.Property(x => x.NewBaseSalary).HasColumnType("decimal(18,2)");
		builder.Property(x => x.Reason).HasMaxLength(200).IsRequired();
		builder.Property(x => x.IsDeleted).HasDefaultValue(false);
		builder.HasOne(x => x.Employee)
			.WithMany(e => e.SalaryChanges)
			.HasForeignKey(x => x.EmployeeId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasIndex(x => new { x.EmployeeId, x.ChangeDate });
	}
}

public class YearEndBonusConfig : IEntityTypeConfiguration<YearEndBonus>
{
	public void Configure(EntityTypeBuilder<YearEndBonus> builder)
	{
		builder.ToTable("YearEndBonus");
		builder.HasKey(x => x.YearEndBonusId);
		builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
		builder.Property(x => x.Remark).HasMaxLength(200);
		builder.Property(x => x.IsDeleted).HasDefaultValue(false);
		builder.HasOne(x => x.Employee)
			.WithMany(e => e.YearEndBonuses)
			.HasForeignKey(x => x.EmployeeId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasIndex(x => new { x.EmployeeId, x.Year }).IsUnique();
	}
}

