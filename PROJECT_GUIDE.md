# BLE Tracking Backend - Project Guide

## Table of Contents
1. [Project Overview](#project-overview)
2. [Tech Stack](#tech-stack)
3. [Architecture](#architecture)
4. [Project Structure](#project-structure)
5. [Coding Guidelines](#coding-guidelines)
6. [Service Port Mapping](#service-port-mapping)
7. [Development Commands](#development-commands)
8. [Common Patterns](#common-patterns)

---

## Project Overview

BLE Tracking Backend adalah sistem microservices untuk memonitor lokasi orang dan gadget menggunakan teknologi Bluetooth Low Energy (BLE). Sistem ini menyediakan fitur real-time tracking, alarm system, patrol management, dan card access control.

### Key Features
- **Real-time BLE Tracking**: Melacak posisi perangkat BLE secara real-time
- **Alarm System**: Deteksi dan notifikasi pelanggaran area (Geofence, Boundary, StayOnArea, Overpopulating)
- **Patrol Management**: Manajemen patroli security dengan route dan checkpoint
- **Card Access Control**: Sistem akses kartu dengan time-based restrictions
- **Visitor Management**: Manajemen visitor dengan pre-registration
- **CCTV & Access Control Integration**: Integrasi dengan perangkat CCTV dan access control
- **Analytics & Reporting**: Dashboard dan laporan tracking

---

## Tech Stack

### Core Framework
- **.NET 8.0** (SDK 8.0.411) - Primary framework
- **C# 12** - Programming language
- **ASP.NET Core 8.0** - Web API framework

### Database & ORM
- **SQL Server** - Primary database
- **Entity Framework Core 8.0.13** - ORM utama
- **Dapper 2.1.66** - Micro-ORM untuk query kompleks

### Authentication & Authorization
- **JWT Bearer Authentication** - Token-based auth
- **BCrypt.Net-Next 4.0.3** - Password hashing

### Communication
- **MQTT (MQTTnet 4.3.3)** - Real-time messaging dengan engine tracking
- **HTTP/REST** - API communication antar services
- **Nginx** - API Gateway dan reverse proxy

### Caching & Storage
- **Redis (StackExchange.Redis 2.9.32)** - Distributed caching
- **Local File Storage** - Untuk gambar floorplan, visitor, member

### Background Processing
- **Quartz.NET 3.15.0** - Job scheduling
- **IHostedService** - Background services

### API Documentation
- **Swagger/OpenAPI (Swashbuckle 6.6.2)** - API documentation

### Validation & Mapping
- **FluentValidation 12.0.0** - Input validation
- **AutoMapper 12.0.1** - Object mapping

### Logging
- **Serilog** - Structured logging
- **Console, File** - Log outputs

### Utilities
- **ClosedXML 0.105.0** - Excel export
- **QuestPDF 2024.12.2** - PDF generation
- **CsvHelper 33.1.0** - CSV processing
- **SixLabors.ImageSharp 3.1.12** - Image processing
- **System.Linq.Dynamic.Core 1.6.6** - Dynamic LINQ queries

### Development Tools
- **Docker & Docker Compose** - Containerization
- **Git** - Version control

---

## Architecture

### Microservices Architecture
Project menggunakan arsitektur microservices dengan prinsip Clean Architecture. Setiap service adalah standalone ASP.NET Core application yang dapat di-deploy secara independen.

```
┌─────────────────────────────────────────────────────────────┐
│                        API Gateway                          │
│                         (Nginx)                             │
└──────────────┬──────────────────────────────────────────────┘
               │
       ┌───────┴───────┐
       │   Routing     │
       └───────┬───────┘
               │
    ┌──────────┼──────────┬──────────┬──────────┐
    │          │          │          │          │
┌───┴───┐ ┌───┴───┐ ┌───┴───┐ ┌───┴───┐ ┌───┴───┐
│ Auth  │ │ Mst*  │ │Tracking│ │ Alarm │ │ Card  │
│Service│ │Services│ │Service │ │Service│ │Service│
└───┬───┘ └───┬───┘ └───┬───┘ └───┬───┘ └───┬───┘
    │         │         │         │         │
    └─────────┴─────────┴─────────┴─────────┘
                        │
            ┌───────────┴───────────┐
            │    Shared Libraries   │
            │  (Entities, Services, │
            │   Repositories, DTOs) │
            └───────────┬───────────┘
                        │
                ┌───────┴───────┐
                │   Database    │
                │  SQL Server   │
                └───────────────┘
```

### Clean Architecture Layers
1. **Controllers Layer** (`Web.API.Controllers`): Handle HTTP requests/responses
2. **Business Logic Layer** (`BusinessLogic.Services`): Business rules dan logic
3. **Repository Layer** (`Repositories`): Data access abstraction
4. **Entity Layer** (`Entities.Models`): Domain entities

### Design Patterns
- **Repository Pattern**: Abstract data access dengan `BaseRepository`
- **Unit of Work**: Transaksi database management
- **Dependency Injection**: Service registration di `Program.cs`
- **CQRS** (partial): Command dan query separation untuk tracking
- **Extension Methods**: Modular DI configuration
- **DTO Pattern**: Data transfer antara layers

---

## Project Structure

```
BleTrackingBackend/
├── Services.API/                       # Microservices (30+ services)
│   ├── Auth/                          # Authentication & Authorization
│   ├── MstBuilding/                   # Master data: Building
│   ├── MstFloor/                      # Master data: Floor
│   ├── MstFloorplan/                  # Master data: Floorplan
│   ├── MstOrganization/               # Master data: Organization
│   ├── MstDepartment/                 # Master data: Department
│   ├── MstMember/                     # Master data: Member/Employee
│   ├── MstSecurity/                   # Master data: Security
│   ├── MstBleReader/                  # Master data: BLE Readers
│   ├── MstAccessControl/              # Master data: Access Control
│   ├── MstAccessCctv/                 # Master data: CCTV
│   ├── MstBrand/                      # Master data: Brand
│   ├── MstDistrict/                   # Master data: District
│   ├── MstIntegration/                # Master data: Integration
│   ├── MstApplication/                # Master data: Application
│   ├── MstEngine/                     # Master data: Tracking Engine
│   ├── Visitor/                       # Visitor management
│   ├── TrxVisitor/                    # Visitor transactions
│   ├── TrackingTransaction/           # Real-time tracking data
│   ├── AlarmRecordTracking/           # Alarm records
│   ├── AlarmTriggers/                 # Alarm trigger rules
│   ├── FloorplanDevice/               # Devices on floorplan
│   ├── FloorplanMaskedArea/           # Masked/restricted areas
│   ├── Card/                          # Card management
│   ├── CardAccess/                    # Card access rules
│   ├── CardRecord/                    # Card transaction records
│   ├── Patrol/                        # Patrol management
│   ├── Geofence/                      # Geofence rules
│   ├── Analytics/                     # Dashboard & analytics
│   ├── JobsScheduler/                 # Background job scheduler
│   ├── MonitoringConfig/              # Monitoring configuration
│   ├── GatewayHealthApi/              # Health check aggregator
│   ├── BleReaderNode/                 # BLE Reader node service
│   ├── MqttPublisher/                 # MQTT message publisher
│   ├── LicenseChecker/                # License validation
│   ├── BlacklistArea/                 # Blacklist area management
│   └── ExtAlarm/                      # External alarm integration
│
├── Shared/                            # Shared libraries
│   ├── Entities.Models/               # Domain entities
│   │   ├── BaseModel.cs               # Base entity class
│   │   ├── User.cs                    # User entity
│   │   ├── MstBuilding.cs             # Building entity
│   │   ├── TrackingTransaction.cs     # Tracking entity
│   │   └── [50+ entity files]
│   │
│   ├── Data.ViewModels/               # DTOs & View Models
│   │   ├── MstBuildingDto.cs          # Building DTO
│   │   ├── TrackingTransactionDto.cs  # Tracking DTO
│   │   ├── Validators/                # FluentValidation validators
│   │   └── Shared/
│   │       ├── ApiResponseHelper.cs   # Standard API response
│   │       └── ExceptionHelper/       # Exception handling
│   │
│   ├── BusinessLogic.Services/        # Business logic layer
│   │   ├── Interface/                 # Service interfaces
│   │   │   ├── IAuthService.cs
│   │   │   ├── IMstBuildingService.cs
│   │   │   └── [50+ interfaces]
│   │   ├── Implementation/            # Service implementations
│   │   │   ├── AuthService.cs
│   │   │   ├── MstBuildingService.cs
│   │   │   ├── BaseService.cs         # Base service dengan audit
│   │   │   └── [50+ services]
│   │   ├── Extension/                 # Extension methods
│   │   │   ├── RootExtension/         # DI extensions
│   │   │   │   ├── AuthExtensions.cs      # JWT setup
│   │   │   │   ├── DbContextExtensions.cs # DB setup
│   │   │   │   ├── SwaggerExtensions.cs   # Swagger setup
│   │   │   │   ├── CorsExtensions.cs      # CORS setup
│   │   │   │   └── [Other extensions]
│   │   │   ├── MstBuildingProfile.cs      # AutoMapper profile
│   │   │   └── [50+ profiles]
│   │   ├── Background/                # Background services
│   │   ├── JobsScheduler/             # Quartz jobs
│   │   └── Resolver/                  # Service resolver
│   │
│   ├── Repositories/                  # Data access layer
│   │   ├── Repository/
│   │   │   ├── BaseRepository.cs          # Base CRUD operations
│   │   │   ├── BaseProjectionRepository.cs # Projection repository
│   │   │   ├── MstBuildingRepository.cs
│   │   │   └── [50+ repositories]
│   │   ├── DbContexts/
│   │   │   └── BleTrackingDbContext.cs    # EF Core DbContext
│   │   ├── Migrations/                # EF Core migrations
│   │   └── Seeding/                   # Database seeding
│   │
│   ├── Web.API.Controllers/           # Shared controllers
│   │   └── Controllers/
│   │       ├── AuthController.cs
│   │       ├── MstBuildingController.cs
│   │       └── [50+ controllers]
│   │
│   ├── Shared.Contracts/              # Shared contracts
│   │   ├── enum.cs                    # Shared enums
│   │   └── Read/                      # Read contracts
│   │
│   └── Helpers.Consumer/              # Utility classes
│       ├── BaseModel.cs               # Base model helpers
│       ├── IApplicationEntity.cs      # App entity interface
│       ├── Mqtt/                      # MQTT client
│       └── EmailTemplate/             # Email templates
│
├── docker-compose.yml                 # Docker orchestration
├── docker-compose-prod.yml            # Production compose
├── Directory.Packages.props           # Central package management
├── global.json                        # .NET SDK version
├── BleTrackingBackend.sln             # Solution file
├── BleTrackingBackend.csproj          # Root project file
├── nginx.conf                         # Nginx configuration
├── .env                               # Environment variables (local)
├── .gitignore                         # Git ignore rules
└── appsettings.json                   # Shared app configuration
```

---

## Coding Guidelines

### 1. Project Structure Guidelines

#### Service Project Structure
Setiap microservice harus memiliki struktur minimal:
```
Services.API/[ServiceName]/
├── [ServiceName].csproj           # Project file dengan SDK Web
├── Program.cs                      # Entry point
├── Dockerfile                      # Container configuration
├── appsettings.json                # Configuration
├── appsettings.Development.json    # Dev configuration
└── Properties/
    └── launchSettings.json         # Launch settings
```

#### Program.cs Template
```csharp
using BusinessLogic.Services.Background;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Data.ViewModels.Shared.ExceptionHelper;
using Helpers.Consumer.Mqtt;
using Microsoft.AspNetCore.Authorization;
using Repositories.DbContexts;
using Repositories.Repository;
using Serilog;
using System.Text.Json.Serialization;

EnvTryCatchExtension.LoadEnvWithTryCatch();

var builder = WebApplication.CreateBuilder(args);

builder.UseSerilogExtension();
builder.Host.UseWindowsService();
builder.Host.UseSerilog();

builder.Services.AddCorsExtension();
builder.Services.AddDbContextExtension(builder.Configuration);
// Optional: builder.Services.AddRedisExtension(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddValidatorExtensions();
builder.Services.AddSwaggerExtension();

builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationNewPolicies();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof([Service]Profile));

// Register service-specific dependencies
builder.Services.AddScoped<I[Service]Service, [Service]Service>();
builder.Services.AddScoped<[Service]Repository>();

// Optional background/MQTT
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddHostedService<MqttRecoveryService>();
// Optional Redis recovery if Redis enabled
// builder.Services.AddHostedService<RedisRecoveryService>();

builder.UseDefaultHostExtension("[SERVICE]_PORT", "[PORT]");

var app = builder.Build();

app.UseHealthCheckExtension();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "[Service] API");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseSerilogRequestLoggingExtension();
app.UseMiddleware<CustomExceptionMiddleware>();
app.UseApiKeyAuthentication();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

#### Unified Refactor Guide (Current Standard)
Use this single checklist for refactoring a Service/Entity stack (DataTables + Auth + Middleware):

1. Analyze & prepare
2. Identify target entity and files:
3. `Shared/Shared.Contracts/Read/[Entity]Read.cs`
4. `Shared/Shared.Contracts/[Entity]Filter.cs`
5. `Shared/Repositories/Repository/[Entity]Repository.cs`
6. `Shared/BusinessLogic.Services/Implementation/[Entity]Service.cs`
7. `Shared/Web.API.Controllers/Controllers/[Entity]Controller.cs`
8. `Services.API/[Service]/Program.cs`
9. Create filter + read contracts
10. Filter DTO must include `Search`, `Page`, `PageSize`, `SortColumn`, `SortDir`, and entity-specific filters (e.g., `DateFrom`, `Status`).
11. Read DTO lives in `Shared/Shared.Contracts/Read` and is used for projection and responses.
12. Repository refactor
13. Add `BaseEntityQuery()` if needed and use it (do not duplicate tenant filter logic).
14. Add `ProjectToRead(...)` with `AsNoTracking()` + manual `Select` (no AutoMapper in query).
15. Add `FilterAsync(FilterDto)` that uses `ApplySorting` and `ApplyPaging`.
16. Service refactor
17. Use `DataTablesProjectedRequest` + typed filter, map request fields to filter.
18. Throw `NotFoundException` for missing entity, `BusinessException` for logic errors.
19. Controller refactor
20. Apply `[MinLevel(LevelPriority.SuperAdmin)]` at class level.
21. Remove manual try/catch and use `ApiResponse` helpers.
22. Filter endpoint must deserialize `request.Filters` with `PropertyNameCaseInsensitive = true`.
23. Program.cs refactor
24. Use RootExtension methods, register `MinLevelHandler`, and include `CustomExceptionMiddleware`.
25. Definition of done
26. Repository has `FilterAsync` returning `(List, int, int)`.
27. Projection uses manual `Select` only.
28. Service maps `DataTablesProjectedRequest` to filter.
29. Controller uses `ApiResponse` and no manual try/catch.
30. Program.cs uses standard extensions and middleware.

Reference implementations: `PatrolCaseRepository.cs`, `PatrolCaseService.cs`, `PatrolCaseController.cs`, and `MstDistrictController.cs`.

### 2. Naming Conventions

#### Files & Classes
- **Controllers**: `[Entity]Controller.cs` (e.g., `MstBuildingController.cs`)
- **Services**: `[Entity]Service.cs` (e.g., `MstBuildingService.cs`)
- **Repositories**: `[Entity]Repository.cs` (e.g., `MstBuildingRepository.cs`)
- **Entities**: `[Entity].cs` (e.g., `MstBuilding.cs`)
- **DTOs**: `[Entity]Dto.cs` (e.g., `MstBuildingDto.cs`)
- **Validators**: `[Entity]Validator.cs` (e.g., `MstBuildingValidator.cs`)
- **Profiles**: `[Entity]Profile.cs` (e.g., `MstBuildingProfile.cs`)

#### Interfaces
- Prefix dengan `I`: `I[Entity]Service`, `I[Entity]Repository`

#### Variables & Properties
- **Private fields**: `_camelCase` (e.g., `_context`, `_httpContextAccessor`)
- **Properties**: `PascalCase` (e.g., `Name`, `CreatedAt`)
- **Method parameters**: `camelCase` (e.g., `id`, `request`)
- **Constants**: `UPPER_CASE` atau `PascalCase`

### 3. Entity Guidelines

#### Entity Structure
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstBuilding : BaseModel, IApplicationEntity
    {
        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("description")]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public MstApplication Application { get; set; }

        // Status: 0 = Deleted, 1 = Active
        [Column("status")]
        public int Status { get; set; } = 1;
    }
}
```

#### Base Model Types
- **BaseModel**: Standard dengan `Id` (Guid) dan `Generate` (long)
- **BaseModelWithTime**: Tambah `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
- **IApplicationEntity**: Interface untuk entities dengan `ApplicationId` (multi-tenancy)

### 4. Repository Guidelines

#### Base Repository Pattern
```csharp
public class MstBuildingRepository : BaseRepository
{
    public MstBuildingRepository(
        BleTrackingDbContext context, 
        IHttpContextAccessor httpContextAccessor) 
        : base(context, httpContextAccessor) { }

    public async Task<MstBuilding> GetByIdAsync(Guid id)
    {
        var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
        
        var query = _context.MstBuildings.AsNoTracking();
        
        if (!isSystemAdmin && applicationId.HasValue)
        {
            query = query.Where(b => b.ApplicationId == applicationId.Value);
        }
        
        return await query.FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
    }

    public async Task<List<MstBuilding>> GetAllAsync()
    {
        var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
        
        var query = _context.MstBuildings
            .AsNoTracking()
            .Where(b => b.Status != 0);
        
        if (!isSystemAdmin && applicationId.HasValue)
        {
            query = query.Where(b => b.ApplicationId == applicationId.Value);
        }
        
        return await query.ToListAsync();
    }

    public async Task AddAsync(MstBuilding entity)
    {
        await _context.MstBuildings.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MstBuilding entity)
    {
        _context.MstBuildings.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var entity = await _context.MstBuildings.FindAsync(id);
        if (entity != null)
        {
            entity.Status = 0;
            await _context.SaveChangesAsync();
        }
    }
}
```

#### Specific Repository Pattern (Recommended for DataTables)
Untuk fitur yang kompleks (DataTables, reporting), gunakan pendekatan **Specific Repository** dengan **Manual Projection**:

```csharp
public class MstBuildingRepository : BaseRepository
{
    // ... basic CRUD methods ...

    public async Task<(List<MstBuildingRead> Data, int Total, int Filtered)> FilterAsync(
        MstBuildingFilter filter)
    {
        var query = BaseEntityQuery(); // Start with base query (tenant filter etc)

        var total = await query.CountAsync();

        // 1. Specific Filtering
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(s));
        }

        var filtered = await query.CountAsync();

        // 2. Sorting & Paging (Use QueryableExtensions)
        query = query.ApplySorting(filter.SortColumn, filter.SortDir);
        query = query.ApplyPaging(filter.Page, filter.PageSize);

        // 3. Manual Projection (CRITICAL: Select specific fields only)
        var data = await query.AsNoTracking().Select(b => new MstBuildingRead
        {
            Id = b.Id,
            Name = b.Name,
            Description = b.Description,
            ApplicationName = b.Application.Name, // Easy Join
            CreatedAt = b.CreatedAt
        }).ToListAsync();

        return (data, total, filtered);
    }
}
```

### 5. Service Guidelines

#### Service Structure
```csharp
public class MstBuildingService : BaseService, IMstBuildingService
{
    private readonly MstBuildingRepository _repository;
    private readonly IMapper _mapper;

    public MstBuildingService(
        MstBuildingRepository repository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor) 
        : base(httpContextAccessor)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<object> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("Building not found");
        
        var dto = _mapper.Map<MstBuildingDto>(entity);
        return ApiResponse.Success("Success", dto);
    }

    public async Task<object> CreateAsync(CreateMstBuildingDto dto)
    {
        var entity = _mapper.Map<MstBuilding>(dto);
        entity.Id = Guid.NewGuid();
        entity.ApplicationId = AppId; // Dari BaseService
        
        SetCreateAudit(entity); // Dari BaseService
        
        await _repository.AddAsync(entity);
        
        return ApiResponse.Created("Building created successfully", entity.Id);
    }

    public async Task<object> UpdateAsync(Guid id, UpdateMstBuildingDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("Building not found");
        
        _mapper.Map(dto, entity);
        SetUpdateAudit(entity); // Dari BaseService
        
        await _repository.UpdateAsync(entity);
        
        return ApiResponse.Success("Building updated successfully", entity.Id);
    }

    public async Task<object> DeleteAsync(Guid id)
    {
        await _repository.SoftDeleteAsync(id);
        return ApiResponse.Success("Building deleted successfully");
    }
}
```

### 6. Controller Guidelines

#### Controller Structure
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MstBuildingController : ControllerBase
{
    private readonly IMstBuildingService _service;

    public MstBuildingController(IMstBuildingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMstBuildingDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMstBuildingDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return Ok(result);
    }
}
```

### 7. DTO Guidelines

#### DTO Structure
```csharp
public class MstBuildingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid ApplicationId { get; set; }
    public string ApplicationName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateMstBuildingDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
}

public class UpdateMstBuildingDto
{
    [StringLength(255)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
}
```

### 8. Validation Guidelines

#### FluentValidation
```csharp
public class CreateMstBuildingValidator : AbstractValidator<CreateMstBuildingDto>
{
    public CreateMstBuildingValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters");
            
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}
```

### 9. AutoMapper Guidelines

#### Profile Definition
```csharp
public class MstBuildingProfile : Profile
{
    public MstBuildingProfile()
    {
        CreateMap<MstBuilding, MstBuildingDto>()
            .ForMember(dest => dest.ApplicationName, 
                opt => opt.MapFrom(src => src.Application.Name));
        
        CreateMap<CreateMstBuildingDto, MstBuilding>();
        CreateMap<UpdateMstBuildingDto, MstBuilding>();
    }
}
```

### 10. API Response Guidelines

#### Standard Response Format
```json
{
  "success": true,
  "msg": "Operation successful",
  "collection": {
    "data": { ... }
  },
  "code": 200
}
```

#### Menggunakan ApiResponse Helper
```csharp
// Success
return ApiResponse.Success("Building created", data);
return ApiResponse.Created("Building created", data);
return ApiResponse.NoContent("Deleted");

// Error
return ApiResponse.BadRequest("Invalid input", errors);
return ApiResponse.NotFound("Building not found");
return ApiResponse.Unauthorized("Access denied");
return ApiResponse.Forbidden("Permission denied");
return ApiResponse.InternalError("Server error");

// Paginated
return ApiResponse.Paginated("Success", paginatedData);
```

### 11. Extension Methods Guidelines

#### Service Registration Extensions
```csharp
public static class ServiceExtensions
{
    public static IServiceCollection Add[Service]Services(this IServiceCollection services)
    {
        services.AddScoped<I[Service]Service, [Service]Service>();
        services.AddScoped<[Service]Repository>();
        return services;
    }
}
```

### 12. Middleware Guidelines

#### Custom Exception Middleware
```csharp
public class CustomExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public CustomExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(ApiResponse.NotFound(ex.Message));
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(ApiResponse.BadRequest(ex.Message, ex.Errors));
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(ApiResponse.InternalError(ex.Message));
        }
    }
}
```

### 13. Multi-tenancy Guidelines

#### Application ID Filtering
Semua entity yang terkait dengan aplikasi harus mengimplementasikan `IApplicationEntity`:

```csharp
public interface IApplicationEntity
{
    Guid ApplicationId { get; set; }
}
```

Di repository, gunakan `GetApplicationIdAndRole()` dari `BaseRepository`:

```csharp
protected (Guid? ApplicationId, bool IsSystemAdmin) GetApplicationIdAndRole()
{
    var isSystemAdmin = _httpContextAccessor.HttpContext?.User.HasClaim(
        c => c.Type == ClaimTypes.Role && c.Value == LevelPriority.System.ToString());
    
    if (isSystemAdmin == true)
        return (null, true);

    var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;
    if (Guid.TryParse(applicationIdClaim, out var applicationId))
        return (applicationId, false);

    return (null, false);
}
```

### 14. Configuration Guidelines

#### Environment Variables
Gunakan `.env` file untuk local development dan environment variables untuk production:

```bash
# Database
DB_HOST=localhost
DB_PORT=1433
DB_NAME=people_tracking_db
DB_USER=sa
DB_PASSWORD=P@ssw0rd

# JWT
JWT_KEY=6Q9kRBtUe46ktWTTd1mUy2nrKrbRIBDk
JWT_ISSUER=TrackingBleAuth
JWT_AUDIENCE=TrackingBleAPI

# MQTT
MQTT_HOST=localhost
MQTT_PORT=1883
MQTT_USERNAME=user
MQTT_PASSWORD=pass

# Service Ports
AUTH_PORT=5001
MST_BUILDING_PORT=5010
```

#### appsettings.json
```json
{
  "ConnectionStrings": {
    "BleTrackingConnectionString": "Server=localhost,1433;Database=people_tracking_db;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "6Q9kRBtUe46ktWTTd1mUy2nrKrbRIBDk",
    "Issuer": "TrackingBleAuth",
    "Audience": "TrackingBleAPI"
  },
  "Mqtt": {
    "Host": "localhost",
    "Port": 1883,
    "Username": "user",
    "Password": "pass"
  }
}
```

---

## Service Port Mapping

| Service | Port | Description |
|---------|------|-------------|
| Auth | 5001 | Authentication & Authorization |
| FloorplanDevice | 5003 | Manage devices on floorplans |
| FloorplanMaskedArea | 5004 | Restricted area management |
| MstAccessCctv | 5005 | CCTV device management |
| MstAccessControl | 5006 | Access control management |
| MstApplication | 5007 | Application master data |
| MstBleReader | 5008 | BLE reader management |
| MstBrand | 5009 | Brand management |
| MstBuilding | 5010 | Building master data |
| MstDepartment | 5011 | Department management |
| MstDistrict | 5012 | District/location management |
| MstFloor | 5013 | Floor management |
| MstFloorplan | 5014 | Floorplan management |
| MstIntegration | 5015 | Integration settings |
| MstMember | 5016 | Member/employee management |
| MstOrganization | 5017 | Organization management |
| TrackingTransaction | 5018 | Real-time tracking data |
| Visitor | 5019 | Visitor management |
| Patrol | 5020 | Patrol management |
| AlarmRecordTracking | 5002 | Alarm record management |
| MstEngine | 5022 | Tracking engine management |
| CardRecord | 5024 | Card transaction records |
| TrxVisitor | 5025 | Visitor transactions |
| Card | 5026 | Card management |
| AlarmTriggers | 5027 | Alarm trigger rules |
| CardAccess | 5028 | Card access control |
| MonitoringConfig | 5029 | Monitoring configuration |
| Geofence | 5030 | Geofence rules & management |
| Analytics | 5031 | Dashboard & analytics |
| GatewayHealthApi | 8080 | Health check aggregator |

---

## Development Commands

### Build & Run

#### Build All Services
```bash
# Build solution
dotnet build BleTrackingBackend.sln

# Build specific service
dotnet build Services.API/MstBuilding/MstBuilding.csproj
```

#### Run Services
```bash
# Run specific service
dotnet run --project Services.API/Auth/Auth.csproj

# Run with specific environment
ASPNETCORE_ENVIRONMENT=Development dotnet run --project Services.API/Auth/Auth.csproj
```

### Docker Commands

#### Build & Run with Docker Compose
```bash
# Build all images
docker-compose build

# Start all services
docker-compose up -d

# Start specific service
docker-compose up -d auth mst-building

# View logs
docker-compose logs -f auth

# Stop all services
docker-compose down

# Rebuild specific service
docker-compose up -d --build auth
```

#### Production Deployment
```bash
# Production compose
docker-compose -f docker-compose-prod.yml up -d
```

### Database Commands

#### Entity Framework Migrations
```bash
# Add migration
dotnet ef migrations add [MigrationName] --project Shared/Repositories/Repositories.csproj --startup-project Services.API/[Service]/[Service].csproj

# Update database
dotnet ef database update --project Shared/Repositories/Repositories.csproj --startup-project Services.API/[Service]/[Service].csproj

# Remove last migration
dotnet ef migrations remove --project Shared/Repositories/Repositories.csproj --startup-project Services.API/[Service]/[Service].csproj
```

### Windows Service Commands

#### Install as Windows Service
```bash
# Install service
install-service.bat

# Install all services
install-all-services.bat

# Start all services
start-all-service.bat

# Stop all services
stop-all-service.bat

# Restart all services
restart-all-service.bat
```

### Publishing

#### Single Service Publish
```bash
# Publish specific service
dotnet publish Services.API/Auth/Auth.csproj -c Release -o ./publish/auth

# Publish all (using script)
publish_single.bat
```

---

## Common Patterns

### 1. CRUD Operations

Standard CRUD pattern untuk semua master data:

```csharp
// Service
public interface IMstBuildingService
{
    Task<object> GetAllAsync();
    Task<object> GetByIdAsync(Guid id);
    Task<object> CreateAsync(CreateMstBuildingDto dto);
    Task<object> UpdateAsync(Guid id, UpdateMstBuildingDto dto);
    Task<object> DeleteAsync(Guid id);
}

// Repository
public class MstBuildingRepository
{
    public async Task<List<MstBuilding>> GetAllAsync();
    public async Task<MstBuilding> GetByIdAsync(Guid id);
    public async Task AddAsync(MstBuilding entity);
    public async Task UpdateAsync(MstBuilding entity);
    public async Task SoftDeleteAsync(Guid id);
}
```

### 2. DataTables Implementation Pattern

Gunakan pattern **Controller -> Service -> Repository** dengan typed filter:

```csharp
// 1. Controller (Deserialize JSON Filter)
[HttpPost("filter")]
public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
{
    var filter = new MstBuildingFilter();
    if (request.Filters.ValueKind == JsonValueKind.Object)
    {
        filter = JsonSerializer.Deserialize<MstBuildingFilter>(
            request.Filters.GetRawText(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? new MstBuildingFilter();
    }
    
    var result = await _service.FilterAsync(request, filter);
    return Ok(ApiResponse.Paginated("Success", result));
}

// 2. Service (Map Request & Bridge)
public async Task<object> FilterAsync(DataTablesProjectedRequest request, MstBuildingFilter filter)
{
    // Map Standard DataTables params
    filter.Page = (request.Start / request.Length) + 1;
    filter.PageSize = request.Length;
    filter.SortColumn = request.SortColumn;
    filter.SortDir = request.SortDir;
    filter.Search = request.SearchValue;

    var (data, total, filtered) = await _repo.FilterAsync(filter);

    return new
    {
        draw = request.Draw,
        recordsTotal = total,
        recordsFiltered = filtered,
        data = data
    };
}

// 3. Repository (Query & Projection)
// Lihat bagian "Specific Repository Pattern" di atas.
```

### 3. Soft Delete Pattern

```csharp
// Entity
public class MstBuilding : BaseModel
{
    public int Status { get; set; } = 1; // 1 = Active, 0 = Deleted
}

// Repository
public async Task SoftDeleteAsync(Guid id)
{
    var entity = await _context.MstBuildings.FindAsync(id);
    if (entity != null)
    {
        entity.Status = 0;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}

// Query Filter di DbContext
modelBuilder.Entity<MstBuilding>().HasQueryFilter(b => b.Status != 0);
```

### 4. Audit Trail Pattern

```csharp
// Base entity with audit
public class BaseModelWithTime : BaseModel
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
}

// Base service
public abstract class BaseService
{
    protected void SetCreateAudit(BaseModelWithTime entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = UsernameFormToken;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = UsernameFormToken;
    }
    
    protected void SetUpdateAudit(BaseModelWithTime entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = UsernameFormToken;
    }
}
```

### 5. JWT Claims Pattern

```csharp
// Token claims
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Group.Name),
    new Claim("ApplicationId", user.ApplicationId.ToString()),
    new Claim("groupId", user.GroupId.ToString()),
    new Claim("groupName", user.Group.Name)
};

// Reading claims di service
protected Guid AppId => Guid.Parse(Http.HttpContext?.User.FindFirst("ApplicationId")?.Value);
protected string UsernameFormToken => Http.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
protected string Role => Http.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
```

### 6. MQTT Message Pattern

```csharp
// MQTT Service
public class MqttClientService : IMqttClientService
{
    public event Func<string, string, Task> OnMessageReceived;
    
    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        
        if (OnMessageReceived != null)
            await OnMessageReceived(topic, payload);
    }
}

// Subscriber
public class EngineMqttListener : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _mqttService.OnMessageReceived += async (topic, payload) =>
        {
            // Process message
            await ProcessTrackingMessage(topic, payload);
        };
    }
}
```

### 7. Multi-tenancy Pattern

```csharp
// Entity dengan application isolation
public class MstBuilding : BaseModel, IApplicationEntity
{
    public Guid ApplicationId { get; set; }
    public MstApplication Application { get; set; }
}

// Repository filtering
protected IQueryable<T> ApplyTenantFilter<T>(IQueryable<T> query) where T : class, IApplicationEntity
{
    var (appId, isSystemAdmin) = GetApplicationIdAndRole();
    
    if (!isSystemAdmin && appId.HasValue)
        return query.Where(x => x.ApplicationId == appId.Value);
    
    return query;
}
```

### 8. Exception Handling Pattern

```csharp
// Custom exceptions
public class NotFoundException : Exception 
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public Dictionary<string, string> Errors { get; set; }
    public ValidationException(string message, Dictionary<string, string> errors) : base(message)
    {
        Errors = errors;
    }
}

// Middleware handling
public class CustomExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(ApiResponse.NotFound(ex.Message));
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(ApiResponse.BadRequest(ex.Message, ex.Errors));
        }
    }
}
```

### 9. API Key Authentication Pattern

```csharp
// Middleware
public class ApiKeyMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-API-KEY", out var apiKey))
        {
            var integration = await _integrationService.ValidateApiKey(apiKey);
            if (integration != null)
            {
                context.Items["MstIntegration"] = integration;
                await _next(context);
                return;
            }
        }
        
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(ApiResponse.Unauthorized("Invalid API Key"));
    }
}

// Registration
app.UseMiddleware<ApiKeyMiddleware>();
```

### 10. Background Job Pattern

```csharp
// Job using Quartz
public class CleanupOldTrackingJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // Cleanup old tracking data
        await _trackingRepo.DeleteOldRecordsAsync(days: 30);
    }
}

// Registration
builder.Services.AddQuartz(q =>
{
    q.AddJob<CleanupOldTrackingJob>(j => j.WithIdentity("cleanupJob"));
    q.AddTrigger(t => t
        .ForJob("cleanupJob")
        .WithIdentity("cleanupTrigger")
        .WithCronSchedule("0 0 2 * * ?")); // Daily at 2 AM
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
```

---

## Best Practices

### 1. Performance
- Selalu gunakan `AsNoTracking()` untuk read-only queries
- Gunakan projection untuk mengurangi data transfer
- Implement caching untuk data yang sering diakses
- Gunakan async/await untuk semua I/O operations

### 2. Security
- Jangan pernah expose database credentials di code
- Selalu validate input menggunakan FluentValidation
- Gun parameterized queries (EF Core melakukan ini secara default)
- Implement proper JWT validation dan claims checking
- Gunakan soft delete, jangan hard delete

### 3. Maintainability
- Follow naming conventions yang konsisten
- Gunakan dependency injection untuk semua dependencies
- Pisahkan business logic ke service layer
- Gunakan repository pattern untuk data access
- Dokumentasikan API dengan Swagger

### 4. Testing
- Tulis unit tests untuk business logic
- Gunakan integration tests untuk API endpoints
- Mock external dependencies (database, MQTT, email)

### 5. Deployment
- Gunakan Docker untuk consistent deployment
- Pisahkan configuration dari code dengan environment variables
- Implement health checks untuk setiap service
- Gunakan proper logging untuk monitoring

---

## Troubleshooting

### Common Issues

#### 1. Port Conflicts
```bash
# Find process using port
netstat -ano | findstr :5001
# Kill process
taskkill /PID [PID] /F
```

#### 2. Database Connection Issues
- Periksa SQL Server status
- Verifikasi connection string
- Pastikan firewall tidak memblokir port 1433

#### 3. JWT Token Issues
- Verifikasi JWT_KEY di .env dan appsettings.json
- Periksa token expiration
- Verifikasi issuer dan audience

#### 4. Docker Issues
```bash
# Clean build
docker-compose down
docker system prune -a
docker-compose build --no-cache
docker-compose up -d
```

---

## Resources

### Internal References
- **Directory.Packages.props**: Centralized package versions
- **docker-compose.yml**: Service orchestration
- **nginx.conf**: Gateway configuration
- **Shared/**: Common libraries dan utilities

### External Documentation
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT.io](https://jwt.io/)
- [MQTT Specification](http://mqtt.org/)

---

## Contact & Support

Untuk pertanyaan atau issues, silakan hubungi:
- **Team Lead**: [Nama]
- **Technical Lead**: [Nama]
- **DevOps**: [Nama]

---

*Last Updated: February 2026*
*Version: 1.0*
