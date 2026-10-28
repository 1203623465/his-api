# His.Api 后端项目文档

> 面向学习的分层 Web API 项目：ASP.NET Core + EF Core（Database First）+ MySQL。

---

## 一、这是什么

一个前后端分离的 HIS（医院信息管理）系统**后端服务**，以 RESTful API 形式向浏览器前端提供数据。

关键特征：

- **分层架构**：Controller（接口层）→ Service（业务层）→ DbContext（数据访问层）
- **Database First**：数据库表已存在，用 `dotnet ef dbcontext scaffold` 反向生成实体，不手写建表
- **统一响应**：所有接口返回 `{ code, message, data }`，前端据此统一处理成功/失败
- **事务一致性**：收费、退费等涉及多表写 + 库存变动的操作放在 EF Core 事务里

---

## 二、技术栈与依赖

| 依赖 | 版本 | 作用 |
|---|---|---|
| .NET | 10.0 | 运行时与 SDK |
| ASP.NET Core | 10.0 | Web API 框架（Controller 模式） |
| MySql.EntityFrameworkCore | 10.0.12 | Oracle 官方 MySQL EF Core 提供程序 |
| Microsoft.EntityFrameworkCore.Design | 10.0.13 | `dotnet ef` 脚手架/迁移命令支持 |
| Swashbuckle.AspNetCore | 10.1.4 | 生成 Swagger 接口文档（开发期） |
| Serilog.AspNetCore | 10.0.0 | 结构化日志：控制台 + 按天滚动文件（保留 30 天） |

---

## 三、目录结构（逐项说明）

```
His.Api/
├── His.Api.csproj              # 项目文件：声明框架、NuGet 依赖
├── Program.cs                  # 应用入口：服务注册 + 中间件管道编排
├── appsettings.json            # 配置：MySQL 连接串、Serilog 日志（级别/输出目标）
├── appsettings.Development.json# 开发环境配置（覆盖正式配置）
├── Properties/launchSettings.json # 本地启动配置（端口 5080 等）
├── Controllers/                # 接口层：HTTP 路由与参数校验，不写业务
│   ├── AuthController.cs       #   假登录 POST /api/auth/login
│   ├── DepartmentsController.cs#   科室 CRUD + 下拉/类型字典
│   ├── DoctorsController.cs    #   医生 CRUD + 在职下拉
│   ├── PatientsController.cs   #   患者 CRUD
│   ├── RegistrationsController.cs # 挂号：分页/队列/创建/退号
│   ├── VisitsController.cs     #   就诊：开始就诊/保存病历
│   ├── PrescriptionsController.cs # 处方：分页/明细/开具
│   ├── DrugsController.cs      #   药品 CRUD + 库存批次
│   ├── BillingController.cs    #   收费/退费/发药
│   └── StatsController.cs      #   仪表盘统计
├── Services/                   # 业务层：核心逻辑与事务集中在此
│   ├── DepartmentService.cs    #   科室（唯一性/引用保护删除）
│   ├── DoctorService.cs        #   医生（在职状态/引用保护删除）
│   ├── PatientService.cs       #   患者（身份证唯一/引用保护删除）
│   ├── RegistrationService.cs  #   挂号（号别费用/状态流转）
│   ├── VisitService.cs         #   就诊（挂号→就诊的枢纽）
│   ├── PrescriptionService.cs  #   处方（明细计价/事务写入）
│   ├── DrugService.cs          #   药品与库存批次
│   ├── BillingService.cs       #   ★收费/退费/发药（事务 + 库存联动）
│   └── StatsService.cs         #   仪表盘统计口径
├── Dtos/                       # 数据传输对象：隔离数据库实体与前端
│   ├── ApiResult.cs            #   统一响应结构 { code, message, data }
│   ├── Paging.cs               #   PageRequest / PageResult 分页模型
│   └── *.cs                    #   各模块请求/响应 DTO（带数据注解校验）
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs # 全局异常中间件 + ApiException 业务异常
├── logs/                       # Serilog 日志输出目录（运行时生成，不入版本库）
└── Models/                     # ★由 Scaffold 自动生成，勿手工改
    ├── HisDbContext.cs         #   DbContext：表→DbSet、列→字段映射配置
    └── *.cs                    #   实体类（对应数据库每张表）
```

