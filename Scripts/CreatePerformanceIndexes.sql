-- ============================================================================
-- Performance Indexes for User Journey Analytics
-- ============================================================================
-- Purpose: Optimize database queries for User Journey & Security Check features
-- Usage: Run this script to add performance indexes
-- Date: 2025-02-08
-- ============================================================================

-- ============================================================================
-- WARNING: This script creates indexes on tracking_transaction_YYYYMMDD tables
-- You need to run this for each existing table, or modify the script to loop
-- through all tables dynamically.
-- ============================================================================

-- ============================================================================
-- 1. Basic Time-Based Indexes
-- ============================================================================

-- Index on trans_time for date range queries (used in ALL analytics)
CREATE NONCLUSTERED INDEX IX_tracking_transaction_trans_time
ON tracking_transaction_20250208(trans_time)
WITH (DATA_COMPRESSION = PAGE);

-- Index on trans_time DESC for most-recent queries
CREATE NONCLUSTERED INDEX IX_tracking_transaction_trans_time_desc
ON tracking_transaction_20250208(trans_time DESC)
WITH (DATA_COMPRESSION = PAGE);

-- ============================================================================
-- 2. Area-Based Indexes (for Dwell Time, Peak Hours, Occupancy)
-- ============================================================================

-- Composite index for area + time (most common query pattern)
CREATE NONCLUSTERED INDEX IX_tracking_transaction_area_time
ON tracking_transaction_20250208(floorplan_masked_area_id, trans_time)
INCLUDE (card_id)
WITH (DATA_COMPRESSION = PAGE);

-- Index for area-only queries
CREATE NONCLUSTERED INDEX IX_tracking_transaction_area
ON tracking_transaction_20250208(floorplan_masked_area_id)
WITH (DATA_COMPRESSION = PAGE);

-- ============================================================================
-- 3. Card-Based Indexes (for Visitor/Member journey tracking)
-- ============================================================================

-- Composite index for card + time (for session grouping)
CREATE NONCLUSTERED INDEX IX_tracking_transaction_card_time
ON tracking_transaction_20250208(card_id, trans_time)
INCLUDE (floorplan_masked_area_id)
WITH (DATA_COMPRESSION = PAGE);

-- Index for card-only queries
CREATE NONCLUSTERED INDEX IX_tracking_transaction_card
ON tracking_transaction_20250208(card_id)
WITH (DATA_COMPRESSION = PAGE);

-- ============================================================================
-- 4. Security Zone Mapping Indexes
-- ============================================================================

-- Index for area lookups in security zone validation
CREATE NONCLUSTERED INDEX IX_security_zone_mapping_area
ON security_zone_mapping(area_id)
WHERE status != 0;

-- Index for application filtering
CREATE NONCLUSTERED INDEX IX_security_zone_mapping_application
ON security_zone_mapping(application_id)
WHERE status != 0;

-- Composite index for area + application
CREATE NONCLUSTERED INDEX IX_security_zone_mapping_area_app
ON security_zone_mapping(area_id, application_id)
INCLUDE (security_zone, requires_escort, allowed_from_zones)
WHERE status != 0;

-- ============================================================================
-- 5. Supporting Table Indexes
-- ============================================================================

-- Floorplan Masked Area indexes
CREATE NONCLUSTERED INDEX IX_floorplan_masked_area_floorplan
ON floorplan_masked_area(floorplan_id)
WHERE status != 0;

CREATE NONCLUSTERED INDEX IX_floorplan_masked_area_status
ON floorplan_masked_area(status)
INCLUDE (id, name);

-- Card indexes for visitor/member lookups
CREATE NONCLUSTERED INDEX IX_card_visitor
ON card(visitor_id)
WHERE status_card != 0 AND visitor_id IS NOT NULL;

CREATE NONCLUSTERED INDEX IX_card_member
ON card(member_id)
WHERE status_card != 0 AND member_id IS NOT NULL;

