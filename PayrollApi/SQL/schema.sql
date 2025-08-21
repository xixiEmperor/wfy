-- SQL Server DDL & Seed for PayrollDb
IF DB_ID('PayrollDb') IS NULL CREATE DATABASE PayrollDb;
GO
USE PayrollDb;
GO

-- Drop existing tables (for idempotent dev setup)
IF OBJECT_ID('dbo.PayrollItem','U') IS NOT NULL DROP TABLE dbo.PayrollItem;
IF OBJECT_ID('dbo.Payroll','U') IS NOT NULL DROP TABLE dbo.Payroll;
IF OBJECT_ID('dbo.SocialSecurity','U') IS NOT NULL DROP TABLE dbo.SocialSecurity;
IF OBJECT_ID('dbo.LogisticsData','U') IS NOT NULL DROP TABLE dbo.LogisticsData;
IF OBJECT_ID('dbo.Attendance','U') IS NOT NULL DROP TABLE dbo.Attendance;
IF OBJECT_ID('dbo.YearEndBonus','U') IS NOT NULL DROP TABLE dbo.YearEndBonus;
IF OBJECT_ID('dbo.SalaryChange','U') IS NOT NULL DROP TABLE dbo.SalaryChange;
IF OBJECT_ID('dbo.Employee','U') IS NOT NULL DROP TABLE dbo.Employee;
IF OBJECT_ID('dbo.Workshop','U') IS NOT NULL DROP TABLE dbo.Workshop;
IF OBJECT_ID('dbo.Department','U') IS NOT NULL DROP TABLE dbo.Department;
GO

CREATE TABLE dbo.Department (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_Department_Name UNIQUE(Name)
);
GO

CREATE TABLE dbo.Workshop (
    WorkshopId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    DepartmentId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Workshop_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.Department(DepartmentId) ON DELETE NO ACTION ON UPDATE CASCADE
);
GO
CREATE INDEX IX_Workshop_Department_Name ON dbo.Workshop(DepartmentId, Name);
GO

CREATE TABLE dbo.Employee (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeNo NVARCHAR(30) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    DepartmentId INT NOT NULL,
    WorkshopId INT NULL,
    HireDate DATE NOT NULL,
    BaseSalary DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_Employee_EmployeeNo UNIQUE(EmployeeNo),
    CONSTRAINT FK_Employee_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.Department(DepartmentId) ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT FK_Employee_Workshop FOREIGN KEY (WorkshopId) REFERENCES dbo.Workshop(WorkshopId) ON DELETE NO ACTION ON UPDATE CASCADE
);
GO
CREATE INDEX IX_Employee_Department_Workshop ON dbo.Employee(DepartmentId, WorkshopId);
GO

CREATE TABLE dbo.Attendance (
    AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    WorkDate DATE NOT NULL,
    HoursWorked DECIMAL(5,2) NOT NULL DEFAULT 0,
    OvertimeHours DECIMAL(5,2) NOT NULL DEFAULT 0,
    AbsentHours DECIMAL(5,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Attendance_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId) ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT UQ_Attendance_EmpDate UNIQUE(EmployeeId, WorkDate)
);
GO

CREATE TABLE dbo.LogisticsData (
    LogisticsDataId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Month CHAR(7) NOT NULL,
    HousingDeduction DECIMAL(18,2) NOT NULL DEFAULT 0,
    MealAllowance DECIMAL(18,2) NOT NULL DEFAULT 0,
    UtilitiesDeduction DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Logistics_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId) ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT UQ_Logistics_EmpMonth UNIQUE(EmployeeId, Month)
);
GO

CREATE TABLE dbo.SocialSecurity (
    SocialSecurityId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Month CHAR(7) NOT NULL,
    Pension DECIMAL(18,2) NOT NULL DEFAULT 0,
    Medical DECIMAL(18,2) NOT NULL DEFAULT 0,
    Unemployment DECIMAL(18,2) NOT NULL DEFAULT 0,
    HousingFund DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Social_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId) ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT UQ_Social_EmpMonth UNIQUE(EmployeeId, Month)
);
GO

CREATE TABLE dbo.Payroll (
    PayrollId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Month CHAR(7) NOT NULL,
    GrossAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Deductions DECIMAL(18,2) NOT NULL DEFAULT 0,
    NetAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status TINYINT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Payroll_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId) ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT UQ_Payroll_EmpMonth UNIQUE(EmployeeId, Month)
);
GO

