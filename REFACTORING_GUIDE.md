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

**IMPORTANT**: Always inherit from `BaseFilter` for consistency:

```csharp
using Shared.Contracts.Read;

public class EntityFilter : BaseFilter
{
    // Entity-specific filters
    public int? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    // Use JsonElement for ID filters to support both single Guid and Guid array
    public JsonElement CategoryId { get; set; }
    public JsonElement FloorId { get; set; }
}
```

`BaseFilter` already includes:

- `Search`, `Page`, `PageSize`, `SortColumn`, `SortDir`, `DateFrom`, `DateTo`

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
- **CRITICAL ⚠️⚠️⚠️: FilterAsync MUST use ProjectToRead() for projection - NEVER duplicate Select() projection inside FilterAsync!**
- If entity references another master (e.g., Floor -> Building), add ownership check helper for tenant safety.

**🚨🚨🚨 CRITICAL RULE - SINGLE SOURCE OF TRUTH 🚨🚨🚨**

**NEVER duplicate the Select() projection inside FilterAsync! This creates TWO sources of truth for the same mapping.**

**WRONG ❌ (DO NOT DO THIS):**

```csharp
public async Task<(List<EntityRead> Data, int Total, int Filtered)> FilterAsync(EntityFilter filter)
{
    var query = BaseEntityQuery();
    // ... filters ...

    // ❌ WRONG: Manual sorting
    if (!string.IsNullOrEmpty(filter.SortColumn))
    {
        query = query.OrderBy($"{filter.SortColumn} {sortDir}");
    }

    // ❌ WRONG: Manual pagination + DUPLICATE Select projection
    var data = await query
        .Skip((filter.Page - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .Select(x => new EntityRead  // ❌ THIS IS WRONG! Duplicates ProjectToRead()
        {
            Id = x.Id,
            Name = x.Name,
            // ... 30+ lines of duplicate mapping ...
        })
        .ToListAsync();
}
```

**CORRECT ✅ (USE THIS PATTERN):**

```csharp
public async Task<(List<EntityRead> Data, int Total, int Filtered)> FilterAsync(EntityFilter filter)
{
    var query = BaseEntityQuery();

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

    var total = await query.CountAsync();
    var filtered = await query.CountAsync();

    // ✅ CORRECT: Use extension methods for sorting and paging
    query = query.ApplySorting(filter.SortColumn, filter.SortDir);
    query = query.ApplyPaging(filter.Page, filter.PageSize);

    // ✅ CORRECT: Use ProjectToRead() for single source of truth
    var data = await ProjectToRead(query).ToListAsync();
    return (data, total, filtered);
}
```

**Why This Rule Exists:**

1. **Single Source of Truth**: All projection logic lives in ONE place (ProjectToRead method)
2. **Maintainability**: When fields change, you only update ProjectToRead, not multiple places
3. **Consistency**: GetByIdAsync, GetAllAsync, and FilterAsync all use the exact same projection
4. **DRY Principle**: Don't Repeat Yourself - duplicate Select() is code duplication
5. **Bug Prevention**: If you add a field to ProjectToRead but forget to update FilterAsync's manual Select, responses become inconsistent

**Reference Implementations:**

- ✅ `Shared/Repositories/Repository/CardRepository.cs` - **GOLDEN STANDARD** - Complete pattern with BaseFilter inheritance, JsonElement for ID filters, ExtractIds helper
- ✅ `Shared/Repositories/Repository/RepoModel/PatrolCaseRepository.cs:198-250` - CORRECT pattern
- ✅ `Shared/Repositories/Repository/MstFloorplanRepository.cs` - CORRECT pattern

**Repository Method Naming Pattern:**

| Method | Returns | Purpose |
|--------|---------|---------|
| `GetByIdAsync(Guid id)` | `EntityRead?` | Query operations - returns Read DTO via ProjectToRead |
| `GetByIdEntityAsync(Guid id)` | `Entity?` | Update/Delete operations - returns entity object |
| `GetAllAsync()` | `IEnumerable<EntityRead>` | Query operations - returns Read DTOs |
| `FilterAsync(EntityFilter filter)` | `(List<EntityRead>, int, int)` | Query with pagination - returns Read DTOs |
| `AddAsync(Entity entity)` | `Entity` | Insert only - no validation logic |
| `UpdateAsync(Entity entity)` | `Task` | Update only - no validation logic |
| `DeleteAsync(Entity entity)` | `Task` | Delete only - accepts entity, not ID |

