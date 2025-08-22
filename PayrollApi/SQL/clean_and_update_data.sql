-- 清理旧数据并重新插入中文员工数据
USE PayrollDb;
GO

-- 清理所有相关表的数据 (保持外键约束顺序)
DELETE FROM dbo.PayrollItem;
DELETE FROM dbo.Payroll;
DELETE FROM dbo.SocialSecurity;
DELETE FROM dbo.LogisticsData;
DELETE FROM dbo.Attendance;
DELETE FROM dbo.YearEndBonus;
DELETE FROM dbo.SalaryChange;
DELETE FROM dbo.Employee;
DELETE FROM dbo.Workshop;
DELETE FROM dbo.Department;
GO

-- 重置自增ID
DBCC CHECKIDENT ('Department', RESEED, 0);
DBCC CHECKIDENT ('Workshop', RESEED, 0);
DBCC CHECKIDENT ('Employee', RESEED, 0);
DBCC CHECKIDENT ('Attendance', RESEED, 0);
DBCC CHECKIDENT ('LogisticsData', RESEED, 0);
DBCC CHECKIDENT ('SocialSecurity', RESEED, 0);
DBCC CHECKIDENT ('Payroll', RESEED, 0);
DBCC CHECKIDENT ('PayrollItem', RESEED, 0);
DBCC CHECKIDENT ('SalaryChange', RESEED, 0);
DBCC CHECKIDENT ('YearEndBonus', RESEED, 0);
GO

-- 重新插入部门数据
INSERT INTO dbo.Department(Name, Description) VALUES
('人事部', '人力资源管理'),
('生产部', '产品生产制造'),
('后勤部', '后勤保障服务'),
('财务部', '财务管理核算'),
('质检部', '质量检验控制');
GO

-- 重新插入车间数据
INSERT INTO dbo.Workshop(Name, DepartmentId) VALUES
('一车间', 2),
('二车间', 2),
('三车间', 2),
('仓库', 3),
('食堂', 3);
GO

-- 重新插入员工数据 (使用中文姓名)
INSERT INTO dbo.Employee(EmployeeNo, FullName, Gender, DepartmentId, WorkshopId, HireDate, BaseSalary) VALUES
('E001','张小花','女',2,1,'2020-01-15',8000),
('E002','李明华','男',2,2,'2019-03-10',7800),
('E003','王建军','男',3,4,'2021-05-01',6000),
('E004','刘美丽','女',1,NULL,'2018-08-20',7000),
('E005','陈志强','男',5,NULL,'2022-02-11',6500),
('E006','赵小刚','男',2,1,'2020-06-01',7500),
('E007','孙丽华','女',2,3,'2021-03-15',7200),
('E008','周建国','男',3,5,'2019-09-20',6800),
('E009','吴小芳','女',4,NULL,'2020-12-10',7300),
('E010','郑大勇','男',5,NULL,'2021-08-05',6900);
GO

-- 重新插入考勤数据
INSERT INTO dbo.Attendance(EmployeeId, WorkDate, HoursWorked, OvertimeHours, AbsentHours) VALUES
(1,'2025-07-01',8,2,0),(1,'2025-07-02',8,0,1),(1,'2025-07-03',8,1.5,0),
(2,'2025-07-01',8,1,0),(2,'2025-07-02',8,2,0),(2,'2025-07-03',8,0,2),
(3,'2025-07-01',8,0,0),(3,'2025-07-02',8,1,0),(3,'2025-07-03',8,0,0),
(4,'2025-07-01',8,0.5,0),(4,'2025-07-02',8,0,0.5),(5,'2025-07-01',8,2,0),
(6,'2025-07-01',8,1.5,0),(7,'2025-07-01',8,0,0),(8,'2025-07-01',8,1,0),
(9,'2025-07-01',8,0.5,0),(10,'2025-07-01',8,2,0);
GO

-- 重新插入后勤数据
INSERT INTO dbo.LogisticsData(EmployeeId, Month, HousingDeduction, MealAllowance, UtilitiesDeduction) VALUES
(1,'2025-07',300,200,120),(2,'2025-07',200,150,100),(3,'2025-07',0,300,0),
(4,'2025-07',100,0,80),(5,'2025-07',250,180,110),(6,'2025-07',280,170,90),
(7,'2025-07',150,200,60),(8,'2025-07',100,250,70),(9,'2025-07',200,150,100),
(10,'2025-07',300,180,120);
GO

-- 重新插入社保数据
INSERT INTO dbo.SocialSecurity(EmployeeId, Month, Pension, Medical, Unemployment, HousingFund) VALUES
(1,'2025-07',800,300,50,600),(2,'2025-07',780,280,50,580),(3,'2025-07',600,220,40,450),
(4,'2025-07',700,260,45,520),(5,'2025-07',650,230,40,480),(6,'2025-07',750,270,48,560),
(7,'2025-07',720,250,45,540),(8,'2025-07',680,240,42,510),(9,'2025-07',730,260,46,550),
(10,'2025-07',690,235,43,520);
GO

-- 重新插入薪资变更数据
INSERT INTO dbo.SalaryChange(EmployeeId, ChangeDate, OldBaseSalary, NewBaseSalary, Reason) VALUES
(1,'2025-05-01',7500,8000,'绩效优秀'),
(2,'2025-04-01',7600,7800,'职务晋升'),
(3,'2025-03-01',5800,6000,'年度调薪'),
(4,'2024-12-01',6800,7000,'KPI达标'),
(5,'2025-01-01',6200,6500,'薪资调整');
GO

-- 重新插入年终奖数据
INSERT INTO dbo.YearEndBonus(EmployeeId, [Year], Amount, Remark) VALUES
(1,2024,12000,'表现优秀'),
(2,2024,10000,'工作出色'),
(3,2024,8000,'表现良好'),
(4,2024,9000,'工作认真'),
(5,2024,7000,'新人奖励'),
(6,2024,11000,'技术能手'),
(7,2024,8500,'团队协作'),
(8,2024,9500,'安全生产'),
(9,2024,10500,'财务管理'),
(10,2024,8000,'质量管控');
GO

PRINT '数据清理和更新完成！';