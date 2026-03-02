# Filter Pattern Guide

This guide documents the two main filter patterns used in BLE Tracking Backend:

1. **DataTables Filter Pattern** - For standard entity listing with pagination
2. **Analytics Filter Pattern** - For analytics, reporting, and complex queries

---

## Pattern 1: DataTables Filter (Standard Entity Listing)

**Use Case:** Standard CRUD entity listing with DataTables pagination

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Controller Layer                              │
│  POST /api/[Entity]/filter                                          │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ DataTablesProjectedRequest                                     │  │
│  │  - Draw (for echo)                                             │  │
│  │  - Start, Length (pagination)                                  │  │
│  │  - SortColumn, SortDir                                         │  │
│  │  - SearchValue                                                 │  │
│  │  - DateFilters (Dictionary<string, DateFilter>)               │  │
│  │  - Filters (JsonElement - for entity-specific filters)        │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                              ↓                                     │
│  Deserialize Filters → EntityFilter                                   │
└─────────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────────┐
│                         Service Layer                                │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ FilterAsync(DataTablesProjectedRequest, EntityFilter)         │  │
│  │  1. Map request → filter                                       │  │
│  │  2. Call repository.FilterAsync(filter)                        │  │
│  │  3. Return DataTables response                                 │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────────┐
│                        Repository Layer                              │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ FilterAsync(EntityFilter filter)                               │  │
│  │  1. Apply search                                               │  │
│  │  2. Apply entity-specific filters                              │  │
│  │  3. Apply sorting (ApplySorting)                               │  │
│  │  4. Apply paging (ApplyPaging)                                 │  │
│  │  5. Project to Read DTO (ProjectToRead)                       │  │
│  │  Returns: (List<Read> Data, int Total, int Filtered)          │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### Filter DTO Structure

```csharp
// Shared/Shared.Contracts/[Entity]Filter.cs
using System.Text.Json;
using Shared.Contracts.Read;

public class EntityFilter : BaseFilter  // ✅ IMPORTANT: Inherit from BaseFilter
{
    // BaseFilter includes: Search, Page, PageSize, SortColumn, SortDir, DateFrom, DateTo

    // ID filters with JsonElement (supports both single Guid and array)
    public JsonElement CategoryId { get; set; }
    public JsonElement FloorId { get; set; }

    // Entity-specific filters
    public int? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SpecificField { get; set; }
}
```

### Repository Implementation

```csharp
// Shared/Repositories/Repository/[Entity]Repository.cs
public async Task<(List<EntityRead> Data, int Total, int Filtered)> FilterAsync(
    EntityFilter filter)
{
    var query = BaseEntityQuery();

    // 1. Apply search
    if (!string.IsNullOrWhiteSpace(filter.Search))
    {
        var s = filter.Search.ToLower();
        query = query.Where(x => x.Name.ToLower().Contains(s));
    }

    // 2. Apply entity-specific filters
    if (filter.Status.HasValue)
        query = query.Where(x => x.Status == filter.Status.Value);

    // Use ExtractIds for ID filters (supports both single Guid and array)
    var categoryIds = ExtractIds(filter.CategoryId);
    if (categoryIds.Count > 0)
        query = query.Where(x => categoryIds.Contains(x.CategoryId));

    // Date range filter
    if (filter.DateFrom.HasValue)
        query = query.Where(x => x.CreatedAt >= filter.DateFrom.Value);
    if (filter.DateTo.HasValue)
        query = query.Where(x => x.CreatedAt <= filter.DateTo.Value);

    var total = await query.CountAsync();
    var filtered = await query.CountAsync();

    // 3. Apply sorting and paging
    query = query.ApplySorting(filter.SortColumn, filter.SortDir);
    query = query.ApplyPaging(filter.Page, filter.PageSize);

    // 4. Project to Read DTO (single source of truth)
    var data = await ProjectToRead(query).ToListAsync();

    return (data, total, filtered);
}
```

