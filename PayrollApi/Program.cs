using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using PayrollApi.Data;
using PayrollApi.Auth;
using PayrollApi.Interceptors;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
	loggerConfiguration
		.ReadFrom.Configuration(context.Configuration)
		.Enrich.FromLogContext()
		.WriteTo.Console()
		.WriteTo.File(
			path: context.Configuration.GetValue<string>("Serilog:FilePath") ?? "logs/log-.txt",
			rollingInterval: RollingInterval.Day,
			retainedFileCountLimit: 10,
			shared: true
		);
});

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
	var jwtSection = builder.Configuration.GetSection("Jwt");
	var secret = jwtSection.GetValue<string>("Secret") ?? "SuperSecretKeyForDevOnly_change_me";
	var issuer = jwtSection.GetValue<string>("Issuer") ?? "PayrollApi";
	var audience = jwtSection.GetValue<string>("Audience") ?? "PayrollApiAudience";
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = issuer,
		ValidAudience = audience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
	};
});

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Payroll API",
		Version = "v1",
		Description = "中小型生产企业工资管理系统 API"
	});
	var securityScheme = new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "输入 JWT，如: Bearer {token}"
	};
	c.AddSecurityDefinition("Bearer", securityScheme);
	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{ securityScheme, new string[] { } }
	});
});

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
	var assembly = Assembly.GetExecutingAssembly();

	// Interceptors
	containerBuilder.RegisterType<LogInterceptor>().SingleInstance();
	containerBuilder.RegisterType<CacheInterceptor>().SingleInstance();
	containerBuilder.RegisterType<AuthorizeRoleInterceptor>().SingleInstance();
	containerBuilder.RegisterType<CacheVersionProvider>().As<ICacheVersionProvider>().SingleInstance();

	// Services
	containerBuilder
		.RegisterAssemblyTypes(assembly)
		.Where(t => t.Name.EndsWith("Service") && !t.Namespace!.Contains("Interceptors"))
		.AsImplementedInterfaces()
		.InstancePerLifetimeScope()
		.EnableInterfaceInterceptors()
		.InterceptedBy(
			typeof(LogInterceptor),
			typeof(CacheInterceptor),
			typeof(AuthorizeRoleInterceptor)
		);

	containerBuilder.RegisterType<JwtService>().As<IJwtService>().SingleInstance();
});

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	await db.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<PayrollApi.Controllers.ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