> **关键约定**：`Models/` 是脚手架自动生成的，**不要手工修改**（表结构变了就重新生成）。手写代码全部在 `Controllers / Services / Dtos / Middleware`，重新生成不会覆盖它们。

---

## 四、分层架构与数据流

```
浏览器前端 (Vue3)
      │  HTTP / JSON
      ▼
Controllers  ── 只做：路由匹配、参数绑定、数据注解校验、调用 Service、包装 ApiResult
      │
      ▼
Services     ── 只做：业务规则、事务、库存计算、状态流转、校验引用关系
      │
      ▼
HisDbContext ── EF Core 把 LINQ 翻译成 SQL 发给 MySQL
      │
      ▼
MySQL (hospital_test)
```

**为什么要分三层？**

- Controller 薄：换接口形式（REST/GraphQL）不影响业务
- Service 独立：业务规则集中，便于单元测试与复用
- 实体隔离：数据库实体（Models）不直接返回给前端，通过 DTO 转换，避免暴露表结构和多余字段

---

## 五、核心设计讲解

### 1. 统一响应 `ApiResult<T>`

```csharp
public record ApiResult<T>(int Code, string Message, T? Data);
// Code = 0 成功；非 0 业务错误。错误时 HTTP 状态码仍返回 200，
// 前端通过 Code 判断，Message 是给用户看的提示。
```

### 2. 全局异常中间件

- 业务异常 `ApiException`：主动 `throw new ApiException("身份证号已存在")`，中间件转成 `{code, message}` 友好返回
- 未知异常：记录日志（含堆栈 + Method/Path/RequestId），对外只返回"服务器内部错误"，**不泄漏堆栈细节**。日志同时写控制台与 `logs/his-YYYYMMDD.log`

### 3. 分页模型

```csharp
public class PageRequest { Page=1, Size=10, Keyword=null }  // Size 自动夹取 1~100
public record PageResult<T>(long Total, IReadOnlyList<T> Items);
```

所有列表接口统一走这个模型，避免全表返回。

### 4. 一个重要踩坑点

官方 `MySql.EntityFrameworkCore` **无法翻译**"联表投影成 DTO 后再 `OrderBy`"的 LINQ（会报 500）。所以服务层统一采用：

```
先分页查实体 → 收集本次需要的关联 ID → 批量查名称 → 内存拼装成 DTO
```

例如"医生列表带科室名"：先 `Skip/Take` 取 10 个医生，再查这 10 个医生的科室名，最后拼装。**新增代码务必沿用此模式**。

### 5. 事务与库存联动（学习重点）

`BillingService` 是全书核心，看这三个方法：

- **收费 `ChargeAsync`**：校验处方状态 → 按药品汇总需求量校验库存 → 事务内「逐行扣库存（FIFO）+ 生成收费单」→ 处方状态 1→2
- **退费 `RefundAsync`**：校验状态 → 按「挂号+金额+未退费」定位收费单 → 事务内「回补库存 + 写退款记录」→ 状态 2→4
- **发药 `DispenseAsync`**：状态 2→3，写发药记录（库存已在收费时扣过，不再动）

库存扣减采用 **FEFO（先过期先用）**：按 `expire_date` 升序扣批次。

---

## 六、数据库（Database First）

- 库：`hospital_test`，连接串见 `appsettings.json` → `ConnectionStrings:HisDb`
- 实体由以下命令生成（表结构变更后重新执行即可）：

