# Unified Refactoring Guide (Current Standard)

Role: .NET Backend Developer (Clean Architecture & Performance).
Goal: Replace generic/reflection DataTables and standardize Auth + Middleware.

---

## Target Scope
Refactor a Service/Entity stack to:
1. Use Specific Repository Pattern (manual projection).
2. Use typed DataTables filters.
3. Standardize Controller responses + MinLevel auth.
4. Standardize Program.cs with RootExtension + middleware.

---

## Step-by-Step Checklist

### 1. Analyze & Prepare
Identify target entity and files:
- Shared/Shared.Contracts/Read/[Entity]Read.cs
- Shared/Shared.Contracts/[Entity]Filter.cs
- Shared/Repositories/Repository/[Entity]Repository.cs
- Shared/BusinessLogic.Services/Implementation/[Entity]Service.cs
- Shared/Web.API.Controllers/Controllers/[Entity]Controller.cs
- Services.API/[Service]/Program.cs

### 2. Create Filter DTO
Add file: Shared/Shared.Contracts/[Entity]Filter.cs
Required fields:
- Search, Page, PageSize, SortColumn, SortDir
- Entity-specific filters (e.g., DateFrom, Status, CategoryId, etc)

```csharp
public class EntityFilter
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortColumn { get; set; }
    public string? SortDir { get; set; }

    // Entity-specific filters
    public DateTime? DateFrom { get; set; }
    public int? Status { get; set; }
}
```

### 3. Create Read DTO
Add file: Shared/Shared.Contracts/Read/[Entity]Read.cs
Used for projection + response.

### 4. Repository Refactor
Update file: [Entity]Repository.cs
- Add BaseEntityQuery() if needed.
- Add ProjectToRead(...) using AsNoTracking() + manual Select.
- Add FilterAsync(FilterDto) returning (List, int, int).
- Use ApplySorting and ApplyPaging.
- If entity references another master (e.g., Floor -> Building), add ownership check helper for tenant safety.

```csharp
public async Task<(List<EntityRead> Data, int Total, int Filtered)> FilterAsync(EntityFilter filter)
{
    var query = BaseEntityQuery();
    var total = await query.CountAsync();

    // filters...
    if (!string.IsNullOrWhiteSpace(filter.Search))
    {
        var s = filter.Search.ToLower();
        query = query.Where(x => x.Name.ToLower().Contains(s));
    }

    var filtered = await query.CountAsync();

    query = query.ApplySorting(filter.SortColumn, filter.SortDir);
    query = query.ApplyPaging(filter.Page, filter.PageSize);

    var data = await ProjectToRead(query).ToListAsync();
    return (data, total, filtered);
}
```

Ownership helper (example):
```csharp
public async Task<IReadOnlyCollection<Guid>> CheckInvalidBuildingOwnershipAsync(
    Guid buildingId,
    Guid applicationId
)
{
    return await CheckInvalidOwnershipIdsAsync<MstBuilding>(
        new[] { buildingId },
        applicationId
    );
}
```

### 5. Service Refactor
Update file: [Entity]Service.cs
- Use DataTablesProjectedRequest + typed filter.
- Map request -> filter.
- Throw NotFoundException (not KeyNotFound).
- Throw BusinessException for logic errors.
- If entity depends on another master, validate ownership via repository helper before create/update.

```csharp
public async Task<object> FilterAsync(DataTablesProjectedRequest request, EntityFilter filter)
{
    filter.Page = (request.Start / request.Length) + 1;
    filter.PageSize = request.Length;
    filter.SortColumn = request.SortColumn ?? "UpdatedAt";
    filter.SortDir = request.SortDir;
    filter.Search = request.SearchValue;

    var (data, total, filtered) = await _repo.FilterAsync(filter);

    return new
    {
        draw = request.Draw,
        recordsTotal = total,
        recordsFiltered = filtered,
        data
    };
}
```

Ownership validation (example):
```csharp
var invalidBuildingId =
    await _repository.CheckInvalidBuildingOwnershipAsync(dto.BuildingId, AppId);
if (invalidBuildingId.Any())
    throw new UnauthorizedException(
        $"BuildingId does not belong to this Application: {string.Join(", ", invalidBuildingId)}"
    );
```

### 6. Controller Refactor
Update file: [Entity]Controller.cs
- Apply [MinLevel(LevelPriority.SuperAdmin)] at class level.
- Remove manual try/catch.
- Use ApiResponse helper.
- Filter endpoint must deserialize request.Filters with PropertyNameCaseInsensitive = true.

```csharp
[HttpPost("filter")]
public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
{
    var filter = new EntityFilter();
    if (request.Filters.ValueKind == JsonValueKind.Object)
    {
        filter = JsonSerializer.Deserialize<EntityFilter>(
            request.Filters.GetRawText(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? new EntityFilter();
    }

    var result = await _service.FilterAsync(request, filter);
    return Ok(ApiResponse.Paginated("Data retrieved", result));
}
```

### 7. Program.cs Refactor
Update file: Services.API/[Service]/Program.cs
Must include:
- RootExtension methods
- MinLevelHandler
- CustomExceptionMiddleware

```csharp
builder.Services.AddDbContextExtension(builder.Configuration);
builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationNewPolicies();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();

app.UseSerilogRequestLoggingExtension();
app.UseMiddleware<CustomExceptionMiddleware>();
```

---

## Additional Patterns Observed in MstFloorService (include when relevant)
1. Cache + Redis grouping:
- `Key()` + `GroupKey` pattern.
- `RemoveGroupAsync()` clears all cache keys in group.
2. MQTT refresh:
- After create/update/delete: publish `"engine/refresh/area-related"`.
3. Audit:
- Use `_audit.Created/Updated/Deleted` after DB changes.
4. Cascade delete:
- Use transaction in DeleteAsync for child entities.
- Provide `CascadeDeleteAsync` for internal-only deletions (no audit/mqtt).

---

## Definition of Done
- Repository has FilterAsync returning (List, int, int)
- Projection uses manual Select only (no AutoMapper in query)
- Service maps DataTablesProjectedRequest to filter
- Controller uses ApiResponse and no manual try/catch
- Program.cs uses RootExtension + MinLevel + middleware

---

## Reference Implementations (Golden Standard)
- Shared/Repositories/Repository/RepoModel/PatrolCaseRepository.cs
- Shared/BusinessLogic.Services/Implementation/PatrolCaseService.cs
- Shared/Web.API.Controllers/Controllers/PatrolCaseController.cs
- Shared/Web.API.Controllers/Controllers/MstDistrictController.cs
- Services.API/Patrol/Program.cs
