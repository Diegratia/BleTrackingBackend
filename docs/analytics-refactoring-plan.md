# Refactoring Plan: Analytics Services & Repositories

## Context
Analytics layer di BLE Tracking Backend memiliki beberapa masalah arsitektur yang perlu diperbaeki:
1. Export logic (PDF/Excel) tersebar dan duplicate di multiple services
2. Duration formatting ada di repository layer (harusnya di service)
3. Time range handling tidak konsisten (timezone, defaults, options)
4. SQL patterns tidak standardized

**Catatan Penting**: Data TrackingTransaction bisa jutaan rows, sehingga beberapa "business logic" yang ada di repository (grouping, aggregation) sengaja ditaruh di SQL untuk performance. Ini BENAR untuk large-scale analytics.

---

## Priority 1: Export Logic Separation

### Problem
Export logic (PDF/Excel) tersebar di 3 services dengan code duplication:

| File | PDF Lines | Excel Lines |
|------|-----------|-------------|
| TrackingSessionService.cs | 347-450 | 452-548 |
| PatrolSessionAnalyticsService.cs | 282-359 | - |
| TrackingAnalyticsV2Service.cs | 99-206 | 218-326 |

### Issues
- Code duplication: Identical PDF/Excel logic di TrackingSession & TrackingAnalyticsV2
- Mixed responsibilities: Export generation mixed dengan business logic
- No abstraction: Common patterns tidak di-extract
- Hardcoded values: Column widths, page settings, styling

### Solution: Create `IReportExportService`

#### 1.1 Create Interface
**File**: `Shared/BusinessLogic.Services/Interface/IReportExportService.cs`

```csharp
public interface IReportExportService
{
    Task<byte[]> ExportToPdfAsync<T>(List<T> data, ReportMetadata metadata);
    Task<byte[]> ExportToExcelAsync<T>(List<T> data, ReportMetadata metadata);
    string GenerateReportTitle(string baseTitle, string? customTitle, string? timeRange);
    string GenerateFilterInfo(ReportFilterInfo filters);
}

// Supporting DTOs
public class ReportMetadata
{
    public string Title { get; set; } = string.Empty;
    public string? FilterInfo { get; set; }
    public List<ReportColumn> Columns { get; set; } = new();
    public ReportOrientation Orientation { get; set; } = ReportOrientation.Landscape;
    public string TimeZone { get; set; } = "UTC";
}

public class ReportColumn
{
    public string Header { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public int Width { get; set; } // Relative width for PDF
    public string Format { get; set; } = string.Empty; // Optional format string
}

public enum ReportOrientation { Portrait, Landscape }
```

#### 1.2 Create Implementation
**File**: `Shared/BusinessLogic.Services/Implementation/ReportExportService.cs`

Dependencies:
- QuestPDF (PDF)
- ClosedXML.Excel (Excel)

Key Methods:
- `ExportToPdfAsync<T>()` - Generic PDF export dengan configurable columns
- `ExportToExcelAsync<T>()` - Generic Excel export dengan configurable columns
- `GenerateReportTitle()` - Centralized title generation
- `GenerateFilterInfo()` - Centralized filter info formatting

Helper Methods (private):
- `CreatePdfDocument()` - QuestPDF document setup
- `CreatePdfTable()` - Table creation dengan dynamic columns
- `CreateExcelWorkbook()` - ClosedXML workbook setup
- `ApplyExcelStyling()` - Consistent Excel formatting

#### 1.3 Refactor Existing Services

**Files to modify**:
1. `TrackingSessionService.cs`
2. `PatrolSessionAnalyticsService.cs`
3. `TrackingAnalyticsV2Service.cs`

**Changes**:
- Remove `GeneratePdfReport()` and `GenerateExcelReport()` private methods
- Remove `GenerateReportTitle()` and `GenerateFilterInfo()` private methods
- Inject `IReportExportService`
- Create method-specific `ReportMetadata` configuration
- Call `_reportExportService.ExportToPdfAsync()` / `ExportToExcelAsync()`

