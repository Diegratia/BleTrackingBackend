-- ============================================================================
-- Migration Verification Script - User Journey Analytics
-- ============================================================================
-- Purpose: Verify that the AddSecurityZoneMapping migration was applied correctly
-- Usage: Run this script after migration to verify database schema
-- Date: 2025-02-08
-- ============================================================================

PRINT '';
PRINT '============================================================';
PRINT 'User Journey Analytics - Migration Verification';
PRINT '============================================================';
PRINT '';

-- ============================================================================
-- 1. Check if security_zone_mapping table exists
-- ============================================================================

PRINT '1. Checking security_zone_mapping table...';

IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'security_zone_mapping'
)
BEGIN
    PRINT '   ✓ Table security_zone_mapping EXISTS';
END
ELSE
BEGIN
    PRINT '   ✗ ERROR: Table security_zone_mapping NOT FOUND!';
    PRINT '   Run migration: dotnet ef database update';
END

PRINT '';

-- ============================================================================
-- 2. Check table structure
-- ============================================================================

PRINT '2. Checking table structure...';

-- Check all required columns
DECLARE @columnCheck TABLE (
    column_name NVARCHAR(128),
    exists bit
);

INSERT INTO @columnCheck
VALUES
    ('id', 0),
    ('area_id', 0),
    ('area_name', 0),
    ('security_zone', 0),
    ('requires_escort', 0),
    ('allowed_from_zones', 0),
    ('application_id', 0),
    ('created_at', 0),
    ('updated_at', 0),
    ('created_by', 0),
    ('updated_by', 0),
    ('status', 0);

-- Update existence flags
UPDATE c
SET exists = CASE WHEN EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'security_zone_mapping'
    AND COLUMN_NAME = c.column_name
) THEN 1 ELSE 0 END
FROM @columnCheck c;

-- Display results
DECLARE @missingColumns NVARCHAR(MAX) = '';
DECLARE @allColumnsExist bit = 1;

DECLARE col_cursor CURSOR FOR
SELECT column_name, exists
FROM @columnCheck;

DECLARE @colName NVARCHAR(128);
DECLARE @colExists bit;

OPEN col_cursor;
FETCH NEXT FROM col_cursor INTO @colName, @colExists;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF @colExists = 1
        PRINT '   ✓ Column ' + @colName + ' exists';
    ELSE
    BEGIN
        PRINT '   ✗ Column ' + @colName + ' MISSING!';
        SET @missingColumns = @missingColumns + @colName + ', ';
        SET @allColumnsExist = 0;
    END

    FETCH NEXT FROM col_cursor INTO @colName, @colExists;
END

CLOSE col_cursor;
DEALLOCATE col_cursor;

IF @allColumnsExist = 0
BEGIN
    PRINT '';
    PRINT '   ERROR: Missing columns: ' + LEFT(@missingColumns, LEN(@missingColumns) - 1);
    PRINT '   Run migration again or check migration script.';
END

PRINT '';

-- ============================================================================
-- 3. Check data types
-- ============================================================================

PRINT '3. Checking data types...';

DECLARE @dataTypeErrors NVARCHAR(MAX) = '';

-- Check critical data types
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'security_zone_mapping'
    AND COLUMN_NAME = 'area_id'
    AND DATA_TYPE NOT IN ('uniqueidentifier', 'guid')
)
BEGIN
    SET @dataTypeErrors = @dataTypeErrors + 'area_id should be uniqueidentifier, ';
END

IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'security_zone_mapping'
    AND COLUMN_NAME = 'security_zone'
    AND DATA_TYPE NOT IN ('int', 'integer')
)
BEGIN
    SET @dataTypeErrors = @dataTypeErrors + 'security_zone should be int, ';
END

IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'security_zone_mapping'
    AND COLUMN_NAME = 'requires_escort'
    AND DATA_TYPE NOT IN ('bit', 'boolean')
)
BEGIN
    SET @dataTypeErrors = @dataTypeErrors + 'requires_escort should be bit, ';
END

IF LEN(@dataTypeErrors) > 0
BEGIN
    PRINT '   ✗ Data type errors: ' + LEFT(@dataTypeErrors, LEN(@dataTypeErrors) - 2);
END
ELSE
BEGIN
    PRINT '   ✓ All data types are correct';
END

PRINT '';

-- ============================================================================
-- 4. Check indexes
-- ============================================================================

PRINT '4. Checking indexes...';

-- Check if indexes exist
DECLARE @indexCount int;

SELECT @indexCount = COUNT(*)
FROM sys.indexes
WHERE object_id = OBJECT_ID('security_zone_mapping')
AND is_primary_key = 0
AND name IS NOT NULL;

IF @indexCount > 0
BEGIN
    PRINT '   ✓ Found ' + CAST(@indexCount AS NVARCHAR(10)) + ' indexes on security_zone_mapping';

    -- List all indexes
    SELECT
        '   - ' + name + ' (' + type_desc + ')' AS index_info
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('security_zone_mapping')
    AND is_primary_key = 0
    AND name IS NOT NULL
    ORDER BY name;