**Key Point:** Repository should ONLY do data access. All validation (null checks, ownership checks, business rules) belongs in the Service layer.

```csharp
// ❌ WRONG - Validation in Repository
public async Task DeleteAsync(Guid id)
{
    var entity = await GetByIdAsync(id);
    if (entity == null)
        throw new NotFoundException("Entity not found");  // ❌ Business logic in repo

    if (!isSystemAdmin && entity.ApplicationId != applicationId)
        throw new UnauthorizedAccessException();  // ❌ Validation in repo

    _context.Remove(entity);
    await _context.SaveChangesAsync();
}

// ✅ CORRECT - Data access only in Repository
public async Task DeleteAsync(Entity entity)
{
    _context.Remove(entity);  // ✅ Only data access
    await _context.SaveChangesAsync();
}

// ✅ CORRECT - Validation in Service
public async Task DeleteAsync(Guid id)
{
    var entity = await _repository.GetByIdEntityAsync(id);
    if (entity == null)
        throw new NotFoundException("Entity not found");  // ✅ Validation in service

    SetDeleteAudit(entity);
    await _repository.DeleteAsync(entity);  // Pass entity, not ID
    _audit.Deleted("Entity", id, $"Entity {entity.Name} deleted");
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

         _audit.Created(
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
  var entity = \_mapper.Map<Entity>(dto);
  entity.Id = Guid.NewGuid();
  entity.ApplicationId = AppId;
  SetCreateAudit(entity);

          await _repo.AddAsync(entity);

           _audit.Created(
              "Entity",
              entity.Id,
              "Created Entity",
              new { entity.Name }
          );

          var result = await _repo.GetByIdAsync(entity.Id);
          return result;
      }

  }

````

Ownership validation (example):
```csharp
var invalidBuildingId =
    await _repository.CheckInvalidBuildingOwnershipAsync(dto.BuildingId, AppId);
if (invalidBuildingId.Any())
    throw new UnauthorizedException(
        $"BuildingId does not belong to this Application: {string.Join(", ", invalidBuildingId)}"
    );
````

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

### 8. Relationship Validation Pattern (CheckInvalidOwnershipIdsAsync)

**CRITICAL**: When entity has relationship to another entity (e.g., Floor → Building, Floorplan → Floor), ALWAYS validate ownership using `CheckInvalidOwnershipIdsAsync` pattern.

#### Why This Pattern?

- Ensures the related entity belongs to the same Application (multi-tenancy safety)
- Prevents users from referencing entities from other applications
- Provides consistent error messages for invalid ownership

#### Repository Implementation

Create a validation helper method in the repository for each relationship:

```csharp
// In EntityRepository.cs
public async Task<IReadOnlyCollection<Guid>> CheckInvalidFloorOwnershipAsync(
    Guid floorId,
    Guid applicationId)
{
    return await CheckInvalidOwnershipIdsAsync<MstFloor>(
        new[] { floorId },
        applicationId
    );
}

public async Task<IReadOnlyCollection<Guid>> CheckInvalidBuildingOwnershipAsync(
    Guid buildingId,
    Guid applicationId)
{
    return await CheckInvalidOwnershipIdsAsync<MstBuilding>(
        new[] { buildingId },
        applicationId
    );
}
```

For multiple IDs (e.g., when using JsonElement filter):

```csharp
public async Task<IReadOnlyCollection<Guid>> CheckInvalidFloorOwnershipAsync(
    List<Guid> floorIds,
    Guid applicationId)
{
    return await CheckInvalidOwnershipIdsAsync<MstFloor>(
        floorIds,
        applicationId
    );
}
```

#### Service Implementation

Use the repository validation helper in Service Create/Update:

