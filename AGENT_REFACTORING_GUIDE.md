# Agent Refactoring Instructions: DataTables & Auth Implementation

**Role**: You are an expert .NET Backend Developer specializing in Clean Architecture and Performance Optimization.
**Task**: Refactor a specific Service/Entity stack to use the **Specific Repository Pattern** and enforce **MinLevel Authorization**.

---

## 🚀 The Goal
1.  Move away from generic `BaseProjectionRepository`.
2.  Implement strongly-typed, manually projected DataTables.
3.  Standardize Controller/Service with Middleware & Helpers.

## 📋 The Procedure

### Step 1: Analyze & Prepare
*   **Target**: Identify the Entity (e.g., `MstDistrict`).
*   **Files**: Contract, Repo, Service, Controller, Program.cs.

### Step 2: Create the Filter DTO
**Location**: `Shared/Shared.Contracts/[Entity]Filter.cs`
```csharp
public class EntityFilter
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortColumn { get; set; }
    public string? SortDir { get; set; }
    
    // Specific filters
    public DateTimeOffset? DateFrom { get; set; }
    public int? Status { get; set; }
}
```

### Step 3: Refactor the Repository
**Location**: `Repositories/Repository/[Entity]Repository.cs`
*   Add `FilterAsync` with `EntityRead` projection.
*   **CRITICAL**: Use `AsNoTracking()` and `Select` (Manual Projection).

```csharp
public async Task<(List<EntityRead> Data, int Total, int Filtered)> FilterAsync(EntityFilter filter)
{
    var query = GetAllQueryable(); 
    // ... Apply filters ...
    // ... Sort & Page ...
    
    // 3. Project
    var data = await query.AsNoTracking().Select(x => new EntityRead
    {
        Id = x.Id,
        Name = x.Name
    }).ToListAsync();

    return (data, total, filtered);
}
```

### Step 4: Refactor the Service (Standardized)
**Location**: `BusinessLogic.Services/Implementation/[Entity]Service.cs`
*   Throw `NotFoundException` (not KeyNotFound).
*   Throw `BusinessException` for logic errors.
*   Map `DataTablesRequest` to `EntityFilter`.

```csharp
public async Task UpdateAsync(Guid id, Dto updateDto)
{
    var entity = await _repo.GetByIdAsync(id);
    if (entity == null) throw new NotFoundException($"Entity {id} not found");
    // ...
}
```

### Step 5: Refactor the Controller (Standardized)
**Location**: `Web.API.Controllers/Controllers/[Entity]Controller.cs`
*   Apply `[MinLevel(LevelPriority.SuperAdmin)]` at **Class Level**.
*   **Remove manual try-catch**. Let `CustomExceptionMiddleware` handle it.
*   Use `ApiResponseHelper` for all returns.

```csharp
[Route("api/[controller]")]
[ApiController]
[MinLevel(LevelPriority.SuperAdmin)] // <--- NEW RULE: Class Level
public class EntityController : ControllerBase
{
    // ...
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse.Success("Retrieved", result));
    }
}
```

### Step 6: Standardize Program.cs
**Location**: `Services.API/[Entity]/Program.cs`
*   Use `RootExtension` methods.
*   Register `MinLevelHandler`.
*   **MUST** include `app.UseMiddleware<CustomExceptionMiddleware>();`.

```csharp
// ...
builder.Services.AddDbContextExtension(builder.Configuration);
builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationNewPolicies();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();
// ...
app.UseMiddleware<CustomExceptionMiddleware>(); // <--- CRITICAL
// ...
```

---

## 🔍 Reference Implementation (Golden Standard)

*   **Repository**: `PatrolCaseRepository.cs`
*   **Controller**: `PatrolCaseController.cs` / `MstDistrictController.cs`
*   **Service**: `PatrolCaseService.cs`
*   **Program**: `Services.API/Patrol/Program.cs`
