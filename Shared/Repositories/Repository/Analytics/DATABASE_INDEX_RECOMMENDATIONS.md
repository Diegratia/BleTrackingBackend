# Database Index Recommendations for Tracking Analytics

## Overview

This document contains recommended database indexes for optimizing the tracking analytics queries in the BLE Tracking Backend system. These indexes are designed to improve query performance for time-based filtering and joins.

## Partitioned Table Indexes

The `tracking_transaction_YYYYMMDD` tables are partitioned by date. For optimal performance, each partitioned table should have the following indexes:

### 1. Time-Based Query Index (Priority: HIGH)

```sql
-- For each partitioned table: tracking_transaction_YYYYMMDD
CREATE NONCLUSTERED INDEX IX_tracking_transaction_trans_time
ON [dbo].[tracking_transaction_YYYYMMDD] (trans_time);
```

**Purpose**: Optimizes the `WHERE t.trans_time >= @from AND t.trans_time <= @to` filter used in all queries.

**Impact**: High - All analytics queries filter by `trans_time` range.

### 2. Card ID Index (Priority: MEDIUM)

```sql
CREATE NONCLUSTERED INDEX IX_tracking_transaction_card_id
ON [dbo].[tracking_transaction_YYYYMMDD] (card_id);
```

**Purpose**: Optimizes JOIN with the `card` table.

**Impact**: Medium - Used in all queries to get visitor/member information.

### 3. Floorplan Masked Area ID Index (Priority: MEDIUM)

```sql
CREATE NONCLUSTERED INDEX IX_tracking_transaction_floorplan_masked_area_id
ON [dbo].[tracking_transaction_YYYYMMDD] (floorplan_masked_area_id)
WHERE floorplan_masked_area_id IS NOT NULL;
```

**Purpose**: Optimizes filtering by area and JOIN with `floorplan_masked_area` table.

**Impact**: Medium - Used when filtering by specific areas.

### 4. Coordinate Index for Visual Paths (Priority: LOW)

```sql
CREATE NONCLUSTERED INDEX IX_tracking_transaction_coordinates
ON [dbo].[tracking_transaction_YYYYMMDD] (coordinate_x, coordinate_y)
WHERE coordinate_x IS NOT NULL AND coordinate_y IS NOT NULL;
```

**Purpose**: Optimizes visual path queries that filter for valid coordinates.

**Impact**: Low - Only used for visual path visualization.

## Core Table Indexes

### AlarmTriggers Table

```sql
-- Time-based query index
CREATE NONCLUSTERED INDEX IX_AlarmTriggers_TriggerTime
ON [dbo].[alarm_triggers] (trigger_time);

-- Composite index for incident matching (with FloorplanId)
CREATE NONCLUSTERED INDEX IX_AlarmTriggers_TriggerTime_FloorplanId
ON [dbo].[alarm_triggers] (trigger_time, floorplan_id);

-- Index for visitor/member filtering
CREATE NONCLUSTERED INDEX IX_AlarmTriggers_VisitorId
ON [dbo].[alarm_triggers] (visitor_id)
WHERE visitor_id IS NOT NULL;

CREATE NONCLUSTERED INDEX IX_AlarmTriggers_MemberId
ON [dbo].[alarm_triggers] (member_id)
WHERE member_id IS NOT NULL;
```

### FloorplanMaskedAreas Table

```sql
-- Index for status filtering (used in incident matching)
CREATE NONCLUSTERED INDEX IX_FloorplanMaskedAreas_Status_FloorplanId
ON [dbo].[floorplan_masked_areas] (status, floorplan_id)
WHERE status != 0;
```

## Index Creation Script for All Partitions

To create indexes on all existing partitioned tables dynamically:

```sql
DECLARE @sql NVARCHAR(MAX) = '';
DECLARE @tableName NVARCHAR(128);

DECLARE cursor_tables CURSOR FOR
SELECT t.name
FROM sys.tables t
WHERE t.name LIKE 'tracking_transaction_%'
AND t.name NOT LIKE '%_index_%'
AND t.is_ms_shipped = 0;

OPEN cursor_tables;
FETCH NEXT FROM cursor_tables INTO @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Create trans_time index
    SET @sql = N'CREATE NONCLUSTERED INDEX IX_tracking_transaction_trans_time
    ON [dbo].[' + @tableName + '] (trans_time);';

    EXEC sp_executesql @sql;

    -- Create card_id index
    SET @sql = N'CREATE NONCLUSTERED INDEX IX_tracking_transaction_card_id
    ON [dbo].[' + @tableName + '] (card_id);';

    EXEC sp_executesql @sql;

    -- Create floorplan_masked_area_id index
    SET @sql = N'CREATE NONCLUSTERED INDEX IX_tracking_transaction_floorplan_masked_area_id
    ON [dbo].[' + @tableName + '] (floorplan_masked_area_id)
    WHERE floorplan_masked_area_id IS NOT NULL;';

    EXEC sp_executesql @sql;

    FETCH NEXT FROM cursor_tables INTO @tableName;
END

CLOSE cursor_tables;
DEALLOCATE cursor_tables;
```

## Maintenance Recommendations

### 1. Index Fragmentation Monitoring

```sql
-- Check index fragmentation
SELECT
    OBJECT_NAME(ind.OBJECT_ID) AS TableName,
    ind.name AS IndexName,
    indexstats.avg_fragmentation_in_percent
FROM
    sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, NULL) indexstats
INNER JOIN
    sys.indexes ind ON ind.object_id = indexstats.object_id
WHERE
    OBJECT_NAME(ind.OBJECT_ID) LIKE 'tracking_transaction_%'
ORDER BY
    avg_fragmentation_in_percent DESC;
```

### 2. Rebuild/Reorganize Schedule

For partitioned tables with high write volume:
- **Reorganize** indexes when fragmentation is between 5-30%
- **Rebuild** indexes when fragmentation is > 30%
- Schedule maintenance during off-peak hours (nightly for high-volume systems)

### 3. Partition Management

- Create partitions for future dates at least 1 week in advance
- Drop/archived partitions older than retention period
- Consider table partitioning for better management

## Performance Monitoring

### Query Performance Tracking

```sql
-- Enable query statistics
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

-- Run your query
SELECT ... FROM tracking_transaction_YYYYMMDD ...

-- Check the output for:
-- - Logical reads (should be low with proper indexes)
-- - CPU time and elapsed time
```

### Missing Indexes Detection

```sql
SELECT
    CONVERT(VARCHAR(30), OBJECT_NAME(mid.object_id)) AS TableName,
    mid.equality_columns,
    mid.inequality_columns,
    mid.included_columns,
    migs.user_seeks,
    migs.avg_user_impact
FROM
    sys.dm_db_missing_index_details mid
INNER JOIN
    sys.dm_db_missing_index_groups mig ON mid.index_handle = mig.index_handle
INNER JOIN
    sys.dm_db_missing_index_group_stats migs ON mig.index_group_handle = migs.group_handle
WHERE
    OBJECT_NAME(mid.object_id) LIKE 'tracking_transaction_%'
ORDER BY
    migs.avg_user_impact * migs.user_seeks DESC;
```

## Summary of Impact

| Index | Priority | Expected Impact | Notes |
|-------|----------|-----------------|-------|
| `IX_tracking_transaction_trans_time` | HIGH | 50-70% query time reduction | All queries use time filtering |
| `IX_tracking_transaction_card_id` | MEDIUM | 10-20% query time reduction | Improves JOIN performance |
| `IX_tracking_transaction_floorplan_masked_area_id` | MEDIUM | 5-15% query time reduction | Helps area filtering |
| `IX_AlarmTriggers_TriggerTime` | HIGH | 30-50% alarm query time reduction | Critical for incident matching |
| `IX_AlarmTriggers_TriggerTime_FloorplanId` | MEDIUM | 10-20% alarm query time reduction | Optimizes composite queries |
| `IX_FloorplanMaskedAreas_Status_FloorplanId` | LOW | 5-10% incident matching reduction | Small optimization |

## Last Updated

- Date: 2025-02-09
- Reviewed by: System Optimization
- Applies to: BLE Tracking Backend v1.0+