END
ELSE
BEGIN
    PRINT '   ⚠ No indexes found. Performance may be degraded.';
    PRINT '   Consider creating indexes on: area_id, application_id';
END

PRINT '';

-- ============================================================================
-- 5. Check query filter (soft delete)
-- ============================================================================

PRINT '5. Checking query filter for soft delete...';

-- Query filters are set in EF Core, not visible in SQL Server metadata
-- We can test it by running a query

DECLARE @hasActiveRecords bit;
DECLARE @hasInactiveRecords bit;

SELECT @hasActiveRecords = CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
FROM security_zone_mapping WITH (NOLOCK)
WHERE status = 1;

SELECT @hasInactiveRecords = CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
FROM security_zone_mapping WITH (NOLOCK)
WHERE status = 0;

IF @hasActiveRecords = 1
    PRINT '   ✓ Found active records (status = 1)';
ELSE
    PRINT '   ⚠ No active records found. Have you seeded data?';

IF @hasInactiveRecords = 1
    PRINT '   ✓ Found inactive records (status = 0) - soft delete is working';

PRINT '';

-- ============================================================================
-- 6. Check seed data
-- ============================================================================

PRINT '6. Checking seed data...';

DECLARE @totalRecords int;
DECLARE @activeRecords int;
DECLARE @applicationCount int;
DECLARE @areaCount int;

SELECT
    @totalRecords = COUNT(*),
    @activeRecords = COUNT(CASE WHEN status = 1 THEN 1 END)
FROM security_zone_mapping WITH (NOLOCK);

SELECT @applicationCount = COUNT(DISTINCT application_id)
FROM security_zone_mapping WITH (NOLOCK)
WHERE status = 1;

SELECT @areaCount = COUNT(DISTINCT area_id)
FROM security_zone_mapping WITH (NOLOCK)
WHERE status = 1;

PRINT '   Total records: ' + CAST(@totalRecords AS NVARCHAR(10));
PRINT '   Active records: ' + CAST(@activeRecords AS NVARCHAR(10));
PRINT '   Applications: ' + CAST(@applicationCount AS NVARCHAR(10));
PRINT '   Areas: ' + CAST(@areaCount AS NVARCHAR(10));

IF @activeRecords = 0
BEGIN
    PRINT '';
    PRINT '   ⚠ WARNING: No active records found!';
    PRINT '   Run seed script: Scripts/SeedSecurityZoneMappings.sql';
    PRINT '   Or trigger DatabaseSeeder.Seed()';
END
ELSE
BEGIN
    PRINT '   ✓ Seed data exists';
END

PRINT '';

-- ============================================================================
-- 7. Check foreign key relationships
-- ============================================================================

PRINT '7. Checking foreign key relationships...';

-- Check if FK to floorplan_masked_area exists (optional, not enforced in EF)
DECLARE @fkToArea bit;
DECLARE @fkToApplication bit;

SELECT @fkToArea = CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('security_zone_mapping')
AND referenced_object_id = OBJECT_ID('floorplan_masked_area');

SELECT @fkToApplication = CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('security_zone_mapping')
AND referenced_object_id = OBJECT_ID('mst_application');

IF @fkToArea = 1
    PRINT '   ✓ FK to floorplan_masked_area exists';
ELSE
    PRINT '   ℹ No FK to floorplan_masked_area (not enforced in EF)';

IF @fkToApplication = 1
    PRINT '   ✓ FK to mst_application exists';
ELSE
    PRINT '   ℹ No FK to mst_application (not enforced in EF)';

PRINT '';

-- ============================================================================
-- 8. Sample data verification
-- ============================================================================

PRINT '8. Sample data verification...';

-- Show sample records
PRINT '   Sample records (top 5):';

SELECT TOP 5
    '   - ' +
    szm.area_name +
    ' (' + CASE szm.security_zone
        WHEN 1 THEN 'Public'
        WHEN 2 THEN 'Secure'
        WHEN 3 THEN 'Restricted'
        WHEN 4 THEN 'Critical'
        ELSE 'Unknown'
    END +
    ', Escort=' + CASE WHEN szm.requires_escort = 1 THEN 'Yes' ELSE 'No' END +
    ', App=' + ISNULL(app.application_name, 'N/A') +
    ')' AS sample_record
FROM security_zone_mapping szm WITH (NOLOCK)
LEFT JOIN mst_application app ON szm.application_id = app.id
WHERE szm.status = 1
ORDER BY szm.area_name;

PRINT '';

-- ============================================================================
-- 9. Test basic query
-- ============================================================================

PRINT '9. Testing basic query performance...';

DECLARE @startTime datetime;
DECLARE @endTime datetime;
DECLARE @queryDuration int;

SET @startTime = GETDATE();

-- Test query: Get zone by area ID
DECLARE @testAreaId uniqueidentifier;
SELECT TOP 1 @testAreaId = area_id
FROM security_zone_mapping WITH (NOLOCK)
WHERE status = 1;

