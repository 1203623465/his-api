using His.Api.Middleware;
using His.Api.Models;
using His.Api.Services;
using His.Api.Swagger;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 日志：Serilog 接管所有 ILogger 输出，目标（控制台 + 按天滚动文件）与级别均由 appsettings 的 Serilog 节点驱动
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

const string CorsPolicy = "HisClient";

// 控制器 + Swagger（开发期接口文档）
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    //设置 Swagger 文档信息
    c.SwaggerDoc("v1", new() { Title = "HIS API", Version = "v1" });
    // 启用 Swagger 注解特性（[SwaggerOperation]、[SwaggerResponse] 等）
    c.EnableAnnotations();
    // 清空顶层 tags 定义，避免 Swagger UI 显示按控制器名生成的空英文分组
    c.DocumentFilter<ClearTopLevelTagsFilter>();
});

// EF Core（Oracle 官方 MySql.EntityFrameworkCore 提供程序，Database First 模式）
builder.Services.AddDbContext<HisDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("HisDb")
        ?? throw new InvalidOperationException("缺少连接字符串 ConnectionStrings:HisDb")));

// 业务服务注册（Scoped：每次请求一个实例，与 DbContext 生命周期一致）
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IDrugService, DrugService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IStatsService, StatsService>();

// CORS：放行 Vite 开发服务器（前端开发期同源代理之外的直连场景）
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// 全局异常中间件：业务异常→友好错误响应；未知异常→记日志并隐藏堆栈
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HIS API v1"));
}

app.UseCors(CorsPolicy);

app.MapControllers();

app.Run();
