# Migrate Alarm Analytics Queries from AlarmRecordTracking to AlarmTriggers

## Context

**Problem**: Current analytics queries use `AlarmRecordTracking` (log table) which contains 10-20+ records per alarm incident. This causes:
- Scanning 10-20x more rows than necessary
- Requires `.Distinct()` to deduplicate by `AlarmTriggersId`
- Higher memory usage and slower queries

**Solution**: Query from `AlarmTriggers` directly (1 record per incident) for summary statistics.

## Entity Relationships

```
AlarmTriggers
  ├── TriggerTime (DateTime) - When alarm first triggered
  ├── Alarm (AlarmRecordStatus?) - Final alarm status
  ├── Action (ActionStatus?) - Final action status
  ├── VisitorId, MemberId, SecurityId
  ├── FloorplanId (FK to MstFloorplan)
  ├── InvestigatedResult, DoneBy, etc.
  └── Floorplan (MstFloorplan)
       ├── Floor (MstFloor)
       │    └── Building (MstBuilding)
       └── FloorplanMaskedAreas (ICollection<FloorplanMaskedArea>)

AlarmRecordTracking (Log/Audit Table)
  ├── Timestamp (DateTime) - When log entry was created
  ├── AlarmTriggersId (FK) - Links to AlarmTriggers
  ├── FloorplanMaskedAreaId (FK) - Direct link to area
  ├── Alarm, Action - Status at time of log
  └── FloorplanMaskedArea (direct navigation)
```

## Current Query Analysis

| Method | Current Table | Can Migrate? | Notes |
|--------|--------------|--------------|-------|
| `GetAreaDailySummaryAsync()` | AlarmRecordTracking | ⚠️ Partial | Needs area-level grouping |
| `GetDailySummaryAsync()` | AlarmRecordTracking | ✅ Yes | Simple date count |
| `GetStatusSummaryAsync()` | AlarmRecordTracking | ✅ Yes | Uses AlarmTriggersId + Alarm |
| `GetVisitorSummaryAsync()` | AlarmRecordTracking | ✅ Yes | AlarmTriggers has VisitorId |
| `GetBuildingSummaryAsync()` | AlarmRecordTracking | ✅ Yes | Can use Floorplan → Floor → Building |
| `GetHourlyStatusSummaryAsync()` | AlarmRecordTracking | ✅ Yes | Uses GroupBy + Min(Timestamp) |
| `GetInvestigatedResultSummaryAsync()` | AlarmTriggers | ✅ Already done | - |
| `GetRawIncidentDurationsAsync()` | AlarmTriggers | ✅ Already done | - |

## Important Limitation

**FloorplanMaskedAreaId filter**: `AlarmTriggers` only has `FloorplanId`, not specific `FloorplanMaskedAreaId`. When filtering by area:
- Current: `AlarmRecordTracking.FloorplanMaskedAreaId == X`
- Migrated: Need to check if `Floorplan.FloorplanMaskedAreas` contains the area

**Decision**: For area-specific queries, we may need to:
1. Keep using `AlarmRecordTracking` for exact area filtering
2. Or aggregate at Floorplan level instead of area level
3. Or add `FloorplanMaskedAreaId` to `AlarmTriggers` (schema change)

## Implementation Plan

### Phase 1: Migrate Simple Queries (No Area Filter)

**File**: `Shared/Repositories/Repository/Analytics/AlarmAnalyticsIncidentRepository.cs`

#### 1. `GetDailySummaryAsync()` (Line 185)

**Before**:
```csharp
var query = _context.AlarmRecordTrackings
    .AsNoTracking()
    .Where(a => a.Timestamp >= from && a.Timestamp <= to);

query = ApplyFilters(query, request);

var incidents = await query
    .Select(a => new
    {
        Date = a.Timestamp.Value.Date,
        a.AlarmTriggersId
    })
    .Distinct()
    .ToListAsync();
```

**After**:
```csharp
var query = _context.AlarmTriggers
    .AsNoTracking()
    .Where(a => a.TriggerTime >= from && a.TriggerTime <= to);

query = ApplyFiltersToAlarmTriggers(query, request);

var incidents = await query
    .Select(a => new
    {
        Date = a.TriggerTime.Value.Date,
        Id = a.Id
    })
    .ToListAsync();
// No need for Distinct() - each trigger is unique
```

