## 工资管理系统（.NET 8 Web API + SQL Server + Autofac AOP）

### 启动步骤
- 配置 `appsettings.json` 的 `ConnectionStrings:Default` 指向 SQL Server。
- 执行 `SQL/schema.sql` 初始化数据库与示例数据。
- 运行项目：`dotnet run`，访问 Swagger：`http://localhost:5095/swagger`。

### 账户样例（用于获取 JWT）
- admin/admin123（Admin）
- hr/hr123（HR）
- user/user123（User）

### AOP 与拦截器
- LogInterceptor：记录请求方法、参数、耗时、异常，输出到控制台与文件。
- CacheAttribute + CacheInterceptor：对读方法启用 IMemoryCache，Key=方法名+序列化参数。
- AuthorizeRoleAttribute + AuthorizeRoleInterceptor：基于 JWT 解析角色，未满足抛 403。

### 统一返回
接口统一返回：
```json
{
  "success": true,
  "message": "OK",
  "data": {},
  "meta": { "page": 1, "pageSize": 20, "total": 135, "sortBy": "CreatedAt", "sortDir": "desc" }
}
```

### 典型端点与示例 cURL
- POST /api/auth/login
```bash
curl -X POST http://localhost:5095/api/auth/login -H "Content-Type: application/json" -d '{"userName":"admin","password":"admin123"}'
```
- GET /api/employees?page=1&pageSize=20&keyword=Alice （需要 Bearer Token）

### 接口分组
- /api/departments, /api/workshops
- /api/employees
- /api/attendance, /api/logistics, /api/social-security
- /api/payrolls（含 generate/confirm/pay 与 items）
- /api/salary-changes, /api/year-end-bonuses
- /api/reports（summary、employee/{id}）

### 工资计算规则（示例）
- 固定部分：BaseSalary。
- 考勤：加班小时×(基础时薪×1.5)，缺勤小时按基础时薪扣减；基础时薪=BaseSalary/21.75/8。
- 后勤/社保：按月表聚合生成工资项。
- 确认后（Confirmed）锁定明细，禁止增删改，仅允许状态流转为 Paid。

### Swagger 安全
- 点击右上角「Authorize」，输入 `Bearer {token}`。

### 目录结构
```
PayrollApi/
  Auth/
  Common/
  Controllers/
  Data/
    Configurations/
  Domain/
    Entities/
  Interceptors/
  Services/
    Implementations/
  SQL/
  Program.cs
  PayrollApi.csproj
```