IF @testAreaId IS NOT NULL
BEGIN
    DECLARE @testZone int;
    SELECT @testZone = security_zone
    FROM security_zone_mapping WITH (NOLOCK)
    WHERE area_id = @testAreaId
    AND status = 1;

    SET @endTime = GETDATE();
    SET @queryDuration = DATEDIFF(millisecond, @startTime, @endTime);

    PRINT '   ✓ Query executed successfully in ' + CAST(@queryDuration AS NVARCHAR(10)) + 'ms';

    IF @queryDuration > 100
        PRINT '   ⚠ Query took > 100ms. Consider adding indexes.';
END
ELSE
BEGIN
    PRINT '   ⚠ No data to test query. Seed data first.';
END

PRINT '';

-- ============================================================================
-- 10. Migration history check
-- ============================================================================

PRINT '10. Checking migration history...';

IF EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE name = '__EFMigrationsHistory'
)
BEGIN
    DECLARE @migrationApplied bit;
    DECLARE @migrationDate datetime;

    SELECT
        @migrationApplied = CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END,
        @migrationDate = MAX(MigrationId)
    FROM __EFMigrationsHistory
    WHERE MigrationId LIKE '%AddSecurityZoneMapping%';

    IF @migrationApplied = 1
        PRINT '   ✓ Migration AddSecurityZoneMapping found in history';
    ELSE
        PRINT '   ⚠ Migration not found in __EFMigrationsHistory';
END
ELSE
BEGIN
    PRINT '   ℹ __EFMigrationsHistory table not found (normal for manual scripts)';
END

PRINT '';

-- ============================================================================
-- 11. Security Zone Mapping Count by Zone
-- ============================================================================

PRINT '11. Security zone distribution...';

SELECT
    '   ' +
    CASE szm.security_zone
        WHEN 1 THEN 'Public'
        WHEN 2 THEN 'Secure'
        WHEN 3 THEN 'Restricted'
        WHEN 4 THEN 'Critical'
        ELSE 'Unknown'
    END + ': ' + CAST(COUNT(*) AS NVARCHAR(10)) + ' areas' AS zone_distribution
FROM security_zone_mapping szm WITH (NOLOCK)
WHERE szm.status = 1
GROUP BY szm.security_zone
ORDER BY szm.security_zone;

PRINT '';

-- ============================================================================
-- 12. Final Verdict
-- ============================================================================

PRINT '============================================================';
PRINT 'VERDICT:';

IF @allColumnsExist = 1 AND LEN(@dataTypeErrors) = 0 AND @activeRecords > 0
BEGIN
    PRINT '✓✓✓ ALL CHECKS PASSED ✓✓✓';
    PRINT '';
    PRINT 'Your database is ready for User Journey Analytics!';
    PRINT '';
    PRINT 'Next steps:';
    PRINT '1. Review missing indexes warning (if any)';
    PRINT '2. Test API endpoints (see docs/api-testing-guide.md)';
    PRINT '3. Monitor query performance and adjust indexes as needed';
END
ELSE IF @allColumnsExist = 0 OR LEN(@dataTypeErrors) > 0
BEGIN
    PRINT '✗✗✗ SCHEMA ISSUES DETECTED ✗✗✗';
    PRINT '';
    PRINT 'Issues found:';
    IF @allColumnsExist = 0
        PRINT '- Missing columns (see section 2)';
    IF LEN(@dataTypeErrors) > 0
        PRINT '- Data type errors (see section 3)';
    PRINT '';
    PRINT 'Action required:';
    PRINT '1. Re-run migration: dotnet ef database update';
    PRINT '2. Or apply migration SQL manually';
    PRINT '3. Re-run this verification script';
END
ELSE IF @activeRecords = 0
BEGIN
    PRINT '⚠⚠⚠ SCHEMA OK, BUT NO DATA ⚠⚠⚠';
    PRINT '';
    PRINT 'The table structure is correct, but no seed data exists.';
    PRINT '';
    PRINT 'Action required:';
    PRINT '1. Run seed script: Scripts/SeedSecurityZoneMappings.sql';
    PRINT '2. Or trigger DatabaseSeeder.Seed() in code';
    PRINT '3. Re-run this verification script';
END

PRINT '============================================================';
PRINT '';

-- ============================================================================
-- 13. Recommendations
-- ============================================================================

PRINT 'Recommendations:';

-- Check if areas without mappings exist
DECLARE @areasWithoutMappings int;
SELECT @areasWithoutMappings = COUNT(DISTINCT fma.id)
FROM floorplan_masked_area fma WITH (NOLOCK)
LEFT JOIN security_zone_mapping szm ON fma.id = szm.area_id AND szm.status = 1
WHERE fma.status != 0
AND szm.id IS NULL;

IF @areasWithoutMappings > 0
BEGIN
    PRINT '⚠ Found ' + CAST(@areasWithoutMappings AS NVARCHAR(10)) + ' areas without security zone mappings.';
    PRINT '   Run seed script to map all areas.';
END

-- Check for missing indexes
IF @indexCount < 3
BEGIN
    PRINT '⚠ Consider creating additional indexes for better performance:';
    PRINT '   See Scripts/CreatePerformanceIndexes.sql';
END

PRINT '';
PRINT 'End of verification.';
PRINT '============================================================';

-- ============================================================================
-- End of Script
-- ============================================================================