### Service Implementation

```csharp
// Shared/BusinessLogic.Services/Implementation/[Entity]Service.cs
public async Task<object> FilterAsync(DataTablesProjectedRequest request, EntityFilter filter)
{
    // Map DataTables request to filter
    filter.Page = (request.Start / request.Length) + 1;
    filter.PageSize = request.Length;
    filter.SortColumn = request.SortColumn ?? "UpdatedAt";
    filter.SortDir = request.SortDir ?? "desc";
    filter.Search = request.SearchValue;

    // Handle date filters from request
    if (request.DateFilters != null)
    {
        if (request.DateFilters.TryGetValue("DateFrom", out var dateFilter))
        {
            filter.DateFrom = dateFilter.DateFrom;
            filter.DateTo = dateFilter.DateTo;
        }
    }

    var (data, total, filtered) = await _repository.FilterAsync(filter);

    return new
    {
        draw = request.Draw,
        recordsTotal = total,
        recordsFiltered = filtered,
        data
    };
}
```

### Controller Implementation

```csharp
// Shared/Web.API.Controllers/Controllers/[Entity]Controller.cs
[HttpPost("filter")]
public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
{
    // Deserialize filters with case-insensitive option
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

### Response Format

```json
{
  "success": true,
  "message": "Data retrieved",
  "data": {
    "draw": 1,
    "recordsTotal": 100,
    "recordsFiltered": 50,
    "data": [
      {
        "id": "guid",
        "name": "Entity Name",
        "status": 1,
        "applicationId": "guid"
      }
    ]
  }
}
```

---

## Pattern 2: Analytics Filter (Reports & Analytics)

**Use Case:** Analytics dashboards, reports, grouped data, export functionality

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Controller Layer                              │
│  GET /api/[Analytics]/report?from=...&to=...&buildingId=...           │
│  GET /api/[Analytics]/export/pdf?...                                   │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ AnalyticsFilter (from query string or body)                   │  │
│  │  - Pagination: Draw, Start, Length                             │  │
│  │  - Time: From, To, TimeRange                                   │  │
│  │  - Entity IDs: BuildingId, FloorId, AreaId, etc.              │  │
│  │  - Options: IncludeSummary, IncludeVisualPaths, etc.          │  │
│  │  - Export: ReportTitle, Timezone, ExportType                  │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────────┐
│                         Service Layer                                │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ GetReportAsync(AnalyticsFilter filter)                        │  │
│  │  1. Call repository for raw data                               │  │
│  │  2. Group/Aggregate data                                       │  │
│  │  3. Build summary statistics                                   │  │
│  │  4. Apply timezone conversion                                  │  │
│  │  5. Apply pagination (after grouping!)                         │  │
│  │  6. Return custom response object                              │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                               ↓                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ ExportToPdfAsync(AnalyticsFilter filter)                      │  │
│  │  ExportToExcelAsync(filter)                                    │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────────┐
│                        Repository Layer                              │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ GetReportDataAsync(AnalyticsFilter filter)                    │  │
│  │  - Returns raw data for processing                             │  │
│  │  - No pagination (applied in service after grouping)           │  │
│  │  - Optional: Returns (Data, Total, Filtered)                   │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### Filter DTO Structure

```csharp
// Shared/Shared.Contracts/[Entity]AnalyticsFilter.cs
using System;

namespace Shared.Contracts
{
    public class EntityAnalyticsFilter
    {
        // =====================================================
        // DATATABLES PAGINATION (embedded, no BaseFilter)
        // =====================================================

        public int Draw { get; set; } = 1;
        public int Start { get; set; } = 0;
        public int Length { get; set; } = 10;
        public string? Search { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }

        // =====================================================
        // TIME FILTERS
        // =====================================================

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? TimeRange { get; set; }  // "today", "daily", "weekly", "monthly"

        // =====================================================
        // ENTITY FILTERS (Guid?, not JsonElement)
        // =====================================================