```bash
dotnet ef dbcontext scaffold "Server=localhost;Port=3306;Database=hospital_test;User=root;Password=<密码>;" MySql.EntityFrameworkCore -o Models -c HisDbContext --context-dir Models --no-onconfiguring --force --project His.Api
```

### 主要表与状态字典

| 表 | 说明 | 关键状态 |
|---|---|---|
| dept_dict | 科室 | is_active：0启用/1停用 |
| doctor | 医生 | is_active：0在职/1离职 |
| patient | 患者 | patient_sex：1男/2女/0未设置 |
| clinic_reg | 挂号 | reg_status：1已挂号/2已就诊/3已退号 |
| clinic_visit | 就诊 | 主诉+诊断 |
| prescription / prescription_detail | 处方头/明细 | pres_status：1未收费/2已收费/3已发药/4已退款 |
| drug / drug_stock | 药品/库存 | drug.is_active：1启用/0未启用 |
| pay_record / refund_record / dispense_record | 收费/退费/发药 | pay_type：1现金/2医保/3扫码 |

> 注意：`is_active` 语义在科室/医生（0=正常）与药品（1=启用）里**相反**，遵循原库设计，写代码时留意。

---

## 七、运行

```bash
cd His.Api
dotnet run                # http://localhost:5080 ，Swagger: /swagger
```

---

## 八、日志

日志由 **Serilog** 接管，业务代码统一注入 `ILogger<T>`，调用方式不需要变。

- 配置位置：`appsettings.json` 的 `Serilog` 节点（`appsettings.Development.json` 只覆盖级别）
- 输出目标：
  - **控制台**：开发时直接看
  - **文件**：`logs/his-YYYYMMDD.log`，**只记录 Warning 及以上**（业务异常 WRN、未处理异常 ERR、框架报错），按天滚动，保留 **30 天**，单文件超过 50MB 再按大小切分
- 调级别：改 `Serilog:MinimumLevel:Default`（`Verbose/Debug/Information/Warning/Error/Fatal`）
- 两级过滤：`MinimumLevel` 决定日志事件是否产生（控制台据此显示）；`Serilog:WriteTo[File]:Args:restrictedToMinimumLevel` 决定**这个 Sink 是否接收**。所以 EF Core 的 SQL 只在控制台出现，不写入文件。参数校验失败（400）由 `[ApiController]` 直接返回，不产生日志
- 环境差异：`appsettings.json` 是生产基线（EF Core 压在 `Warning`，不输出 SQL）；`appsettings.Development.json` 把 `Microsoft.EntityFrameworkCore` 放开到 `Information`，本地调试能看到 SQL，启动横幅 `Microsoft.Hosting.Lifetime` 也保持可见
- `MinimumLevel:Override` 是**最长前缀匹配**，越具体的命名空间优先级越高，所以开发环境只需单独放开 EF Core，不必改动生产基线
- 调保留天数/路径：改 `Serilog:WriteTo[File]:Args` 里的 `retainedFileCountLimit` / `path`
- 排障：前端报错时响应体不含堆栈，到 `logs/` 目录用 `RequestId` 或 `ERR` 搜索，能拿到完整异常堆栈与请求路径
- 注意：原 `Logging` 节点已移除，**改它不会生效**，级别统一以 `Serilog:MinimumLevel` 为准
- 脱敏：日志只记业务主键与金额，**不得**记录连接串、患者姓名/身份证、请求体与 QueryString

---

## 九、学习路径建议

1. 先读 `Program.cs`：理解"服务注册 + 中间件管道"两段式
2. 读 `Models/HisDbContext.cs` + 任一实体：理解 Database First 的映射
3. 读 `Dtos/ApiResult.cs`、`Paging.cs`、`Middleware/`：理解统一响应与异常
4. 读一个简单模块 `DepartmentService` + `DepartmentsController`：打通 Controller→Service→Db 全链路
5. 进阶读 `BillingService`：理解事务、库存联动、状态机
6. 动手：给某个模块加一个新接口（如"按日期范围查挂号"），照着现有模式写