#### 2. `GetStatusSummaryAsync()` (Line 220)

**Before**:
```csharp
var query = _context.AlarmRecordTrackings
    .AsNoTracking()
    .Where(a => a.Timestamp >= from && a.Timestamp <= to);

query = ApplyFilters(query, request);

var incidents = await query
    .Select(a => new
    {
        a.AlarmTriggersId,
        a.Alarm
    })
    .Distinct()
    .ToListAsync();

var grouped = incidents
    .GroupBy(x => x.Alarm)
    .Select(g => new AlarmStatusRead
    {
        Status = g.Key.HasValue ? g.Key.Value.ToString() : "Unknown",
        Total = g.Count()
    })
```

**After**:
```csharp
var query = _context.AlarmTriggers
    .AsNoTracking()
    .Where(a => a.TriggerTime >= from && a.TriggerTime <= to);

query = ApplyFiltersToAlarmTriggers(query, request);

var incidents = await query
    .Select(a => new
    {
        a.Id,
        a.Alarm
    })
    .ToListAsync();

var grouped = incidents
    .GroupBy(x => x.Alarm)
    .Select(g => new AlarmStatusRead
    {
        Status = g.Key.HasValue ? g.Key.Value.ToString() : "Unknown",
        Total = g.Count()
    })
```

#### 3. `GetVisitorSummaryAsync()` (Line 283)

**Before**:
```csharp
var query = _context.AlarmRecordTrackings
    .AsNoTracking()
    .Include(a => a.Visitor)
    .Where(a => a.Timestamp >= from && a.Timestamp <= to);

query = ApplyFilters(query, request);

var incidents = await query
    .Select(a => new
    {
        a.AlarmTriggersId,
        a.VisitorId,
        VisitorName = a.Visitor != null ? a.Visitor.Name : "Unknown"
    })
    .Distinct()
    .ToListAsync();
```

**After**:
```csharp
var query = _context.AlarmTriggers
    .AsNoTracking()
    .Include(a => a.Visitor)
    .Where(a => a.TriggerTime >= from && a.TriggerTime <= to);

query = ApplyFiltersToAlarmTriggers(query, request);

var incidents = await query
    .Select(a => new
    {
        a.Id,
        a.VisitorId,
        VisitorName = a.Visitor != null ? a.Visitor.Name : "Unknown"
    })
    .ToListAsync();
```

#### 4. `GetBuildingSummaryAsync()` (Line 327)

**Before**:
```csharp
var query = _context.AlarmRecordTrackings
    .AsNoTracking()
    .Include(a => a.FloorplanMaskedArea.Floorplan.Floor.Building)
    .Where(a => a.Timestamp >= from && a.Timestamp <= to);

query = ApplyFilters(query, request);

var incidents = await query
    .Select(a => new
    {
        a.AlarmTriggersId,
        BuildingId = a.FloorplanMaskedArea.Floorplan.Floor.Building.Id,
        BuildingName = a.FloorplanMaskedArea.Floorplan.Floor.Building.Name
    })
    .Distinct()
    .ToListAsync();
```

**After**:
```csharp
var query = _context.AlarmTriggers
    .AsNoTracking()
    .Include(a => a.Floorplan.Floor.Building)
    .Where(a => a.TriggerTime >= from && a.TriggerTime <= to);

query = ApplyFiltersToAlarmTriggers(query, request);

var incidents = await query
    .Select(a => new
    {
        a.Id,
        BuildingId = a.Floorplan.Floor.Building.Id,
        BuildingName = a.Floorplan.Floor.Building.Name
    })
    .ToListAsync();
```

#### 5. `GetHourlyStatusSummaryAsync()` (Line 365)

**Before**:
```csharp
var query = _context.AlarmRecordTrackings
    .AsNoTracking()
    .Where(a => a.Timestamp >= from && a.Timestamp <= to);

query = ApplyFilters(query, request);

// STEP 1: Ambil *alarm pertama* dari setiap AlarmTriggersId dalam 1 hari
var incidents = await query
    .GroupBy(a => a.AlarmTriggersId)
    .Select(g => new
    {
        AlarmTriggersId = g.Key,
        FirstTimestamp = g.Min(x => x.Timestamp),   // ambil jam pertama
        Status = g.OrderBy(x => x.Timestamp)
                  .First().Alarm.ToString() ?? "Unknown"
    })
    .ToListAsync();

// STEP 2: Konversi ke hour
var flattened = incidents
    .Select(x => new
    {
        Hour = x.FirstTimestamp.Value.Hour,
        x.Status
    })
    .ToList();
```