**Example refactored code**:
```csharp
// Before
private byte[] GeneratePdfReport(List<VisitorSessionRead> sessions, TrackingAnalyticsFilter request)
{
    // 100+ lines of QuestPDF code...
}

// After
public async Task<byte[]> ExportVisitorSessionSummaryToPdfAsync(TrackingAnalyticsFilter request)
{
    var (sessions, _, _) = await _repository.GetVisitorSessionSummaryAsync(request);

    var metadata = new ReportMetadata
    {
        Title = _reportExportService.GenerateReportTitle(
            "Visitor Session Summary Report",
            request.ReportTitle,
            request.TimeRange),
        FilterInfo = _reportExportService.GenerateFilterInfo(new ReportFilterInfo
        {
            From = request.From,
            To = request.To,
            BuildingId = request.BuildingId,
            FloorId = request.FloorId,
            AreaId = request.AreaId,
            VisitorId = request.VisitorId
        }),
        Columns = GetSessionReportColumns(),
        Orientation = ReportOrientation.Landscape
    };

    return await _reportExportService.ExportToPdfAsync(sessions, metadata);
}

private List<ReportColumn> GetSessionReportColumns()
{
    return new()
    {
        new() { Header = "#", PropertyName = "Index", Width = 35 },
        new() { Header = "Visitor Name", PropertyName = "VisitorName", Width = 2.5f },
        // ... other columns
    };
}
```

---

## Priority 2: Move FormatDuration to Service Layer

### Problem
`FormatDuration` method ada di repository layer:

| File | Lines | Issue |
|------|-------|-------|
| TrackingSessionRepository.cs | 728-742 | Formatting di repository |
| TrackingSessionRepository.cs | 448, 686, 688 | Usage of FormatDuration |
| TrackingAnalyticsV2Repository.cs | 334-335 | Inline formatting |

### Solution

#### 2.1 Create Shared Duration Formatter

**Option A: Static Helper Class** (Recommended untuk reusability)

**File**: `Shared/Helpers/Formatting/DurationFormatter.cs`

```csharp
namespace Helpers.Formatting;

public static class DurationFormatter
{
    /// <summary>
    /// Format duration in seconds to human-readable string
    /// </summary>
    public static string FormatSeconds(double seconds)
    {
        if (seconds < 60)
            return $"{(int)seconds} seconds";

        if (seconds < 3600)
        {
            var minutes = (int)(seconds / 60);
            var remainingSeconds = (int)(seconds % 60);
            return remainingSeconds > 0
                ? $"{minutes} minutes {remainingSeconds} seconds"
                : $"{minutes} minutes";
        }

        var hours = (int)(seconds / 3600);
        var remainingMinutes = (int)((seconds % 3600) / 60);
        return remainingMinutes > 0
            ? $"{hours} hours {remainingMinutes} minutes"
            : $"{hours} hours";
    }

    /// <summary>
    /// Format duration in minutes to human-readable string
    /// </summary>
    public static string FormatMinutes(int minutes)
    {
        if (minutes < 60)
            return $"{minutes} min";

        var hours = minutes / 60;
        var mins = minutes % 60;

        if (mins == 0)
            return $"{hours} hour{(hours > 1 ? "s" : "")}";

        return $"{hours} hour{(hours > 1 ? "s" : "")} {mins} min";
    }

    /// <summary>
    /// Format TimeSpan to short format (e.g., "1h 30m", "45m", "30s")
    /// </summary>
    public static string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours}h {span.Minutes}m";
        if (span.TotalMinutes >= 1)
            return $"{span.Minutes}m";
        return $"{span.Seconds}s";
    }
}
```

**Option B: Extension Methods**

**File**: `Shared/Helpers/Formatting/DurationExtensions.cs`

```csharp
namespace Helpers.Formatting;

public static class DurationExtensions
{
    public static string ToDurationString(this double seconds)
        => DurationFormatter.FormatSeconds(seconds);

    public static string ToDurationString(this int minutes)
        => DurationFormatter.FormatMinutes(minutes);

    public static string ToShortDurationString(this TimeSpan span)
        => DurationFormatter.FormatTimeSpan(span);
}
```

#### 2.2 Update Repository to Return Raw Data

**File**: `Shared/Repositories/Repository/Analytics/TrackingSessionRepository.cs`

**Remove**:
- Lines 728-742: `FormatDuration()` private method
- Line 448: `DurationFormatted = FormatDuration(...)` → Change to return raw seconds/minutes
- Lines 686, 688: `ResponseTimeFormatted`, `ResolutionTimeFormatted` → Remove formatted versions

**Return raw numbers** instead of formatted strings:
```csharp
// Before
DurationFormatted = FormatDuration(session.DurationInMinutes.Value)

// After - remove formatted, let service handle it
// Or rename to DurationMinutes for clarity
```

#### 2.3 Update Services to Use DurationFormatter

**Files to modify**:
1. `AlarmAnalyticsIncidentService.cs` - Replace private `FormatDuration()` with `DurationFormatter`
2. `PatrolSessionAnalyticsService.cs` - Replace private `FormatDuration()` with `DurationFormatter`
3. `TrackingSessionService.cs` - Replace private `FormatDuration()` with `DurationFormatter`

