# Analytics Services Refactoring Plan
## Standardize to ApiResponse with DTOs

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Current State Analysis](#current-state-analysis)
3. [Endpoint Categorization & Naming](#endpoint-categorization--naming)
4. [Universal Filter Design](#universal-filter-design)
5. [Proposed Solution](#proposed-solution)
6. [Implementation Guide](#implementation-guide)
7. [Testing & Verification](#testing--verification)
8. [Rollback Plan](#rollback-plan)

---

## Executive Summary

**Objective**: Standardize all analytics services to use `ApiResponse` helper with strongly-typed DTOs for consistent API responses.

**Current State**: Mixed response patterns - some services use `ResponseCollection<T>`, some use `ApiResponse.Success()`, some return anonymous objects.

**Target State**: All analytics services use `ApiResponse` with well-defined DTOs.

**Impact**: 2 main services (AlarmAnalyticsIncident, TrackingAnalytics), ~15 endpoints, 0 controller changes needed.

---

## Current State Analysis

### Services Overview

| Service | Location | Endpoints | Current Response Pattern |
|---------|----------|-----------|-------------------------|
| AlarmAnalyticsIncidentService | `Shared/BusinessLogic.Services/Implementation/Analytics/` | 7 endpoints | Mixed: 6× ResponseCollection, 1× object |
| TrackingAnalyticsService | `Shared/BusinessLogic.Services/Implementation/Analytics/` | 10 endpoints | Mixed: 8× ResponseCollection, 2× object |

### Detailed Method Breakdown

#### AlarmAnalyticsIncidentService (7 methods)

| Method | Line | Current Return | Current Pattern |
|--------|------|----------------|-----------------|
| `GetAreaSummaryChartAsync` | 46 | `Task<object>` | ✅ Already uses ApiResponse pattern |
| `GetDailySummaryAsync` | 101 | `Task<ResponseCollection<AlarmDailySummaryDto>>` | ❌ Needs refactor |
| `GetStatusSummaryAsync` | 116 | `Task<ResponseCollection<AlarmStatusSummaryDto>>` | ❌ Needs refactor |
| `GetVisitorSummaryAsync` | 131 | `Task<ResponseCollection<AlarmVisitorSummaryDto>>` | ❌ Needs refactor |
| `GetBuildingSummaryAsync` | 146 | `Task<ResponseCollection<AlarmBuildingSummaryDto>>` | ❌ Needs refactor |
| `GetHourlyStatusSummaryAsync` | 160 | `Task<ResponseCollection<AlarmHourlyStatusSummaryDto>>` | ❌ Needs refactor |
| *(Commented)* `GetAreaSummaryAsync` | 31-44 | N/A | Remove commented code |

#### TrackingAnalyticsService (10 methods)

| Method | Line | Current Return | Current Pattern |
|--------|------|----------------|-----------------|
| `GetAreaSummaryAsync` | 32 | `Task<ResponseCollection<TrackingAreaSummaryDto>>` | ❌ Needs refactor |
| `GetDailySummaryAsync` | 47 | `Task<ResponseCollection<TrackingDailySummaryDto>>` | ❌ Needs refactor |
| `GetReaderSummaryAsync` | 62 | `Task<ResponseCollection<TrackingReaderSummaryDto>>` | ❌ Needs refactor |
| `GetVisitorSummaryAsync` | 77 | `Task<ResponseCollection<TrackingVisitorSummaryDto>>` | ❌ Needs refactor |
| `GetBuildingSummaryAsync` | 92 | `Task<ResponseCollection<TrackingBuildingSummaryDto>>` | ❌ Needs refactor |
| `GetTrackingMovementByCardIdAsync` | 107 | `Task<ResponseCollection<TrackingMovementRM>>` | ❌ Needs refactor |
| `GetHeatmapDataAsync` | 124 | `Task<ResponseCollection<TrackingHeatmapRM>>` | ❌ Needs refactor |
| `GetCardSummaryAsync` | 138 | `Task<ResponseCollection<TrackingCardSummaryRM>>` | ❌ Needs refactor |
| `GetAreaAccessedSummaryAsyncV3` | 200 | `Task<object>` | ✅ Already uses ApiResponse pattern |
| *(Commented)* Various | 170-198 | N/A | Remove commented code |

---

## Endpoint Categorization & Naming

### Current Endpoint Categories

Analytics endpoints serve **3 different purposes**:

#### 1. SUMMARY ENDPOINTS (Data Agregasi - List/Tabular)

**Purpose**: Return aggregated data for tables, lists, or grids

| Service | Method | DTO | Use Case |
|---------|--------|-----|----------|
| Alarm | `GetDailySummaryAsync` | `AlarmDailySummaryDto` | Total per day for table/grid |
| Alarm | `GetStatusSummaryAsync` | `AlarmStatusSummaryDto` | Total per status for pie chart/table |
| Alarm | `GetVisitorSummaryAsync` | `AlarmVisitorSummaryDto` | Total per visitor for list |
| Alarm | `GetBuildingSummaryAsync` | `AlarmBuildingSummaryDto` | Total per building for table |
| Alarm | `GetHourlyStatusSummaryAsync` | `AlarmHourlyStatusSummaryDto` | Hourly breakdown for bar chart/table |
| Tracking | `GetAreaSummaryAsync` | `TrackingAreaSummaryDto` | Total per area for table |
| Tracking | `GetDailySummaryAsync` | `TrackingDailySummaryDto` | Total per day for table |
| Tracking | `GetReaderSummaryAsync` | `TrackingReaderSummaryDto` | Total per reader for table |
| Tracking | `GetVisitorSummaryAsync` | `TrackingVisitorSummaryDto` | Total per visitor for list |
| Tracking | `GetBuildingSummaryAsync` | `TrackingBuildingSummaryDto` | Total per building for table |
| Tracking | `GetCardSummaryAsync` | `TrackingCardSummaryRM` | Latest positions for list |

**Response Pattern**:
```json
{
  "success": true,
  "msg": "Daily summary retrieved",
  "collection": {
    "data": [
      { "date": "2025-01-01", "total": 15 },
      { "date": "2025-01-02", "total": 23 }
    ]
  }
}
```

#### 2. CHART ENDPOINTS (Data Visualisasi)

**Purpose**: Return chart-specific data with labels and series for frontend chart libraries

| Service | Method | DTO | Chart Type |
|---------|--------|-----|------------|
| Alarm | `GetAreaSummaryChartAsync` | `AlarmAreaChartResponseDto` | Multi-line: Area × Status over time |
| Tracking | `GetAreaAccessedSummaryAsyncV3` | `AreaAccessResponseDto` | Multi-line: Permission breakdown over time |

**Response Pattern**:
```json
{
  "success": true,
  "msg": "Chart retrieved",
  "collection": {
    "data": {
      "labels": ["2025-01-01", "2025-01-02"],
      "areas": [
        {
          "areaName": "Lobby",
          "series": [
            { "name": "Active", "data": [10, 20] },
            { "name": "Inactive", "data": [5, 8] }
          ]
        }
      ]
    }
  }
}
```

#### 3. DETAIL/TRANSACTION ENDPOINTS (Data Detail)

**Purpose**: Return detailed transaction data for detail views or special visualizations

| Service | Method | DTO | Use Case |
|---------|--------|-----|----------|
| Tracking | `GetTrackingMovementByCardIdAsync` | `TrackingMovementRM` | Movement history for card |
| Tracking | `GetHeatmapDataAsync` | `TrackingHeatmapRM` | Position data for heatmap |

#### 4. COMBINED ENDPOINTS (Hybrid)

**Purpose**: Return both summary and chart in single response

| Service | Method | DTO | Contents |
|---------|--------|-----|----------|
| Tracking | `GetAreaAccessedSummaryAsyncV3` | `AreaAccessResponseDto` | `Summary` object + `Chart` object |

---

### Naming Convention Recommendations

#### Current Issues

1. **Ambiguous naming**: `GetAreaSummaryChartAsync` vs `GetDailySummaryAsync` - not clear which returns chart vs list
2. **Version suffixes**: `GetAreaAccessedSummaryAsyncV3` - indicates evolution but unclear current state
3. **Inconsistent patterns**: Some use "Summary", some use "Chart", some use "Data"

#### Proposed Naming Convention

**Option A: Suffix-Based (Recommended)** ✅

```
Get{Entity}SummaryAsync  → List data for tables/grids
Get{Entity}ChartAsync    → Chart data (Labels + Series)
Get{Entity}DetailAsync   → Detailed transaction data
Get{Entity}OverviewAsync → Combined (Summary + Chart)
```

**Examples**:
- `GetDailySummaryAsync` ✅ (already correct)
- `GetStatusSummaryAsync` ✅ (already correct)
- `GetAreaChartAsync` (rename from `GetAreaSummaryChartAsync`)
- `GetMovementDetailAsync` (rename from `GetTrackingMovementByCardIdAsync`)
- `GetAreaAccessedOverviewAsync` (rename from `GetAreaAccessedSummaryAsyncV3`)

**Option B: Purpose-Based**

```
Get{Entity}ForTableAsync   → List data for tables
Get{Entity}ForChartAsync   → Chart data
Get{Entity}ForListAsync    → List data for dropdowns
Get{Entity}ForDashboardAsync → Combined data
```

**Option C: Output-Based**

```
Get{Entity}ListAsync       → Returns list/array
Get{Entity}SeriesAsync     → Returns chart series
Get{Entity}RecordAsync     → Returns single record with details
```

---

### Recommended Renaming (Optional)

| Current Name | Recommended Name | Reason |
|--------------|------------------|--------|
| `GetAreaSummaryChartAsync` | `GetAreaChartAsync` | Clearer: returns chart data |
| `GetAreaAccessedSummaryAsyncV3` | `GetAreaAccessedChartAsync` | Clearer: returns chart data |
| `GetTrackingMovementByCardIdAsync` | `GetMovementDetailAsync` | Shorter, clear purpose |
| `GetHeatmapDataAsync` | ✅ Keep | Already clear |
| All `*SummaryAsync` | ✅ Keep | Already clear |

**Note**: Renaming is **OPTIONAL** and can be done in a separate refactoring. The main task is standardizing response types.

---

## Universal Filter Design

### Current Filter Structure

Both services use similar request models:

**AlarmAnalyticsRequestRM**:
```csharp
public class AlarmAnalyticsRequestRM
{
    // Date filters
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? TimeRange { get; set; }  // "daily", "weekly", "monthly"

    // Entity filters
    public Guid? BuildingId { get; set; }
    public Guid? FloorId { get; set; }
    public Guid? AreaId { get; set; }
    public Guid? CardId { get; set; }
    public Guid? VisitorId { get; set; }
    public Guid? MemberId { get; set; }
    public Guid? ReaderId { get; set; }
    public Guid? FloorplanMaskedAreaId { get; set; }

    // String filters
    public string? OperatorName { get; set; }
    public bool? IsActive { get; set; }
}
```

**TrackingAnalyticsRequestRM**:
```csharp
public class TrackingAnalyticsRequestRM
{
    // Date filters
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? TimeRange { get; set; }

    // Entity filters
    public Guid? BuildingId { get; set; }
    public Guid? FloorId { get; set; }
    public Guid? AreaId { get; set; }
    public Guid? CardId { get; set; }
    public Guid? VisitorId { get; set; }
    public Guid? MemberId { get; set; }
    public Guid? ReaderId { get; set; }

    // Additional filters
    public string? IdentityId { get; set; }
    public string? PersonType { get; set; }  // "visitor", "member"
    public string? Timezone { get; set; }
    public string? ReportTitle { get; set; }
    public string? ExportType { get; set; }
}
```

### Universal Filter Pattern ✅ **ALREADY IMPLEMENTED**

**Good News**: Your current architecture **ALREADY SUPPORTS** universal filters!

**How it works**:

1. **Same filter for multiple endpoints**:
   ```javascript
   // Frontend: One filter component
   const [filter, setFilter] = useState({
     from: '2025-01-01',
     to: '2025-01-31',
     buildingId: selectedBuilding,
     areaId: selectedArea
   });

   // Use for BOTH summary and chart
   const summary = await api.post('/alarmanalyticsincident/daily', filter);
   const chart = await api.post('/alarmanalyticsincident/area', filter);
   ```

2. **All endpoints accept the same request model**:
   - ✅ `GetDailySummaryAsync(AlarmAnalyticsRequestRM request)`
   - ✅ `GetAreaChartAsync(AlarmAnalyticsRequestRM request)`
   - ✅ `GetStatusSummaryAsync(AlarmAnalyticsRequestRM request)`

3. **Frontend can create ONE filter component**:
   ```jsx
   <UniversalFilter
     onChange={(filter) => {
       // Update ALL views with same filter
       fetchDailySummary(filter);
       fetchAreaChart(filter);
       fetchStatusSummary(filter);
     }}
   />
   ```

---

### Frontend Implementation Guide

#### Pattern 1: Single Filter, Multiple Views

```jsx
// AnalyticsDashboard.jsx
function AnalyticsDashboard() {
  const [filter, setFilter] = useState({
    from: moment().subtract(7, 'days').toDate(),
    to: moment().toDate(),
    buildingId: null,
    areaId: null,
    timeRange: 'daily'
  });

  // Fetch ALL views with SAME filter
  useEffect(() => {
    fetchDailySummary(filter);
    fetchAreaChart(filter);
    fetchStatusSummary(filter);
  }, [filter]);

  return (
    <>
      <FilterPanel filter={filter} onChange={setFilter} />

      <Row>
        <Col><DailySummaryTable data={summary} /></Col>
        <Col><AreaChart data={chart} /></Col>
      </Row>
      <Row>
        <Col><StatusPieChart data={status} /></Col>
      </Row>
    </>
  );
}
```

#### Pattern 2: Switch Between Chart and Summary

```jsx
function AnalyticsView() {
  const [viewType, setViewType] = useState('chart'); // 'chart' or 'table'

  return (
    <>
      <FilterPanel value={filter} onChange={setFilter} />

      <ViewToggle value={viewType} onChange={setViewType} />

      {viewType === 'chart' ? (
        <AreaChart data={chartData} />
      ) : (
        <DailySummaryTable data={summaryData} />
      )}
    </>
  );
}
```

---

### Backend Considerations

#### Current Implementation ✅ **Already Optimized**

1. **Repositories reuse the same filters**:
   ```csharp
   public async Task<List<AlarmDailySummaryRM>> GetDailySummaryAsync(
       AlarmAnalyticsRequestRM request)
   {
       var query = BaseEntityQuery();

       // Apply SAME filters for all query types
       if (request.BuildingId.HasValue)
           query = query.Where(x => x.BuildingId == request.BuildingId.Value);
       // ... etc

       return await query.GroupBy(x => x.TransTime.Date)
           .Select(g => new AlarmDailySummaryRM { ... })
           .ToListAsync();
   }

   public async Task<List<AlarmAreaDailyAggregateRM>> GetAreaDailySummaryAsync(
       AlarmAnalyticsRequestRM request)
   {
       var query = BaseEntityQuery();

       // Apply SAME filters (consistency!)
       if (request.BuildingId.HasValue)
           query = query.Where(x => x.BuildingId == request.BuildingId.Value);
       // ... etc

       return await query.GroupBy(x => new { x.AreaId, x.Date })
           .Select(g => new AlarmAreaDailyAggregateRM { ... })
           .ToListAsync();
   }
   ```

2. **Service methods accept same request type**:
   ```csharp
   // ALL methods accept AlarmAnalyticsRequestRM
   Task<object> GetDailySummaryAsync(AlarmAnalyticsRequestRM request);
   Task<object> GetAreaChartAsync(AlarmAnalyticsRequestRM request);
   Task<object> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request);
   ```

---

### Benefits of Universal Filter Pattern

| Benefit | Description |
|---------|-------------|
| **DRY Principle** | One filter component serves all views |
| **Consistent UX** | User sees same filter options everywhere |
| **Less Code** | Don't repeat filter logic in multiple components |
| **Easier Maintenance** | Change filter in one place, updates all views |
| **Better Performance** | Can cache/batch API calls with same filter |

---

### Implementation Checklist for Universal Filter

- [x] **Backend**: All endpoints accept same request model ✅ (already done)
- [x] **Backend**: Repositories apply filters consistently ✅ (already done)
- [ ] **Frontend**: Create universal filter component
- [ ] **Frontend**: Use filter for both chart and summary endpoints
- [ ] **Frontend**: Add loading states for multiple concurrent requests
- [ ] **Frontend**: Consider request batching/performance optimization

---

### Optional: Create Base Request Model

If you want even more consistency, consider creating a base class:

```csharp
// Shared/Repositories/Repository/RepoModel/BaseAnalyticsRequestRM.cs
public abstract class BaseAnalyticsRequestRM
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? TimeRange { get; set; }
    public Guid? BuildingId { get; set; }
    public Guid? FloorId { get; set; }
    public Guid? AreaId { get; set; }
}

// Alarm-specific request
public class AlarmAnalyticsRequestRM : BaseAnalyticsRequestRM
{
    public Guid? VisitorId { get; set; }
    public Guid? MemberId { get; set; }
    public Guid? ReaderId { get; set; }
    public string? OperatorName { get; set; }
    public bool? IsActive { get; set; }
}

// Tracking-specific request
public class TrackingAnalyticsRequestRM : BaseAnalyticsRequestRM
{
    public Guid? CardId { get; set; }
    public string? IdentityId { get; set; }
    public string? PersonType { get; set; }
    public string? Timezone { get; set; }
}
```

**This is OPTIONAL** - current implementation already works well!

---

## Proposed Solution

### Design Decision: ApiResponse with DTOs

**Chosen Pattern**:
```csharp
// Service Method Signature
public async Task<object> MethodName(RequestType request)

// Success Response
return ApiResponse.Success("Message", stronglyTypedDto);

// Error Response
return ApiResponse.InternalError($"Error: {ex.Message}");
```

**Why This Pattern**:
1. ✅ Consistent with existing endpoints that already use ApiResponse
2. ✅ Simpler response structure (flat, no CollectionWrapper nesting)
3. ✅ DTOs provide compile-time type safety
4. ✅ Zero controller changes needed

### Response Structure Comparison

**Before (ResponseCollection)**:
```json
{
  "success": true,
  "msg": "OK",
  "timezone": "Asia/Jakarta",
  "collection": {
    "data": [
      {"id": "xxx", "name": "Area A", "total": 10}
    ]
  },
  "code": 200
}
```

**After (ApiResponse - Target)**:
```json
{
  "success": true,
  "msg": "Area summary retrieved successfully",
  "collection": {
    "data": [
      {"id": "xxx", "name": "Area A", "total": 10}
    ]
  },
  "code": 200
}
```

**Key Difference**: Remove `timezone` property (not consistently used), remove extra CollectionWrapper nesting.

---

## Implementation Guide

### Phase 1: AlarmAnalyticsIncidentService Refactoring

#### Step 1.1: Update Interface

**File**: `Shared/BusinessLogic.Services/Interface/Analytics/IAlarmAnalyticsIncidentService.cs`

**Current Interface**:
```csharp
using Data.ViewModels.AlarmAnalytics;
using Data.ViewModels;
using AlarmAnalyticsRequestRM = Data.ViewModels.AlarmAnalytics.AlarmAnalyticsRequestRM;

namespace BusinessLogic.Services.Interface.Analytics
{
    public interface IAlarmAnalyticsIncidentService
    {
        Task<ResponseCollection<AlarmAreaSummaryDto>> GetAreaSummaryAsync(AlarmAnalyticsRequestRM request); // COMMENTED
        Task<object> GetAreaSummaryChartAsync(AlarmAnalyticsRequestRM request);
        Task<ResponseCollection<AlarmDailySummaryDto>> GetDailySummaryAsync(AlarmAnalyticsRequestRM request);
        Task<ResponseCollection<AlarmStatusSummaryDto>> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request);
        Task<ResponseCollection<AlarmVisitorSummaryDto>> GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request);
        Task<ResponseCollection<AlarmBuildingSummaryDto>> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request);
        Task<ResponseCollection<AlarmHourlyStatusSummaryDto>> GetHourlyStatusSummaryAsync(AlarmAnalyticsRequestRM request);
    }
}
```

**Updated Interface**:
```csharp
using Data.ViewModels.AlarmAnalytics;
using AlarmAnalyticsRequestRM = Data.ViewModels.AlarmAnalytics.AlarmAnalyticsRequestRM;

namespace BusinessLogic.Services.Interface.Analytics
{
    public interface IAlarmAnalyticsIncidentService
    {
        Task<object> GetAreaSummaryChartAsync(AlarmAnalyticsRequestRM request);
        Task<object> GetDailySummaryAsync(AlarmAnalyticsRequestRM request);
        Task<object> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request);
        Task<object> GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request);
        Task<object> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request);
        Task<object> GetHourlyStatusSummaryAsync(AlarmAnalyticsRequestRM request);
    }
}
```

**Changes**:
- Remove `using Data.ViewModels;` (no longer need ResponseCollection)
- Remove commented `GetAreaSummaryAsync` method
- Change all return types from `Task<ResponseCollection<T>>` to `Task<object>`

---

#### Step 1.2: Update Service Implementation

**File**: `Shared/BusinessLogic.Services/Implementation/Analytics/AlarmAnalyticsIncidentService.cs`

**Add Using Statement** (at top, after existing usings):
```csharp
using Data.ViewModels.ResponseHelper;
```

**Update Method 1: GetDailySummaryAsync**

**Before**:
```csharp
public async Task<ResponseCollection<AlarmDailySummaryDto>> GetDailySummaryAsync(AlarmAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetDailySummaryAsync(request);
        var dto = _mapper.Map<List<AlarmDailySummaryDto>>(data);
        return ResponseCollection<AlarmDailySummaryDto>.Ok(dto, "Incident daily summary retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident daily summary");
        return ResponseCollection<AlarmDailySummaryDto>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetDailySummaryAsync(AlarmAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetDailySummaryAsync(request);
        var dto = _mapper.Map<List<AlarmDailySummaryDto>>(data);
        return ApiResponse.Success("Incident daily summary retrieved successfully", dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident daily summary");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 2: GetStatusSummaryAsync**

**Before**:
```csharp
public async Task<ResponseCollection<AlarmStatusSummaryDto>> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetStatusSummaryAsync(request);
        var dto = _mapper.Map<List<AlarmStatusSummaryDto>>(data);
        return ResponseCollection<AlarmStatusSummaryDto>.Ok(dto, "Incident status summary retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident status summary");
        return ResponseCollection<AlarmStatusSummaryDto>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetStatusSummaryAsync(request);
        var dto = _mapper.Map<List<AlarmStatusSummaryDto>>(data);
        return ApiResponse.Success("Incident status summary retrieved successfully", dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident status summary");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 3: GetVisitorSummaryAsync**

**Before**:
```csharp
public async Task<ResponseCollection<AlarmVisitorSummaryDto>> GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetVisitorSummaryAsync(request);
        var dto = _mapper.Map<List<AlarmVisitorSummaryDto>>(data);
        return ResponseCollection<AlarmVisitorSummaryDto>.Ok(dto, "Incident visitor summary retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident visitor summary");
        return ResponseCollection<AlarmVisitorSummaryDto>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetVisitorSummaryAsync(request);
        var dto = _mapper.Map<List<AlarmVisitorSummaryDto>>(data);
        return ApiResponse.Success("Incident visitor summary retrieved successfully", dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident visitor summary");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 4: GetBuildingSummaryAsync**

**Before**:
```csharp
public async Task<ResponseCollection<AlarmBuildingSummaryDto>> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetBuildingSummaryAsync(request);
        var dto = _mapper.Map<List<AlarmBuildingSummaryDto>>(data);
        return ResponseCollection<AlarmBuildingSummaryDto>.Ok(dto, "Incident building summary retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident building summary");
        return ResponseCollection<AlarmBuildingSummaryDto>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetBuildingSummaryAsync(request);
        var dto = _mapper.Map<List<AlarmBuildingSummaryDto>>(data);
        return ApiResponse.Success("Incident building summary retrieved successfully", dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident building summary");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 5: GetHourlyStatusSummaryAsync**

**Before**:
```csharp
public async Task<ResponseCollection<AlarmHourlyStatusSummaryDto>> GetHourlyStatusSummaryAsync(AlarmAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetHourlyStatusSummaryAsync(request);
        var dto = _mapper.Map<List<AlarmHourlyStatusSummaryDto>>(data);
        return ResponseCollection<AlarmHourlyStatusSummaryDto>.Ok(dto, "Incident Daily(24 Hours) summary retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident Daily(24 Hours) summary");
        return ResponseCollection<AlarmHourlyStatusSummaryDto>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetHourlyStatusSummaryAsync(AlarmAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetHourlyStatusSummaryAsync(request);
        var dto = _mapper.Map<List<AlarmHourlyStatusSummaryDto>>(data);
        return ApiResponse.Success("Incident Daily(24 Hours) summary retrieved successfully", dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident Daily(24 Hours) summary");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 6: GetAreaSummaryChartAsync**

**Before** (current implementation):
```csharp
public async Task<object> GetAreaSummaryChartAsync(
    AlarmAnalyticsRequestRM request
)
{
    var rows = await _repository.GetAreaDailySummaryAsync(request);

    // labels (dates)
    var dates = rows
        .Select(r => r.Date)
        .Distinct()
        .OrderBy(d => d)
        .ToList();

    var labels = dates
        .Select(d => d.ToString("yyyy-MM-dd"))
        .ToList();

    // group per area
    var areas = rows
        .GroupBy(r => new { r.AreaId, r.AreaName })
        .Select(areaGroup =>
        {
            var statuses = areaGroup
                .Select(x => x.AlarmStatus)
                .Distinct();

            var series = statuses.Select(status => new ChartSeriesDto
            {
                Name = status,
                Data = dates.Select(date =>
                    areaGroup.FirstOrDefault(r =>
                        r.Date == date &&
                        r.AlarmStatus == status
                    )?.Total ?? 0
                ).ToList()
            }).ToList();

            return new AlarmAreaChartDto
            {
                AreaId = areaGroup.Key.AreaId,
                AreaName = areaGroup.Key.AreaName,
                Series = series
            };
        })
        .ToList();

    var result = new AlarmAreaChartResponseDto
    {
        Labels = labels,
        Areas = areas
    };
    return result;
}
```

**After** (add try-catch, wrap with ApiResponse):
```csharp
public async Task<object> GetAreaSummaryChartAsync(
    AlarmAnalyticsRequestRM request
)
{
    try
    {
        var rows = await _repository.GetAreaDailySummaryAsync(request);

        // labels (dates)
        var dates = rows
            .Select(r => r.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var labels = dates
            .Select(d => d.ToString("yyyy-MM-dd"))
            .ToList();

        // group per area
        var areas = rows
            .GroupBy(r => new { r.AreaId, r.AreaName })
            .Select(areaGroup =>
            {
                var statuses = areaGroup
                    .Select(x => x.AlarmStatus)
                    .Distinct();

                var series = statuses.Select(status => new ChartSeriesDto
                {
                    Name = status,
                    Data = dates.Select(date =>
                        areaGroup.FirstOrDefault(r =>
                            r.Date == date &&
                            r.AlarmStatus == status
                        )?.Total ?? 0
                    ).ToList()
                }).ToList();

                return new AlarmAreaChartDto
                {
                    AreaId = areaGroup.Key.AreaId,
                    AreaName = areaGroup.Key.AreaName,
                    Series = series
                };
            })
            .ToList();

        var response = new AlarmAreaChartResponseDto
        {
            Labels = labels,
            Areas = areas
        };

        return ApiResponse.Success("Area summary chart retrieved successfully", response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting area summary chart");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Step 1.3: Remove Commented Code**

Remove lines 31-44 (commented GetAreaSummaryAsync method).

---

### Phase 2: TrackingAnalyticsService Refactoring

#### Step 2.1: Update Interface

**File**: `Shared/BusinessLogic.Services/Interface/Analytics/ITrackingAnalyticsService.cs`

**Current Interface**:
```csharp
using Data.ViewModels;
using Data.ViewModels.AlarmAnalytics;
using Helpers.Consumer;

namespace BusinessLogic.Services.Interface.Analytics
{
    public interface ITrackingAnalyticsService
    {
        Task<ResponseCollection<TrackingAreaSummaryDto>> GetAreaSummaryAsync(TrackingAnalyticsRequestRM request);
        Task<ResponseCollection<TrackingDailySummaryDto>> GetDailySummaryAsync(TrackingAnalyticsRequestRM request);
        Task<ResponseCollection<TrackingReaderSummaryDto>> GetReaderSummaryAsync(TrackingAnalyticsRequestRM request);
        Task<ResponseCollection<TrackingVisitorSummaryDto>> GetVisitorSummaryAsync(TrackingAnalyticsRequestRM request);
        Task<ResponseCollection<TrackingBuildingSummaryDto>> GetBuildingSummaryAsync(TrackingAnalyticsRequestRM request);
        Task<ResponseCollection<TrackingMovementRM>> GetTrackingMovementByCardIdAsync(Guid cardId);
        Task<ResponseCollection<TrackingHeatmapRM>> GetHeatmapDataAsync(TrackingAnalyticsRequestRM request);
        Task<ResponseCollection<TrackingCardSummaryRM>> GetCardSummaryAsync(TrackingAnalyticsRequestRM request);
        Task<object> GetAreaAccessedSummaryAsyncV3(TrackingAnalyticsRequestRM request);
    }
}
```

**Updated Interface**:
```csharp
using Data.ViewModels.AlarmAnalytics;
using Helpers.Consumer;

namespace BusinessLogic.Services.Interface.Analytics
{
    public interface ITrackingAnalyticsService
    {
        Task<object> GetAreaSummaryAsync(TrackingAnalyticsRequestRM request);
        Task<object> GetDailySummaryAsync(TrackingAnalyticsRequestRM request);
        Task<object> GetReaderSummaryAsync(TrackingAnalyticsRequestRM request);
        Task<object> GetVisitorSummaryAsync(TrackingAnalyticsRequestRM request);
        Task<object> GetBuildingSummaryAsync(TrackingAnalyticsRequestRM request);
        Task<object> GetTrackingMovementByCardIdAsync(Guid cardId);
        Task<object> GetHeatmapDataAsync(TrackingAnalyticsRequestRM request);
        Task<object> GetCardSummaryAsync(TrackingAnalyticsRequestRM request);
        Task<object> GetAreaAccessedSummaryAsyncV3(TrackingAnalyticsRequestRM request);
    }
}
```

**Changes**:
- Remove `using Data.ViewModels;` (no longer need ResponseCollection)
- Change all return types from `Task<ResponseCollection<T>>` to `Task<object>`

---

#### Step 2.2: Update Service Implementation

**File**: `Shared/BusinessLogic.Services/Implementation/Analytics/TrackingAnalyticsService.cs`

**Add Using Statement** (at top, after existing usings):
```csharp
using Data.ViewModels.ResponseHelper;
```

**Update Method 1: GetAreaSummaryAsync**

**Before**:
```csharp
public async Task<ResponseCollection<TrackingAreaSummaryDto>> GetAreaSummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetAreaSummaryAsync(request);
        var dto = _mapper.Map<List<TrackingAreaSummaryDto>>(data);
        return ResponseCollection<TrackingAreaSummaryDto>.Ok(dto, "Area summary retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting area summary");
        return ResponseCollection<TrackingAreaSummaryDto>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetAreaSummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetAreaSummaryAsync(request);
        var dto = _mapper.Map<List<TrackingAreaSummaryDto>>(data);
        return ApiResponse.Success("Area summary retrieved successfully", dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting area summary");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 2: GetDailySummaryAsync**

**Before**:
```csharp
public async Task<ResponseCollection<TrackingDailySummaryDto>> GetDailySummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetDailySummaryAsync(request);
        var dto = _mapper.Map<List<TrackingDailySummaryDto>>(data);
        return ResponseCollection<TrackingDailySummaryDto>.Ok(dto, "daily summary retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident daily summary");
        return ResponseCollection<TrackingDailySummaryDto>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetDailySummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetDailySummaryAsync(request);
        var dto = _mapper.Map<List<TrackingDailySummaryDto>>(data);
        return ApiResponse.Success("Daily summary retrieved successfully", dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting daily summary");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 3: GetReaderSummaryAsync**

**Before**:
```csharp
public async Task<ResponseCollection<TrackingReaderSummaryDto>> GetReaderSummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetReaderSummaryAsync(request);
        var dto = _mapper.Map<List<TrackingReaderSummaryDto>>(data);
        return ResponseCollection<TrackingReaderSummaryDto>.Ok(dto, "status summary retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident status summary");
        return ResponseCollection<TrackingReaderSummaryDto>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetReaderSummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetReaderSummaryAsync(request);
        var dto = _mapper.Map<List<TrackingReaderSummaryDto>>(data);
        return ApiResponse.Success("Reader summary retrieved successfully", dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting reader summary");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 4: GetVisitorSummaryAsync**

**Before**:
```csharp
public async Task<ResponseCollection<TrackingVisitorSummaryDto>> GetVisitorSummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetVisitorSummaryAsync(request);
        var dto = _mapper.Map<List<TrackingVisitorSummaryDto>>(data);
        return ResponseCollection<TrackingVisitorSummaryDto>.Ok(dto, "Incident visitor summary retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident visitor summary");
        return ResponseCollection<TrackingVisitorSummaryDto>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetVisitorSummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetVisitorSummaryAsync(request);
        var dto = _mapper.Map<List<TrackingVisitorSummaryDto>>(data);
        return ApiResponse.Success("Visitor summary retrieved successfully", dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting visitor summary");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 5: GetBuildingSummaryAsync**

**Before**:
```csharp
public async Task<ResponseCollection<TrackingBuildingSummaryDto>> GetBuildingSummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetBuildingSummaryAsync(request);
        var dto = _mapper.Map<List<TrackingBuildingSummaryDto>>(data);
        return ResponseCollection<TrackingBuildingSummaryDto>.Ok(dto, "Incident building summary retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting incident building summary");
        return ResponseCollection<TrackingBuildingSummaryDto>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetBuildingSummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetBuildingSummaryAsync(request);
        var dto = _mapper.Map<List<TrackingBuildingSummaryDto>>(data);
        return ApiResponse.Success("Building summary retrieved successfully", dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting building summary");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 6: GetTrackingMovementByCardIdAsync**

**Before**:
```csharp
public async Task<ResponseCollection<TrackingMovementRM>> GetTrackingMovementByCardIdAsync(Guid cardId)
{
    try
    {
        var data = await _repository.GetTrackingMovementByCardIdAsync(cardId);
        return ResponseCollection<TrackingMovementRM>.Ok(data, "Tracking movement retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error while getting tracking movement by card ID {CardId}", cardId);
        return ResponseCollection<TrackingMovementRM>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetTrackingMovementByCardIdAsync(Guid cardId)
{
    try
    {
        var data = await _repository.GetTrackingMovementByCardIdAsync(cardId);
        return ApiResponse.Success("Tracking movement retrieved successfully", data);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error while getting tracking movement by card ID {CardId}", cardId);
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 7: GetHeatmapDataAsync**

**Before**:
```csharp
public async Task<ResponseCollection<TrackingHeatmapRM>> GetHeatmapDataAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetHeatmapDataAsync(request);
        return ResponseCollection<TrackingHeatmapRM>.Ok(data, "Tracking heatmap retrieved successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error while getting heatmap data");
        return ResponseCollection<TrackingHeatmapRM>.Error($"Internal server error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetHeatmapDataAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetHeatmapDataAsync(request);
        return ApiResponse.Success("Tracking heatmap retrieved successfully", data);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error while getting heatmap data");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Update Method 8: GetCardSummaryAsync**

**Before**:
```csharp
public async Task<ResponseCollection<TrackingCardSummaryRM>> GetCardSummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetCardSummaryAsync(request);

        var tz = TimezoneHelper.Resolve(request.Timezone);

        if (tz.Id != TimeZoneInfo.Utc.Id)
        {
            foreach (var item in data)
            {
                item.LastDetectedAt =
                    TimezoneHelper.ConvertFromUtc(item.LastDetectedAt, tz);
            }
        }

        return ResponseCollection<TrackingCardSummaryRM>
            .Ok(data, "Tracking Summary retrieved successfully", timezone: tz.Id);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in GetVisitorSessionSummaryAsync");
        return ResponseCollection<TrackingCardSummaryRM>
            .Error($"Internal error: {ex.Message}");
    }
}
```

**After**:
```csharp
public async Task<object> GetCardSummaryAsync(TrackingAnalyticsRequestRM request)
{
    try
    {
        var data = await _repository.GetCardSummaryAsync(request);

        var tz = TimezoneHelper.Resolve(request.Timezone);

        if (tz.Id != TimeZoneInfo.Utc.Id)
        {
            foreach (var item in data)
            {
                item.LastDetectedAt =
                    TimezoneHelper.ConvertFromUtc(item.LastDetectedAt, tz);
            }
        }

        return ApiResponse.Success("Tracking summary retrieved successfully", data);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting card summary");
        return ApiResponse.InternalError($"Internal server error: {ex.Message}");
    }
}
```

**Note**: `GetAreaAccessedSummaryAsyncV3` already uses ApiResponse pattern - no changes needed!

**Step 2.3: Remove Commented Code**

Remove lines 170-198 (commented GetAreaAccessedSummaryAsync versions).

---

### Phase 3: DTO Organization (Optional but Recommended)

#### Step 3.1: Create Shared Chart DTOs

**New File**: `Shared/DataViewModels/Shared/ChartDto.cs`

```csharp
namespace Data.ViewModels.Shared
{
    /// <summary>
    /// Universal chart series DTO used across all analytics services
    /// </summary>
    public class ChartSeriesDto
    {
        /// <summary>
        /// Series name (e.g., "Active", "Inactive", "With Permission")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Data points for this series
        /// </summary>
        public List<int> Data { get; set; } = new();

        /// <summary>
        /// Optional: Color for consistent frontend rendering
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Optional: Chart type (line, bar, area)
        /// </summary>
        public string? Type { get; set; }
    }

    /// <summary>
    /// Optional metadata for chart statistics
    /// </summary>
    public class ChartMetadataDto
    {
        public int Total { get; set; }
        public double Average { get; set; }
        public int Max { get; set; }
        public int Min { get; set; }
    }
}
```

#### Step 3.2: Update Existing DTO Files

**File**: `Shared/DataViewModels/AlarmAnalytics/TrackingAnalyticsSummaryDto.cs`

**Add Using** (at top):
```csharp
using Data.ViewModels.Shared;
```

**Remove local definition** (lines 101-105):
```csharp
// REMOVE THIS:
// public class ChartSeriesDto
// {
//     public string Name { get; set; }
//     public List<int> Data { get; set; }
// }
```

**File**: `Shared/DataViewModels/AlarmAnalytics/AlarmAnalyticsSummaryDto.cs`

**Remove commented definition** (lines 65-69):
```csharp
// REMOVE THIS:
// public class ChartSeriesDto
// {
//     public string Name { get; set; } = string.Empty;
//     public List<int> Data { get; set; } = new();
// }
```

**Add Using** (at top):
```csharp
using Data.ViewModels.Shared;
```

---

### Phase 4: Controllers (NO CHANGES NEEDED)

Controllers already return `Ok(response)` and will automatically work with the new ApiResponse format.

**Example** (AlarmAnalyticsIncidentController):
```csharp
[HttpPost("daily")]
public async Task<IActionResult> GetDailySummary(AlarmAnalyticsRequestRM request)
{
    var response = await _service.GetDailySummaryAsync(request);
    return Ok(response);
}
```

This continues to work unchanged because `Ok()` accepts any `object`.

---

## Testing & Verification

### Manual Testing Steps

#### 1. Test AlarmAnalyticsIncidentService Endpoints

**Endpoint**: `POST /api/alarmanalyticsincident/daily`

**Request Body**:
```json
{
  "from": "2025-01-01T00:00:00Z",
  "to": "2025-01-31T23:59:59Z"
}
```

**Expected Response** (200 OK):
```json
{
  "success": true,
  "msg": "Incident daily summary retrieved successfully",
  "collection": {
    "data": [
      {
        "date": "2025-01-01T00:00:00",
        "total": 15
      }
    ]
  },
  "code": 200
}
```

**Verification Checklist**:
- [ ] Response structure matches expected format
- [ ] No `timezone` property in response
- [ ] No extra `CollectionWrapper` nesting
- [ ] `success` is `true`
- [ ] `msg` contains descriptive message
- [ ] `code` is `200`
- [ ] `collection.data` contains array of items

**Repeat for**:
- [ ] `/api/alarmanalyticsincident/status`
- [ ] `/api/alarmanalyticsincident/visitor`
- [ ] `/api/alarmanalyticsincident/building`
- [ ] `/api/alarmanalyticsincident/hourly`
- [ ] `/api/alarmanalyticsincident/area` (chart endpoint)

#### 2. Test TrackingAnalyticsService Endpoints

**Test all endpoints**:
- [ ] `POST /api/trackinganalytics/area`
- [ ] `POST /api/trackinganalytics/daily`
- [ ] `POST /api/trackinganalytics/reader`
- [ ] `POST /api/trackinganalytics/visitor`
- [ ] `POST /api/trackinganalytics/building`
- [ ] `GET /api/trackinganalytics/movement/{cardId}`
- [ ] `POST /api/trackinganalytics/heatmap`
- [ ] `POST /api/trackinganalytics/latest-position`
- [ ] `POST /api/trackinganalytics/area-accessed`

#### 3. Test Error Handling

**Test invalid request**:
```json
{
  "from": "invalid-date"
}
```

**Expected Response** (500 Internal Server Error):
```json
{
  "success": false,
  "msg": "Internal server error: [error details]",
  "collection": {
    "data": null
  },
  "code": 500
}
```

### Post-Deployment Verification

1. **Check Logs**: Ensure no exceptions in service logs
2. **Monitor Frontend**: Verify charts render correctly with new response format
3. **API Consumers**: Confirm all consumers still work (response structure compatible)

---

## Rollback Plan

If issues arise after deployment:

### Quick Rollback Steps

1. **Revert Service Interfaces**:
   - Restore return types to `Task<ResponseCollection<T>>`

2. **Revert Service Implementations**:
   - Restore `ResponseCollection<T>.Ok()` calls
   - Restore `ResponseCollection<T>.Error()` calls

3. **No Controller Changes Needed**: Controllers never changed, so no rollback needed there

### Git Rollback Commands

```bash
# View commits to find the refactoring commit
git log --oneline -10

# Revert the refactoring commit
git revert <commit-hash>

# Or reset to previous commit (if no other changes since)
git reset --hard <commit-before-refactoring>
```

---

## Summary of Changes

### Files Modified

| File | Changes | Lines Changed |
|------|---------|---------------|
| `IAlarmAnalyticsIncidentService.cs` | Update interface return types | ~7 lines |
| `AlarmAnalyticsIncidentService.cs` | Replace ResponseCollection with ApiResponse + add error handling to chart method | ~80 lines |
| `ITrackingAnalyticsService.cs` | Update interface return types | ~9 lines |
| `TrackingAnalyticsService.cs` | Replace ResponseCollection with ApiResponse | ~120 lines |

**Total**: 4 files, ~216 lines changed

### Files Created (Optional)

| File | Purpose |
|------|---------|
| `Shared/DataViewModels/Shared/ChartDto.cs` | Shared chart DTOs |

### Files Cleaned Up

| File | Action |
|------|--------|
| `AlarmAnalyticsIncidentService.cs` | Remove commented code (lines 31-44) |
| `TrackingAnalyticsService.cs` | Remove commented code (lines 170-198) |
| `AlarmAnalyticsSummaryDto.cs` | Remove commented ChartSeriesDto (lines 65-69) |
| `TrackingAnalyticsSummaryDto.cs` | Remove local ChartSeriesDto (lines 101-105) |

---

## Benefits

1. ✅ **Consistent API** - All analytics endpoints return identical response structure
2. ✅ **Simpler Response** - Flat JSON structure, no extra nesting
3. ✅ **Type-Safe DTOs** - Compile-time checking for data structures
4. ✅ **Zero Breaking Changes** - Controllers unchanged, response structure compatible
5. ✅ **Better Error Handling** - Consistent error responses across all endpoints
6. ✅ **Cleaner Code** - Remove commented code bloat

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Frontend breaks due to response format change | Low | Medium | Response structure is compatible (same fields, just nested differently) |
| Compilation errors | Low | Low | Simple find-replace pattern, easy to verify |
| Runtime errors | Low | Medium | Try-catch blocks preserved, error handling improved |
| Timezone handling regression | Low | Low | GetCardSummaryAsync preserves timezone logic |

**Overall Risk**: **LOW** - This is a straightforward refactoring with clear patterns and rollback plan.
