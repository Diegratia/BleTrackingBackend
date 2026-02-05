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
using System.Text.Json;

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

    // Use JsonElement for ID filters to support both single Guid and Guid array
    public JsonElement CategoryId { get; set; }
}
```

**Important**: For ID filters that need to support both single value and array, use `JsonElement` type and the `ExtractIds` helper method from `BaseRepository` (see section 8 for details).

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

    // Use ExtractIds for ID filters (supports both single Guid and array)
    var categoryIds = ExtractIds(filter.CategoryId);
    if (categoryIds.Count > 0)
        query = query.Where(x => categoryIds.Contains(x.CategoryId));

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
- Inherit from `BaseService`
- Inject `IAuditEmitter` for audit logging
- Use DataTablesProjectedRequest + typed filter.
- Map request -> filter.
- Throw NotFoundException (not KeyNotFound).
- Throw BusinessException for logic errors.
- If entity depends on another master, validate ownership via repository helper before create/update.
- Call `_audit.Created/Updated/Deleted` after DB operations.
- **CRITICAL**: For GetById/GetAll that return Read DTOs from repository, DO NOT use mapper - return directly.

```csharp
public class EntityService : BaseService, IEntityService
{
    private readonly EntityRepository _repo;
    private readonly IMapper _mapper;
    private readonly IAuditEmitter _audit;

    public EntityService(
        EntityRepository repo,
        IMapper mapper,
        IAuditEmitter audit,
        IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repo = repo;
        _mapper = mapper;
        _audit = audit;
    }

    // CORRECT: Return Read DTO directly from repository (NO MAPPER)
    public async Task<EntityRead> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException($"Entity with id {id} not found");
        return entity;  // Direct return, no mapper needed
    }

    // CORRECT: Return Read DTO list directly
    public async Task<IEnumerable<EntityRead>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return entities;  // Direct return
    }

    // CORRECT: Use mapper for Create (DTO → Entity)
    public async Task<EntityDto> CreateAsync(EntityCreateDto dto)
    {
        var entity = _mapper.Map<Entity>(dto);  // Mapper for DTO → Entity
        entity.Id = Guid.NewGuid();
        entity.ApplicationId = AppId;
        SetCreateAudit(entity);

        await _repo.AddAsync(entity);

        await _audit.Created(
            "Entity",
            entity.Id,
            "Created Entity",
            new { entity.Name }
        );

        // Option 1: Return Read DTO directly (no mapper)
        var result = await _repo.GetByIdAsync(entity.Id);
        return result;  // or _mapper.Map<EntityDto>(result)
    }

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
            data  // Already EntityRead[] from repository
        };
    }
}
```

#### Interface Service Return Types:

```csharp
public interface IEntityService
{
    // Use Read DTO for query operations (no mapper in service)
    Task<EntityRead> GetByIdAsync(Guid id);           // Not EntityDto
    Task<IEnumerable<EntityRead>> GetAllAsync();      // Not IEnumerable<EntityDto>

    // Use Entity DTO for create/update (mapper needed for DTO → Entity)
    Task<EntityDto> CreateAsync(EntityCreateDto dto);
    Task UpdateAsync(Guid id, EntityUpdateDto dto);
    Task DeleteAsync(Guid id);

    // Filter returns object with data array
    Task<object> FilterAsync(DataTablesProjectedRequest request, EntityFilter filter);
}
```

#### Key Points - Return Type Pattern:

1. **Repository returns Read DTO** → Service returns directly (NO MAPPER)
2. **Create/Update needs Entity** → Use mapper for DTO → Entity conversion
3. **Interface uses Read DTO** for GetById/GetAll return types
4. **No double mapping** → Entity → Read → DTO is redundant

**Why?**
- Repository's `ProjectToRead()` already does the projection (Entity → Read DTO)
- Adding mapper again would be: Entity → Read DTO → Entity DTO (inefficient)
- Read operations should return Read DTOs for consistency
        var entity = _mapper.Map<Entity>(dto);
        entity.Id = Guid.NewGuid();
        entity.ApplicationId = AppId;
        SetCreateAudit(entity);

        await _repo.AddAsync(entity);

        await _audit.Created(
            "Entity",
            entity.Id,
            "Created Entity",
            new { entity.Name }
        );

        var result = await _repo.GetByIdAsync(entity.Id);
        return result;
    }
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
- AuditEmitter (for audit logging)

```csharp
builder.Services.AddDbContextExtension(builder.Configuration);
builder.Services.AddJwtAuthExtension(builder.Configuration);
builder.Services.AddAuthorizationNewPolicies();
builder.Services.AddSingleton<IAuthorizationHandler, MinLevelHandler>();
builder.Services.AddScoped<IAuditEmitter, AuditEmitter>();