-- Visitor indexes
CREATE NONCLUSTERED INDEX IX_visitor_application
ON visitor(application_id)
WHERE status != 0;

CREATE NONCLUSTERED INDEX IX_visitor_status
ON visitor(status)
INCLUDE (id, name);

-- Member indexes
CREATE NONCLUSTERED INDEX IX_mst_member_application
ON mst_member(application_id)
WHERE status != 0;

CREATE NONCLUSTERED INDEX IX_mst_member_status
ON mst_member(status)
INCLUDE (id, name);

-- ============================================================================
-- 6. Filtered Indexes for Common Queries
-- ============================================================================

-- Only active transactions (if you have a status column)
-- CREATE NONCLUSTERED INDEX IX_tracking_transaction_active
-- ON tracking_transaction_20250208(trans_time, floorplan_masked_area_id)
-- WHERE status = 1;

-- Only recent transactions (last 30 days) - requires periodic maintenance
-- CREATE NONCLUSTERED INDEX IX_tracking_transaction_recent
-- ON tracking_transaction_20250208(trans_time DESC, floorplan_masked_area_id)
-- WHERE trans_time > DATEADD(day, -30, GETDATE());

-- ============================================================================
-- 7. Columnstore Index for Analytics (Optional - for large datasets)
-- ============================================================================

-- Columnstore index for aggregation queries (COUNT, AVG, etc.)
-- Only create if you have 1M+ rows per day
-- CREATE NONCLUSTERED COLUMNSTORE INDEX IX_tracking_transaction_columnstore
-- ON tracking_transaction_20250208(trans_time, floorplan_masked_area_id, card_id);

-- ============================================================================
-- 8. Update Statistics (Critical for Query Performance)
-- ============================================================================

UPDATE STATISTICS tracking_transaction_20250208 WITH FULLSCAN;
UPDATE STATISTICS security_zone_mapping WITH FULLSCAN;
UPDATE STATISTICS floorplan_masked_area WITH FULLSCAN;
UPDATE STATISTICS card WITH FULLSCAN;
UPDATE STATISTICS visitor WITH FULLSCAN;
UPDATE STATISTICS mst_member WITH FULLSCAN;

-- ============================================================================
-- 9. Dynamic Index Creation for All Tables
-- ============================================================================

-- Uncomment and run this to create indexes on ALL tracking_transaction tables

/*
DECLARE @tableName NVARCHAR(128);
DECLARE @sql NVARCHAR(MAX);

DECLARE table_cursor CURSOR FOR
SELECT name
FROM sys.tables
WHERE name LIKE 'tracking_transaction_%'
ORDER BY name;

OPEN table_cursor;

FETCH NEXT FROM table_cursor INTO @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Create area + time index
    SET @sql = N'
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = ''IX_tracking_transaction_area_time'' AND object_id = OBJECT_ID(''' + @tableName + '''))
BEGIN
    CREATE NONCLUSTERED INDEX IX_tracking_transaction_area_time
    ON ' + @tableName + '(floorplan_masked_area_id, trans_time)
    INCLUDE (card_id)
    WITH (DATA_COMPRESSION = PAGE, ONLINE = ON);
END';

    EXEC sp_executesql @sql;

    -- Create card + time index
    SET @sql = N'
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = ''IX_tracking_transaction_card_time'' AND object_id = OBJECT_ID(''' + @tableName + '''))
BEGIN
    CREATE NONCLUSTERED INDEX IX_tracking_transaction_card_time
    ON ' + @tableName + '(card_id, trans_time)
    INCLUDE (floorplan_masked_area_id)
    WITH (DATA_COMPRESSION = PAGE, ONLINE = ON);
END';

    EXEC sp_executesql @sql;

    -- Update statistics
    SET @sql = N'UPDATE STATISTICS ' + @tableName + ' WITH FULLSCAN;';
    EXEC sp_executesql @sql;

    FETCH NEXT FROM table_cursor INTO @tableName;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;
*/

