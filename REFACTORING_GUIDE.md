# Agent Refactoring Instructions: DataTables Implementation

**Role**: You are an expert .NET Backend Developer specializing in Clean Architecture and Performance Optimization.
**Task**: Refactor a specific Service/Entity stack to use the **Specific Repository Pattern** for DataTables/Grid handling.

---

## 🚀 The Goal
Move away from the generic `BaseProjectionRepository` or `CommonService` reflection-based logic.
Implement a strongly-typed, manually projected, optimized query path for DataTables.

## 📋 The Procedure

Execute the following steps in order. Do not skip steps.

### Step 1: Analyze & Prepare
1.  Identify the Target Entity (e.g., `MstBuilding`).
2.  Locate the files:
    *   **Contract**: `Shared/Shared.Contracts/Read/[Entity]Read.cs`
    *   **Repo**: `Shared/Repositories/Repository/[Entity]Repository.cs`
    *   **Service**: `Shared/BusinessLogic.Services/Implementation/[Entity]Service.cs`
    *   **Controller**: `Shared/Web.API.Controllers/Controllers/[Entity]Controller.cs`

### Step 2: Create the Filter DTO
**Location**: `Shared/Shared.Contracts/[Entity]Filter.cs` (Create if missing)
*   Must be a `public class`.
*   Must include standard DataTables params (`Page`, `PageSize`, `SortColumn`, `SortDir`, `Search`).
*   Must include specific entity filters (e.g., `DateFrom`, `Status`, `CategoryId`).

```csharp
public class EntityFilter
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortColumn { get; set; }
    public string? SortDir { get; set; }
    
    // Add specific filters here
    public DateTime? DateFrom { get; set; }
    public int? Status { get; set; }
}
```

### Step 3: Refactor the Repository
**Location**: `[Entity]Repository.cs`
1.  Add `FilterAsync` method.
2.  **CRITICAL**: Use `AsNoTracking()` and `Select` (Manual Projection).
3.  **FORBIDDEN**: Do NOT use AutoMapper `ProjectTo` or `_mapper.Map` inside the query loop.
4.  Use `QueryableExtensions` for sorting and paging.

```csharp
public async Task<(List<EntityRead> Data, int Total, int Filtered)> FilterAsync(EntityFilter filter)
{
    var query = BaseEntityQuery(); // Or _context.Entities.AsNoTracking()
    var total = await query.CountAsync();

    // 1. Filter
    if (!string.IsNullOrWhiteSpace(filter.Search)) {
        var s = filter.Search.ToLower();
        query = query.Where(x => x.Name.ToLower().Contains(s));
    }
    // ... apply other filters ...

    var filtered = await query.CountAsync();

    // 2. Sort & Page
    query = query.ApplySorting(filter.SortColumn, filter.SortDir);
    query = query.ApplyPaging(filter.Page, filter.PageSize);

    // 3. Project
    var data = await query.AsNoTracking().Select(x => new EntityRead
    {
        Id = x.Id,
        Name = x.Name,
        RelatedName = x.RelatedEntity.Name, // Cheap join
        CreatedAt = x.CreatedAt
    }).ToListAsync();

    return (data, total, filtered);
}
```

### Step 4: Refactor the Service
**Location**: `[Entity]Service.cs`
1.  Implement `FilterAsync` that accepts `DataTablesProjectedRequest` + `EntityFilter`.
2.  Map generic request params to the typed filter.
3.  Call the Repository.

```csharp
public async Task<object> FilterAsync(DataTablesProjectedRequest request, EntityFilter filter)
{
    // Map Standard Params
    filter.Page = (request.Start / request.Length) + 1;
    filter.PageSize = request.Length;
    filter.SortColumn = request.SortColumn;
    filter.SortDir = request.SortDir;
    filter.Search = request.SearchValue;

    // Handle complex logic if needed (e.g., Parsing TimeReport shortcuts)

    var (data, total, filtered) = await _repo.FilterAsync(filter);

    return new
    {
        draw = request.Draw,
        recordsTotal = total,
        recordsFiltered = filtered,
        data = data
    };
}
```

### Step 5: Refactor the Controller
**Location**: `[Entity]Controller.cs`
1.  Update the `[HttpPost("filter")]` endpoint.
2.  Use `JsonSerializer` to deserialize the `Filters` property safely.

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

---

## ⚡ Critical Rules & "Gotchas"

1.  **Manual Select vs AutoMapper**: Prioritize **Manual Select** for the `FilterAsync` method. It prevents N+1 issues and over-fetching implicitly.
2.  **Extensions**: Ensure `Repositories.Extensions.QueryableExtensions` is imported (`using Repositories.Extensions;`).
3.  **JSON Deserialization**: Always use `PropertyNameCaseInsensitive = true` to handle frontend inconsistencies.
4.  **BaseEntityQuery**: If the repository has a `BaseEntityQuery()` method that handles Multi-tenancy/RBAC, USE IT. Do not re-write the `Where(x => ApplicationId == ...)` logic manually if it exists.

## ✅ Definition of Done

*   [ ] Repository has `FilterAsync` returning `(List, int, int)`.
*   [ ] Repository uses `Select` extraction (no AutoMapper in SQL gen).
*   [ ] Service maps `DataTablesRequest` to `FilterDto`.
*   [ ] Controller deserializes JSON cleanly.

## 🔍 Reference Implementation (Golden Standard)

If you are unsure about any step, refer to **`PatrolCase`** implementation in the current codebase. It is the verified "Golden Standard" for this pattern.

*   **Repository**: `Repositories/Repository/RepoModel/PatrolCaseRepository.cs` (Method `FilterAsync`)
*   **Service**: `BusinessLogic.Services/Implementation/PatrolCaseService.cs` (Method `FilterAsync`)
*   **Filter DTO**: `Shared.Contracts/PatrolCase.cs` (Class `PatrolCaseFilter`)
*   **Controller**: `Web.API.Controllers/Controllers/PatrolCaseController.cs` (Endpoint `Filter`)

Use these files to resolve ambiguities.