        public Guid? BuildingId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? SecurityId { get; set; }
        public Guid? RouteId { get; set; }

        // =====================================================
        // STRING/ENUM FILTERS
        // =====================================================

        public string? PersonType { get; set; }  // "visitor", "member", "all"
        public string? Status { get; set; }

        // =====================================================
        // INCLUDE OPTIONS (for shaping response)
        // =====================================================

        public bool IncludeSummary { get; set; } = false;
        public bool IncludeDetails { get; set; } = true;
        public bool IncludeVisualPaths { get; set; } = false;
        public int? MaxItems { get; set; }

        // =====================================================
        // EXPORT OPTIONS
        // =====================================================

        public string? ReportTitle { get; set; }
        public string? ExportType { get; set; }  // "pdf", "excel"
        public string? Timezone { get; set; }  // "WIB", "UTC+7", "Asia/Jakarta"
    }
}
```

### Service Implementation Pattern

```csharp
// Shared/BusinessLogic.Services/Implementation/Analytics/[Entity]AnalyticsService.cs
public async Task<CustomResponse> GetReportAsync(EntityAnalyticsFilter request)
{
    // 1. Get raw data from repository (no pagination yet!)
    var (data, total, filtered) = await _repository.GetReportDataAsync(request);

    // 2. Apply timezone conversion if needed
    var tz = TimezoneHelper.Resolve(request.Timezone);
    if (tz.Id != TimeZoneInfo.Utc.Id)
    {
        foreach (var item in data)
        {
            item.Timestamp = TimezoneHelper.ConvertFromUtc(item.Timestamp, tz);
        }
    }

    // 3. Group/Aggregate data BEFORE pagination
    var groupedData = GroupByPerson(data);  // Custom grouping logic

    // 4. Count records AFTER grouping (for DataTables response)
    var recordsFiltered = groupedData.Count;

    // 5. Apply pagination ON GROUPED DATA
    var start = request.Start >= 0 ? request.Start : 0;
    var length = request.Length > 0 ? request.Length : 10;
    var pagedData = groupedData
        .Skip(start)
        .Take(length)
        .ToList();

    // 6. Build response
    var response = new CustomResponse
    {
        Draw = request.Draw,
        RecordsTotal = total,
        RecordsFiltered = recordsFiltered,
        Data = pagedData
    };

    // 7. Optional: Include summary (calculate from ALL data, not paged)
    if (request.IncludeSummary)
    {
        response.Summary = BuildSummary(data);
    }

    // 8. Optional: Include visual paths
    if (request.IncludeVisualPaths)
    {
        response.VisualPaths = await BuildVisualPathsAsync(request);
    }

    return response;
}