// Optional: MQTT if needed
// builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
// builder.Services.AddHostedService<MqttRecoveryService>();

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

## Advanced Patterns

### 8. ExtractIds Pattern for ID Filters

When filtering by ID fields that need to support both single values and arrays, use the `ExtractIds` helper method from `BaseRepository`.

#### Why This Pattern?
- Frontend may send single ID as string: `"integrationId": "123-456..."`
- Or multiple IDs as array: `"integrationId": ["123-456...", "789-012..."]`
- Using `JsonElement` + `ExtractIds` handles both cases seamlessly

#### Implementation in Filter DTO:
```csharp
using System.Text.Json;

public class EntityFilter
{
    // Use JsonElement to support both single Guid and Guid array
    public JsonElement CategoryId { get; set; }
    public JsonElement BuildingId { get; set; }
}
```

#### Implementation in Repository:
```csharp
// ExtractIds is inherited from BaseRepository
public async Task<(List<EntityRead> Data, int Total, int Filtered)> FilterAsync(
    EntityFilter filter)
{
    var query = BaseEntityQuery();

    // Use ExtractIds to handle both single Guid and Guid array
    var categoryIds = ExtractIds(filter.CategoryId);
    if (categoryIds.Count > 0)
        query = query.Where(x => categoryIds.Contains(x.CategoryId));

    // For nullable foreign keys, check HasValue first
    var buildingIds = ExtractIds(filter.BuildingId);
    if (buildingIds.Count > 0)
        query = query.Where(x => x.BuildingId.HasValue && buildingIds.Contains(x.BuildingId.Value));

    // ... rest of filtering logic
}
```

#### ExtractIds Method (in BaseRepository):
The `ExtractIds` method automatically handles:
- String → Single Guid
- Array of strings → Multiple Guids
- Invalid values → Filtered out
- Empty/null → Returns empty list

```csharp
// From BaseRepository.cs (lines 229-254)
public static List<Guid> ExtractIds(JsonElement element)
{
    var ids = new List<Guid>();

    if (element.ValueKind == JsonValueKind.String)
    {
        var raw = element.GetString();
        if (!string.IsNullOrWhiteSpace(raw) && Guid.TryParse(raw, out var singleId))
            ids.Add(singleId);
    }
    else if (element.ValueKind == JsonValueKind.Array)
    {
        foreach (var el in element.EnumerateArray())
        {
            if (el.ValueKind != JsonValueKind.String) continue;
            var raw = el.GetString();
            if (string.IsNullOrWhiteSpace(raw)) continue;
            if (Guid.TryParse(raw, out var parsed))
                ids.Add(parsed);
        }
    }

    return ids;
}
```

---

## Definition of Done
- Repository has FilterAsync returning (List, int, int)
- Projection uses manual Select only (no AutoMapper in query)
- Service maps DataTablesProjectedRequest to filter
- Service uses IAuditEmitter for Create/Update/Delete operations
- Controller uses ApiResponse and no manual try/catch
- Program.cs uses RootExtension + MinLevel + middleware + AuditEmitter
- ID filters use JsonElement + ExtractIds pattern for flexibility

---

## Reference Implementations (Golden Standard)
- Shared/Repositories/Repository/RepoModel/PatrolCaseRepository.cs
- Shared/BusinessLogic.Services/Implementation/PatrolCaseService.cs
- Shared/Web.API.Controllers/Controllers/PatrolCaseController.cs
- Shared/Web.API.Controllers/Controllers/MstDistrictController.cs
- Services.API/Patrol/Program.cs