-- ============================================================================
-- 10. Index Usage Monitoring
-- ============================================================================

-- Query to check index usage after implementation
SELECT
    t.name AS table_name,
    i.name AS index_name,
    i.type_desc AS index_type,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates,
    s.last_user_seek,
    s.last_user_scan
FROM sys.indexes i
JOIN sys.tables t ON i.object_id = t.object_id
LEFT JOIN sys.dm_db_index_usage_stats s
    ON i.object_id = s.object_id
    AND i.index_id = s.index_id
    AND s.database_id = DB_ID()
WHERE t.name IN ('tracking_transaction_20250208', 'security_zone_mapping', 'card', 'visitor', 'mst_member')
ORDER BY t.name, i.name;

-- ============================================================================
-- 11. Missing Indexes Detection
-- ============================================================================

-- Query to find missing indexes (run after system has been in use for a while)
SELECT TOP 20
    CONVERT(VARCHAR(30), OBJECT_NAME(mid.object_id)) AS table_name,
    mid.equality_columns,
    mid.inequality_columns,
    mid.included_columns,
    migs.user_seeks,
    migs.avg_user_impact,
    'CREATE NONCLUSTERED INDEX IX_' + REPLACE(REPLACE(
        COALESCE(mid.equality_columns, '') +
        CASE WHEN mid.inequality_columns IS NOT NULL THEN '_' ELSE '' END +
        COALESCE(mid.inequality_columns, ''), ', ', '_'), ']', '')
    + ' ON ' + mid.statement +
    ' (' + COALESCE(mid.equality_columns, '') +
    CASE WHEN mid.inequality_columns IS NOT NULL
        THEN COALESCE(', ' + mid.inequality_columns, '')
        ELSE '' END
    + ')' +
    CASE WHEN mid.included_columns IS NOT NULL
        THEN ' INCLUDE (' + mid.included_columns + ')'
        ELSE '' END AS create_index_statement
FROM sys.dm_db_missing_index_details mid
JOIN sys.dm_db_missing_index_groups mig ON mid.index_handle = mig.index_handle
JOIN sys.dm_db_missing_index_group_stats migs ON mig.index_group_handle = migs.group_handle
WHERE mid.database_id = DB_ID()
    AND OBJECT_NAME(mid.object_id) LIKE 'tracking_transaction_%'
ORDER BY migs.avg_user_impact * migs.user_seeks DESC;

-- ============================================================================
-- 12. Index Maintenance Job
-- ============================================================================

-- Rebuild fragmented indexes (run weekly)
ALTER INDEX ALL ON tracking_transaction_20250208 REBUILD WITH (ONLINE = ON);
ALTER INDEX ALL ON security_zone_mapping REBUILD;
ALTER INDEX ALL ON card REBUILD;
ALTER INDEX ALL ON visitor REBUILD;
ALTER INDEX ALL ON mst_member REBUILD;

-- ============================================================================
-- Expected Performance Improvements
-- ============================================================================

-- Before indexes:
-- - Common Paths query: 10-30 seconds (10K sessions)
-- - Security Check query: 2-5 seconds per visitor

-- After indexes:
-- - Common Paths query: 1-3 seconds (10K sessions)
-- - Security Check query: 100-300ms per visitor

-- ============================================================================
-- Notes
-- ============================================================================

-- 1. These indexes are optimized for the User Journey Analytics queries
-- 2. Column compression (PAGE) reduces storage and improves I/O
-- 3. ONLINE = ON allows index creation without locking tables
-- 4. Filtered indexes (WHERE status != 0) reduce index size
-- 5. Update statistics after bulk data loads
-- 6. Monitor index usage and remove unused indexes
-- 7. Schedule regular index maintenance (rebuild/reorganize)

-- ============================================================================
-- End of Script
-- ============================================================================