**Usage**:
```csharp
using Helpers.Formatting;

// Replace existing FormatDuration calls
TotalFormatted = DurationFormatter.FormatSeconds(avgTotalSeconds),
DurationFormatted = DurationFormatter.FormatMinutes(totalMinutes),
```

---

## Priority 3: Standardize Time Range Logic

### Problem
Time range handling tidak konsisten:

| Aspect | Inconsistency |
|--------|---------------|
| **Timezone** | BaseRepository: UTC vs TrackingSessionRepository: WIB |
| **Defaults** | 7 days, 1 day, 2 hours |
| **Options** | "daily", "weekly", "monthly", "last_week", "last_month", "yearly" |

### Solution

#### 3.1 Create Centralized TimeRange Helper

**File**: `Shared/Helpers/Analytics/TimeRangeHelper.cs`

```csharp
namespace Helpers.Analytics;

public enum TimeRangeType
{
    Daily,
    Weekly,
    LastWeek,
    Monthly,
    LastMonth,
    Yearly
}

public class TimeRangeResult
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public static class TimeRangeHelper
{
    private static readonly TimeZoneInfo WibTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    /// <summary>
    /// Get time range with optional timezone conversion
    /// </summary>
    public static (DateTime? from, DateTime? to) GetTimeRange(
        string? timeRange,
        bool convertToWib = false,
        DateTime? defaultFrom = null,
        DateTime? defaultTo = null)
    {
        var (utcFrom, utcTo) = GetUtcRange(timeRange);

        if (!utcFrom.HasValue && !utcTo.HasValue)
            return (defaultFrom, defaultTo);

        if (convertToWib && utcFrom.HasValue && utcTo.HasValue)
        {
            return (
                TimeZoneInfo.ConvertTimeFromUtc(utcFrom.Value, WibTimeZone),
                TimeZoneInfo.ConvertTimeFromUtc(utcTo.Value, WibTimeZone)
            );
        }

        return (utcFrom, utcTo);
    }

    private static (DateTime? from, DateTime? to) GetUtcRange(string? timeRange)
    {
        if (string.IsNullOrWhiteSpace(timeRange))
            return (null, null);

        var now = DateTime.UtcNow;
        var today = now.Date;

        return timeRange.ToLower() switch
        {
            "daily" => (today, now),
            "weekly" => (today.AddDays(-7), now),
            "last_week" => (today.AddDays(-(int)now.DayOfWeek - 7), today.AddDays(-(int)now.DayOfWeek - 1)),
            "monthly" => (today.AddMonths(-1), now),
            "last_month" => (today.AddMonths(-1).AddDays(-today.Day + 1), today.AddDays(-today.Day)),
            "yearly" => (today.AddYears(-1), now),
            _ => (null, null)
        };
    }

    /// <summary>
    /// Get default time range if not specified
    /// </summary>
    public static (DateTime from, DateTime to) GetDefaultRange(
        TimeSpan defaultOffset,
        DateTime? explicitFrom = null,
        DateTime? explicitTo = null)
    {
        var to = explicitTo ?? DateTime.UtcNow;
        var from = explicitFrom ?? DateTime.UtcNow.Add(defaultOffset);
        return (from, to);
    }
}
```

#### 3.2 Update Repositories to Use TimeRangeHelper

**Files to modify**:
1. `TrackingSessionRepository.cs` - Remove override GetTimeRange (lines 33-79)
2. `TrackingAnalyticsV2Repository.cs` - Remove override GetTimeRange (lines 31-77)
3. `AlarmAnalyticsIncidentRepository.cs` - Use TimeRangeHelper
4. `TrackingSummaryRepository.cs` - Use TimeRangeHelper

**Example replacement**:
```csharp
// Before
var range = GetTimeRange(request.TimeRange);
var (from, to) = (
    range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7),
    range?.to ?? request.To ?? DateTime.UtcNow
);

// After
using Helpers.Analytics;

var (rangeFrom, rangeTo) = TimeRangeHelper.GetTimeRange(
    request.TimeRange,
    convertToWib: true);  // only if WIB needed
var (from, to) = (
    rangeFrom ?? request.From ?? DateTime.UtcNow.AddDays(-7),
    rangeTo ?? request.To ?? DateTime.UtcNow
);
```

---

## Priority 4: Standardize SQL Patterns

### Problem
Mixed data access patterns across analytics repositories:
- EF Core with Include/ThenInclude
- Raw SQL with SqlQueryRaw
- Dapper for specific queries

### Solution

#### 4.1 Document Recommended Pattern

**For Analytics (JUTAAN data)**:

```
1. Use raw SQL/Dapper untuk:
   - Complex aggregations
   - Large dataset queries (jutaan rows)
   - Window functions (ROW_NUMBER, RANK)
   - JSON result queries

2. Use EF Core untuk:
   - Simple queries dengan small dataset
   - Queries yang butuh Include relationships
   - Queries yang benefit dari change tracking

3. Always:
   - Use AsNoTracking() untuk read-only queries
   - Return Read DTOs via manual projection
   - Apply filters in SQL (not in-memory)
```

#### 4.2 Create Analytics Query Builder Helper (Optional)

**File**: `Shared/Helpers/Analytics/AnalyticsQueryBuilder.cs`

```csharp
namespace Helpers.Analytics;

public static class AnalyticsQueryBuilder
{
    /// <summary>
    /// Build time range filter for SQL queries
    /// </summary>
    public static string BuildTimeRangeFilter(string columnName, DateTime from, DateTime to)
    {
        return $"{columnName} >= @from AND {columnName} <= @to";
    }

    /// <summary>
    /// Build application ID filter for multi-tenancy
    /// </summary>
    public static string BuildApplicationIdFilter(string tableAlias = "a")
    {
        return $"{tableAlias}.ApplicationId = @applicationId";
    }

    /// <summary>
    /// Build status filter (exclude deleted)
    /// </summary>
    public static string BuildStatusFilter(string tableAlias = "a")
    {
        return $"{tableAlias}.Status != 0";
    }
}
```

---

## Implementation Order

### Phase 1: Foundation (Quick Wins)
1. Create `DurationFormatter` helper class
2. Create `TimeRangeHelper` helper class
3. Update services to use `DurationFormatter`

### Phase 2: Repository Cleanup
1. Remove `FormatDuration` from TrackingSessionRepository
2. Remove GetTimeRange overrides, use TimeRangeHelper
3. Update repository projections to return raw data

### Phase 3: Export Service
1. Create `IReportExportService` interface
2. Create `ReportExportService` implementation
3. Refactor TrackingSessionService
4. Refactor PatrolSessionAnalyticsService
5. Refactor TrackingAnalyticsV2Service

### Phase 4: Documentation
1. Document recommended SQL patterns
2. Add code comments explaining why aggregation is in repo (performance)

---

## Files to Create

| File | Purpose |
|------|---------|
| `Shared/Helpers/Formatting/DurationFormatter.cs` | Shared duration formatting |
| `Shared/Helpers/Formatting/DurationExtensions.cs` | Extension methods for duration |
| `Shared/Helpers/Analytics/TimeRangeHelper.cs` | Centralized time range logic |
| `Shared/Helpers/Analytics/AnalyticsQueryBuilder.cs` | SQL query builders |
| `Shared/BusinessLogic.Services/Interface/IReportExportService.cs` | Export service interface |
| `Shared/BusinessLogic.Services/Implementation/ReportExportService.cs` | Export service implementation |
| `Shared/Shared.Contracts/Reporting/ReportMetadata.cs` | Export DTOs |

## Files to Modify

| File | Changes |
|------|---------|
| `TrackingSessionService.cs` | Remove export methods, use IReportExportService |
| `PatrolSessionAnalyticsService.cs` | Remove export methods, use IReportExportService |
| `TrackingAnalyticsV2Service.cs` | Remove export methods, use IReportExportService |
| `AlarmAnalyticsIncidentService.cs` | Use DurationFormatter |
| `TrackingSessionRepository.cs` | Remove FormatDuration, GetTimeRange override |
| `TrackingAnalyticsV2Repository.cs` | Remove GetTimeRange override |
| `TrackingSummaryRepository.cs` | Use TimeRangeHelper |
| `AlarmAnalyticsIncidentRepository.cs` | Use TimeRangeHelper |

---

## Verification

### Test Export Service
1. Create unit test for PDF generation with sample data
2. Create unit test for Excel generation with sample data
3. Verify existing export endpoints still work

### Test Duration Formatting
1. Verify all duration formats are consistent
2. Check no formatting remains in repository layer

### Test Time Range
1. Verify all time range options work correctly
2. Check timezone handling (UTC vs WIB)
3. Verify default ranges are applied correctly

### Test Existing Endpoints
1. Run through analytics API endpoints
2. Verify no breaking changes
3. Check performance for large datasets

---

## Notes

### Why Keep Some Logic in Repository?
Dengan context data TrackingTransaction = jutaan rows:
- SQL aggregation lebih efisien daripada in-memory C#
- Grouping di database mengurangi data transfer
- Window functions di SQL optimal untuk large dataset

### What Should NOT Be in Repository?
- String formatting (presentation concern)
- Business rule validation (service concern)
- Caching logic (service concern)
- Export generation (separate service concern)
