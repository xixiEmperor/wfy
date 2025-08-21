# 薪资管理系统前端

基于 Vue 3 + Element Plus + Tailwind CSS 的现代化薪资管理系统前端应用。

## 技术栈

- **框架**: Vue 3 (Composition API)
- **构建工具**: Vite
- **语言**: JavaScript
- **状态管理**: Pinia
- **路由**: Vue Router
- **HTTP客户端**: Axios
- **UI组件库**: Element Plus
- **样式**: Tailwind CSS
- **图表**: ECharts
- **工具库**: Day.js, File-saver, XLSX, PapaParse

## 功能特性

### 🔐 用户认证
- JWT Token 登录
- 基于角色的权限控制 (Admin/HR/User)
- 路由守卫与页面权限

### 📊 仪表盘
- 数据概览卡片
- 工资发放趋势图表
- 部门分布饼图
- 系统动态展示

### 👥 组织管理
- **部门管理**: 部门的增删改查
- **车间管理**: 车间与部门关联管理
- **员工管理**: 员工信息管理，支持批量导入

### 📅 考勤管理
- 考勤记录的录入与编辑
- 按日显示与按月汇总切换
- 批量导入考勤数据
- 加班时长与缺勤统计

### 💰 薪资管理
- **工资单管理**: 自动生成、编辑、确认、发放
- **工资明细**: 分项目显示各种收入与扣除
- **工资变动**: 调薪记录与历史追踪
- **年终奖金**: 年度奖金发放记录

### 🏠 后勤管理
- 住宿费扣除
- 餐费补贴
- 水电费扣除
- 按月维度管理

### 🛡️ 三金管理
- 养老保险
- 医疗保险
- 失业保险
- 住房公积金

### 📈 统计报表
- 多维度数据查询
- 图表可视化展示
- 数据导出功能
- 员工薪资历史

## 环境要求

- Node.js >= 16.0.0
- pnpm >= 8.0.0

## 安装与运行

### 1. 安装依赖

```bash
pnpm install
```

### 2. 配置环境变量

复制环境变量文件：

```bash
cp .env .env.local
```

根据实际情况修改 `.env.local` 中的配置：

```env
# 应用标题
VITE_APP_TITLE=薪资管理系统

# API 基础地址 (后端服务地址)
VITE_API_BASE=http://localhost:5095
```

### 3. 启动开发服务器

```bash
pnpm dev
```

应用将在 http://localhost:5173 启动

### 4. 构建生产版本

```bash
pnpm build
```

构建文件将生成在 `dist` 目录中

## 项目结构

```
src/
├── api/                    # API 接口封装
├── components/             # 通用组件
│   ├── DataTable.vue      # 数据表格组件
│   ├── FilterBar.vue      # 筛选条组件
│   ├── FileUpload.vue     # 文件上传组件
│   └── ...
├── layouts/                # 布局组件
│   └── MainLayout.vue     # 主布局
├── router/                 # 路由配置
│   └── index.js           # 路由定义与守卫
├── stores/                 # Pinia 状态管理
│   ├── user.js            # 用户状态
│   └── app.js             # 应用状态
├── styles/                 # 样式文件
│   └── tailwind.css       # Tailwind CSS 配置
├── utils/                  # 工具函数
│   └── index.js           # 通用工具
├── views/                  # 页面组件
│   ├── Auth/              # 登录页面
│   ├── Dashboard/         # 仪表盘
│   ├── Departments/       # 部门管理
│   ├── Workshops/         # 车间管理
│   ├── Employees/         # 员工管理
│   ├── Attendance/        # 考勤管理
│   ├── Logistics/         # 后勤管理
│   ├── SocialSecurity/    # 三金管理
│   ├── Payrolls/          # 工资单管理
│   ├── SalaryChanges/     # 工资变动
│   ├── YearEndBonus/      # 年终奖金
│   └── Reports/           # 统计报表
├── App.vue                # 根组件
└── main.js               # 入口文件
```

## 演示账号

系统提供以下演示账号：

| 角色 | 用户名 | 密码 | 权限说明 |
|------|--------|------|----------|
| 管理员 | admin | admin123 | 所有功能权限 |
| HR | hr | hr123 | 除系统管理外的所有权限 |
| 普通用户 | user | user123 | 只读权限 |

## 权限说明

### Admin (管理员)
- 所有模块的完整 CRUD 权限
- 系统配置管理
- 用户管理

### HR (人力资源)
- 员工管理
- 薪资管理
- 考勤管理
- 报表查看
- 不包含系统配置权限

### User (普通用户)
- 查看个人信息
- 查看个人薪资记录
- 基础数据浏览权限

## 批量导入功能

系统支持以下模块的批量导入：

1. **员工信息导入**
   - 支持 Excel (.xlsx, .xls) 和 CSV 格式
   - 提供标准模板下载
   - 导入前数据预览与验证

2. **考勤数据导入**
   - 按模板格式批量导入考勤记录
   - 支持错误行提示与修正

## 开发指南

### 添加新页面

1. 在 `src/views/` 下创建页面组件
2. 在 `src/router/index.js` 中添加路由配置
3. 如需权限控制，在路由 meta 中配置 roles

### API 接口

所有 API 接口统一在 `src/api/index.js` 中管理，遵循 RESTful 规范。

### 组件开发

- 使用 Composition API
- 遵循单一职责原则
- 合理使用 Props 和 Emits
- 添加适当的类型检查

## 部署说明

### Nginx 配置示例

```nginx
server {
    listen 80;
    server_name your-domain.com;
    root /path/to/dist;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://your-backend-server:5095;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### Docker 部署

```dockerfile
FROM nginx:alpine
COPY dist/ /usr/share/nginx/html/
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## 常见问题

### Q: 如何修改 API 地址？
A: 修改 `.env` 或 `.env.local` 文件中的 `VITE_API_BASE` 变量

### Q: 如何添加新的权限角色？
A: 在后端添加角色后，前端在路由配置和权限检查中添加对应的角色判断

### Q: 批量导入失败如何处理？
A: 检查导入文件格式是否符合模板要求，查看错误提示信息进行修正

## 技术支持

如遇问题请检查：

1. Node.js 版本是否符合要求
2. 后端服务是否正常启动
3. 网络连接是否正常
4. 浏览器控制台错误信息

## 更新日志

### v1.0.0 (2024-01-01)
- 初始版本发布
- 完整的薪资管理功能
- 基于角色的权限控制
- 数据导入导出功能