CREATE TABLE dbo.PayrollItem (
    PayrollItemId INT IDENTITY(1,1) PRIMARY KEY,
    PayrollId INT NOT NULL,
    ItemType VARCHAR(30) NOT NULL,
    ItemName NVARCHAR(100) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_PayrollItem_Payroll FOREIGN KEY (PayrollId) REFERENCES dbo.Payroll(PayrollId) ON DELETE CASCADE ON UPDATE CASCADE
);
GO
CREATE INDEX IX_PayrollItem_Payroll_Type ON dbo.PayrollItem(PayrollId, ItemType);
GO

CREATE TABLE dbo.SalaryChange (
    SalaryChangeId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    ChangeDate DATE NOT NULL,
    OldBaseSalary DECIMAL(18,2) NOT NULL,
    NewBaseSalary DECIMAL(18,2) NOT NULL,
    Reason NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_SalaryChange_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId) ON DELETE NO ACTION ON UPDATE CASCADE
);
GO
CREATE INDEX IX_SalaryChange_EmpDate ON dbo.SalaryChange(EmployeeId, ChangeDate);
GO

CREATE TABLE dbo.YearEndBonus (
    YearEndBonusId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    [Year] INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Remark NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_YearEndBonus_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employee(EmployeeId) ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT UQ_YearEndBonus_EmpYear UNIQUE(EmployeeId, [Year])
);
GO

-- Seed data (≥5 each)
INSERT INTO dbo.Department(Name, Description) VALUES
('HR', 'Human Resources'),
('Production', 'Production Dept'),
('Logistics', 'Logistics Dept'),
('Finance', 'Finance Dept'),
('QA', 'Quality Assurance');
GO

INSERT INTO dbo.Workshop(Name, DepartmentId) VALUES
('Workshop A', 2),('Workshop B', 2),('Workshop C', 2),('Warehouse', 3),('Canteen', 3);
GO

INSERT INTO dbo.Employee(EmployeeNo, FullName, Gender, DepartmentId, WorkshopId, HireDate, BaseSalary) VALUES
('E001','Alice','Female',2,1,'2020-01-15',8000),
('E002','Bob','Male',2,2,'2019-03-10',7800),
('E003','Charlie','Male',3,4,'2021-05-01',6000),
('E004','Diana','Female',1,NULL,'2018-08-20',7000),
('E005','Evan','Male',5,NULL,'2022-02-11',6500);
GO

-- Attendance sample (mix of overtime/absent)
INSERT INTO dbo.Attendance(EmployeeId, WorkDate, HoursWorked, OvertimeHours, AbsentHours) VALUES
(1,'2025-07-01',8,2,0),(1,'2025-07-02',8,0,1),(1,'2025-07-03',8,1.5,0),
(2,'2025-07-01',8,1,0),(2,'2025-07-02',8,2,0),(2,'2025-07-03',8,0,2),
(3,'2025-07-01',8,0,0),(3,'2025-07-02',8,1,0),(3,'2025-07-03',8,0,0),
(4,'2025-07-01',8,0.5,0),(4,'2025-07-02',8,0,0.5),(5,'2025-07-01',8,2,0);
GO

-- LogisticsData
INSERT INTO dbo.LogisticsData(EmployeeId, Month, HousingDeduction, MealAllowance, UtilitiesDeduction) VALUES
(1,'2025-07',300,200,120),(2,'2025-07',200,150,100),(3,'2025-07',0,300,0),(4,'2025-07',100,0,80),(5,'2025-07',250,180,110);
GO

-- SocialSecurity
INSERT INTO dbo.SocialSecurity(EmployeeId, Month, Pension, Medical, Unemployment, HousingFund) VALUES
(1,'2025-07',800,300,50,600),
(2,'2025-07',780,280,50,580),
(3,'2025-07',600,220,40,450),
(4,'2025-07',700,260,45,520),
(5,'2025-07',650,230,40,480);
GO

-- SalaryChange
INSERT INTO dbo.SalaryChange(EmployeeId, ChangeDate, OldBaseSalary, NewBaseSalary, Reason) VALUES
(1,'2025-05-01',7500,8000,'Performance'),
(2,'2025-04-01',7600,7800,'Promotion'),
(3,'2025-03-01',5800,6000,'Annual'),
(4,'2024-12-01',6800,7000,'KPI'),
(5,'2025-01-01',6200,6500,'Adjustment');
GO

-- YearEndBonus
INSERT INTO dbo.YearEndBonus(EmployeeId, [Year], Amount, Remark) VALUES
(1,2024,12000,'Excellent'),
(2,2024,10000,'Great'),
(3,2024,8000,'Solid'),
(4,2024,9000,'Good'),
(5,2024,7000,'Newcomer');
GO