public async Task<byte[]> ExportToPdfAsync(EntityAnalyticsFilter request)
{
    // Get ALL data (no pagination)
    var (data, _, _) = await _repository.GetReportDataAsync(request);
    return GeneratePdfReport(data, request);
}
```

### Response Format

```json
{
  "draw": 1,
  "recordsTotal": 500,
  "recordsFiltered": 120,
  "data": [
    {
      "personId": "guid",
      "personName": "John Doe",
      "totalSessions": 5,
      "totalDuration": "2 hours 30 min",
      "areasVisited": ["Lobby", "Office A", "Cafeteria"],
      "sessions": [
        { "area": "Lobby", "enterTime": "2025-02-19T08:00:00", "exitTime": "2025-02-19T08:30:00" }
      ]
    }
  ],
  "summary": {
    "uniquePersons": 25,
    "totalSessions": 120,
    "averageDuration": "45 min"
  },
  "visualPaths": {
    "floorplan-1": {
      "floorplanId": "guid",
      "points": [
        { "x": 100, "y": 200, "time": "2025-02-19T08:00:00", "person": "John Doe" }
      ]
    }
  }
}
```

---

## Key Differences Summary

| Aspect | DataTables Pattern | Analytics Pattern |
|--------|-------------------|-------------------|
| **Filter Inheritance** | `: BaseFilter` | Standalone class |
| **Pagination Location** | Filter has Page/PageSize | Filter has Start/Length |
| **ID Filter Type** | `JsonElement` (supports array) | `Guid?` (single value) |
| **ID Filter Handler** | `ExtractIds()` helper | Direct `==` comparison |
| **Response Structure** | Standard DataTables | Custom grouped/aggregated |
| **Pagination Applied** | In repository (SQL level) | In service (after grouping) |
| **Grouping** | No grouping | Often groups by entity |
| **Summary/Stats** | Not included | Often included |
| **Export** | Separate endpoint | Built-in options |
| **Timezone** | Usually UTC | Optional conversion |
| **Use Case** | Standard CRUD listing | Reports, analytics, dashboards |

---

## When to Use Which Pattern?

### Use DataTables Pattern When:
- ✅ Standard entity listing (CRUD)
- ✅ Simple filtering and sorting
- ✅ No data transformation needed
- ✅ Direct database pagination (efficient)
- ✅ Examples: `PatrolRoute`, `PatrolArea`, `PatrolAssignment`, `PatrolCase`

### Use Analytics Pattern When:
- ✅ Need grouping/aggregation
- ✅ Need summary statistics
- ✅ Need data transformation (timezone, grouping)
- ✅ Export functionality (PDF/Excel)
- ✅ Complex visualizations (charts, paths)
- ✅ Examples: `TrackingSession`, `AlarmTriggers timeline`, `PatrolSession reports`

---

## Example: Patrol Reporting

For **Patrol Reporting**, we need **Analytics Pattern** because:

1. Need to group sessions by security personnel
2. Calculate completion rates and metrics
3. Include incident data within timeline
4. Support PDF export
5. Timezone-aware timestamps

### Filter DTO

```csharp
// Shared/Shared.Contracts/PatrolSessionAnalyticsFilter.cs
public class PatrolSessionAnalyticsFilter
{
    // DataTables pagination
    public int Draw { get; set; } = 1;
    public int Start { get; set; } = 0;
    public int Length { get; set; } = 10;

    // Time filters
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? TimeRange { get; set; }

    // Entity filters
    public Guid? SecurityId { get; set; }
    public Guid? RouteId { get; set; }
    public Guid? AreaId { get; set; }
    public bool? IsCompleted { get; set; }

    // Include options
    public bool IncludeTimeline { get; set; } = true;
    public bool IncludeIncidents { get; set; } = true;
    public bool IncludeSummary { get; set; } = false;

    // Export options
    public string? ReportTitle { get; set; }
    public string? Timezone { get; set; }
}
```

### Expected Response

```json
{
  "draw": 1,
  "recordsTotal": 200,
  "recordsFiltered": 45,
  "data": [
    {
      "sessionId": "guid",
      "securityName": "John Doe",
      "routeName": "Morning Route A",
      "startedAt": "2025-02-19T08:00:00",
      "endedAt": "2025-02-19T09:30:00",
      "duration": "1h 30m",
      "metrics": {
        "totalCheckpoints": 5,
        "completedCheckpoints": 5,
        "completionPercentage": 100,
        "totalIncidents": 1
      },
      "timeline": [
        { "stage": "started", "timestamp": "2025-02-19T08:00:00" },
        { "stage": "checkpoint_1", "stageName": "Lobby", "timestamp": "2025-02-19T08:05:00", "isDelayed": false },
        { "stage": "completed", "timestamp": "2025-02-19T09:30:00" }
      ],
      "incidents": [
        { "title": "Suspicious Vehicle", "threatLevel": "Medium", "areaName": "Parking" }
      ]
    }
  ],
  "summary": {
    "totalSessions": 45,
    "completedOnTime": 40,
    "averageCompletionRate": 95.5,
    "totalIncidents": 8
  }
}
```