```csharp
// In EntityService.cs
public async Task<EntityRead> CreateAsync(EntityCreateDto dto)
{
    // 1. Check if Floor exists
    if (!await _repository.FloorExistsAsync(dto.FloorId))
        throw new NotFoundException($"Floor with id {dto.FloorId} not found");

    // 2. Check if Floor belongs to this Application
    var invalidFloorIds = await _repository.CheckInvalidFloorOwnershipAsync(dto.FloorId, AppId);
    if (invalidFloorIds.Any())
    {
        throw new UnauthorizedException(
            $"FloorId does not belong to this Application: {string.Join(", ", invalidFloorIds)}"
        );
    }

    // 3. Get the Floor to inherit ApplicationId
    var floor = await _repository.GetFloorByIdAsync(dto.FloorId);
    if (floor == null)
        throw new NotFoundException($"Floor with id {dto.FloorId} not found");

    // 4. Create entity with inherited ApplicationId
    var entity = new Entity
    {
        Id = Guid.NewGuid(),
        FloorId = floor.Id,
        ApplicationId = floor.ApplicationId,  // Inherit from Floor
        Name = dto.Name,
        Status = 1
    };

    SetCreateAudit(entity);
    await _repository.AddAsync(entity);

     _audit.Created("Entity", entity.Id, "Created entity", new { entity.Name });

    var result = await _repository.GetByIdAsync(entity.Id);
    return result!;
}
```

#### Pattern Summary for Relationships

| Step                     | Repository Method                            | Service Usage                                            |
| ------------------------ | -------------------------------------------- | -------------------------------------------------------- |
| 1. Check existence       | `FloorExistsAsync(Guid id)`                  | `if (!await _repo.FloorExistsAsync(dto.FloorId))`        |
| 2. Check ownership       | `CheckInvalidFloorOwnershipAsync(id, appId)` | `var invalid = await _repo.CheckInvalid...()`            |
| 3. Get related entity    | `GetFloorByIdAsync(Guid id)`                 | `var floor = await _repo.GetFloorByIdAsync(dto.FloorId)` |
| 4. Inherit ApplicationId | N/A                                          | `entity.ApplicationId = floor.ApplicationId`             |

#### Common Relationships & Validation

| Entity     | Related To    | Repository Method to Create           |
| ---------- | ------------- | ------------------------------------- |
| Floorplan  | Floor         | `CheckInvalidFloorOwnershipAsync`     |
| Floor      | Building      | `CheckInvalidBuildingOwnershipAsync`  |
| PatrolArea | Floor         | `CheckInvalidFloorOwnershipAsync`     |
| MaskedArea | Floorplan     | `CheckInvalidFloorplanOwnershipAsync` |
| **Card**   | **Member**    | `CheckInvalidMemberOwnershipAsync`    |
| **Card**   | **Visitor**   | `CheckInvalidVisitorOwnershipAsync`   |
| **Card**   | **CardGroup** | `CheckInvalidCardGroupOwnershipAsync` |

#### Example: CardService Ownership Validation

**CardRepository.cs** - Add ownership validation helpers:

```csharp
public async Task<IReadOnlyCollection<Guid>> CheckInvalidMemberOwnershipAsync(
    Guid memberId,
    Guid applicationId)
{
    return await CheckInvalidOwnershipIdsAsync<MstMember>(
        new[] { memberId },
        applicationId
    );
}

public async Task<IReadOnlyCollection<Guid>> CheckInvalidVisitorOwnershipAsync(
    Guid visitorId,
    Guid applicationId)
{
    return await CheckInvalidOwnershipIdsAsync<Visitor>(
        new[] { visitorId },
        applicationId
    );
}

public async Task<IReadOnlyCollection<Guid>> CheckInvalidCardGroupOwnershipAsync(
    Guid cardGroupId,
    Guid applicationId)
{
    return await CheckInvalidOwnershipIdsAsync<CardGroup>(
        new[] { cardGroupId },
        applicationId
    );
}
```

**CardService.cs** - Validate ownership in Create/Update:

```csharp
public async Task<CardRead> CreateAsync(CardCreateDto createDto)
{
    var card = _mapper.Map<Card>(createDto);

    // Ownership validation - prevents cross-tenant data access
    if (card.MemberId.HasValue)
    {
        var invalidMemberIds = await _repository.CheckInvalidMemberOwnershipAsync(
            card.MemberId.Value, AppId);

        if (invalidMemberIds.Any())
            throw new UnauthorizedException(
                $"MemberId does not belong to this Application: {string.Join(", ", invalidMemberIds)}");
    }

    if (card.VisitorId.HasValue)
    {
        var invalidVisitorIds = await _repository.CheckInvalidVisitorOwnershipAsync(
            card.VisitorId.Value, AppId);

        if (invalidVisitorIds.Any())
            throw new UnauthorizedException(
                $"VisitorId does not belong to this Application: {string.Join(", ", invalidVisitorIds)}");
    }

    if (card.CardGroupId.HasValue)
    {
        var invalidCardGroupIds = await _repository.CheckInvalidCardGroupOwnershipAsync(
            card.CardGroupId.Value, AppId);

        if (invalidCardGroupIds.Any())
            throw new UnauthorizedException(
                $"CardGroupId does not belong to this Application: {string.Join(", ", invalidCardGroupIds)}");
    }

    SetCreateAudit(card);
    await _repository.AddAsync(card);
     _audit.Created("Card", card.Id, $"Card {card.CardNumber} created");

    return await _repository.GetByIdAsync(card.Id);
}
```

**Key Points for Card Ownership Pattern:**

- Card tidak perlu inherit ApplicationId dari Member/Visitor (punya ApplicationId sendiri)
- Validasi mencegah user assign Card ke Member/Visitor/CardGroup dari application lain
- Untuk Update, cek hanya jika nilai berubah: `if (dto.MemberId.HasValue && dto.MemberId.Value != entity.MemberId)`

---

### 9. Audit Fields in Response (JsonIgnore Pattern)

**IMPORTANT**: Audit fields should NOT be visible in API response. They are already hidden in `BaseRead` using `[JsonIgnore]`:

```csharp
// Shared/Shared.Contracts/Read/BaseRead.cs
public class BaseRead
{
    [JsonPropertyOrder(-10)]
    public Guid Id { get; set; }

    [JsonIgnore]  // NOT sent in response
    public string? CreatedBy { get; set; }

    [JsonIgnore]  // NOT sent in response
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]  // NOT sent in response
    public string? UpdatedBy { get; set; }

    [JsonIgnore]  // NOT sent in response
    public DateTime UpdatedAt { get; set; }

    public int Status { get; set; }  // Included in response
    public Guid ApplicationId { get; set; }
}
```

**Key Points**:

- Audit fields are stored in DB but NOT sent in JSON response
- `Status` IS included in response (needed for frontend display)
- `ApplicationId` IS included (needed for multi-tenancy)
- Do NOT include `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt` in manual projection

---

### 10. ExtractIds Pattern for ID Filters

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
// From BaseRepository.cs
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
- Filter inherits from `BaseFilter`
- Service maps DataTablesProjectedRequest to filter
- Service uses IAuditEmitter for Create/Update/Delete operations
- Controller uses ApiResponse and no manual try/catch
- Program.cs uses RootExtension + MinLevel + middleware + AuditEmitter
- ID filters use JsonElement + ExtractIds pattern for flexibility
- **Relationship validation**: All related entities validated with `CheckInvalidOwnershipIdsAsync`
- **Audit fields hidden**: CreatedBy, CreatedAt, UpdatedBy, UpdatedAt use `[JsonIgnore]` in BaseRead
- Repository has FilterAsync returning (List, int, int)
- Projection uses manual Select only (no AutoMapper in query)
- Service maps DataTablesProjectedRequest to filter
- Service uses IAuditEmitter for Create/Update/Delete operations
- Controller uses ApiResponse and no manual try/catch
- Program.cs uses RootExtension + MinLevel + middleware + AuditEmitter
- ID filters use JsonElement + ExtractIds pattern for flexibility

---

## Reference Implementations (Golden Standard)

- **Shared/Repositories/Repository/CardRepository.cs** - **PRIMARY STANDARD** - BaseFilter inheritance, JsonElement ID filters, ExtractIds, complete pattern
- Shared/BusinessLogic.Services/Implementation/CardService.cs
- Shared/Web.API.Controllers/Controllers/CardController.cs
- Shared/Repositories/Repository/RepoModel/PatrolCaseRepository.cs
- Shared/BusinessLogic.Services/Implementation/PatrolCaseService.cs
- Shared/Web.API.Controllers/Controllers/PatrolCaseController.cs
- Shared/Web.API.Controllers/Controllers/MstDistrictController.cs
- Services.API/Patrol/Program.cs
- **Shared/Repositories/Repository/MstFloorplanRepository.cs** - Relationship validation pattern
- **Shared/BusinessLogic.Services/Implementation/MstFloorplanService.cs** - Full audit + cache + MQTT pattern