**After**:
```csharp
var query = _context.AlarmTriggers
    .AsNoTracking()
    .Where(a => a.TriggerTime >= from && a.TriggerTime <= to);

query = ApplyFiltersToAlarmTriggers(query, request);

// Direct query - no need for GroupBy + Min()
var incidents = await query
    .Select(a => new
    {
        Hour = a.TriggerTime.Value.Hour,
        Status = a.Alarm.ToString() ?? "Unknown"
    })
    .ToListAsync();

// No need for flattening step
```

### Phase 2: Handle Area-Level Queries

#### 6. `GetAreaDailySummaryAsync()` (Line 66)

**Challenge**: Needs area-level grouping but `AlarmTriggers` doesn't have `FloorplanMaskedAreaId`.

**Option A - Keep Current**: Continue using `AlarmRecordTracking` for area queries
- Pros: Works correctly, exact area filtering
- Cons: Still queries log table, but acceptable for this specific use case

**Option B - Aggregate by Floorplan**: Change to group by Floorplan instead of Area
- Pros: Uses `AlarmTriggers` directly
- Cons: Changes API behavior - may not be acceptable

**Recommended**: Option A - Keep `AlarmRecordTracking` for `GetAreaDailySummaryAsync` since:
- Area-specific analytics genuinely need the area relationship
- The performance impact is isolated to this one query
- Adding `FloorplanMaskedAreaId` to `AlarmTriggers` would be a schema change

### Phase 3: Update ApplyFilters Method

**New method** for `AlarmTriggers`:
```csharp
private IQueryable<AlarmTriggers> ApplyFiltersToAlarmTriggers(
    IQueryable<AlarmTriggers> query,
    AlarmAnalyticsFilter request)
{
    // Building filter via Floorplan → Floor → Building
    if (request.BuildingId.HasValue)
    {
        query = query.Where(a => a.Floorplan != null
            && a.Floorplan.Floor != null
            && a.Floorplan.Floor.BuildingId == request.BuildingId);
    }

    // Floor filter
    if (request.FloorId.HasValue)
    {
        query = query.Where(a => a.Floorplan != null
            && a.Floorplan.FloorId == request.FloorId);
    }

    // Note: FloorplanMaskedAreaId filter not supported for AlarmTriggers queries
    // If area filter is specified, return empty result or use alternative method
    if (request.FloorplanMaskedAreaId.HasValue)
    {
        // Option 1: Return empty (area filter not supported)
        return query.Where(x => false);

        // Option 2: Filter by floorplan (less precise)
        // var floorplanIds = _context.FloorplanMaskedAreas
        //     .Where(fma => fma.Id == request.FloorplanMaskedAreaId)
        //     .Select(fma => fma.FloorplanId);
        // query = query.Where(a => a.FloorplanId.HasValue && floorplanIds.Contains(a.FloorplanId.Value));
    }

    // Visitor filter
    if (request.VisitorId.HasValue)
    {
        query = query.Where(a => a.VisitorId == request.VisitorId);
    }

    // Operator filter
    if (!string.IsNullOrWhiteSpace(request.OperatorName))
    {
        query = query.Where(a => a.DoneBy == request.OperatorName);
    }

    // Building access filter for operators
    var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
    if (accessibleBuildingIds.Any())
    {
        query = query.Where(a => a.Floorplan != null
            && a.Floorplan.Floor != null
            && accessibleBuildingIds.Contains(a.Floorplan.Floor.BuildingId));
    }

    return query;
}
```

## Files to Modify

| File | Changes |
|------|---------|
| `Shared/Repositories/Repository/Analytics/AlarmAnalyticsIncidentRepository.cs` | Migrate 5 methods, add new ApplyFiltersToAlarmTriggers |

## Verification

1. Create test alarms and verify counts match between old and new queries
2. Test with different time ranges (daily, weekly, monthly)
3. Verify building access filter works for operators
4. Compare query execution times (should be ~5-10x faster)
5. Ensure all filters (BuildingId, FloorId, VisitorId, OperatorName) work correctly

## Rollback Plan

If issues arise:
1. Keep both old and new methods
2. Use feature flag to switch between them
3. Gradually migrate based on monitoring data
