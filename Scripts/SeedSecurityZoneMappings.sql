-- ============================================================================
-- Security Zone Mapping Seed Data Script
-- ============================================================================
-- Purpose: Seed security zone mappings for existing floorplan_masked_area records
-- Usage: Run this script manually or let DatabaseSeeder.cs handle it automatically
-- Date: 2025-02-08
-- ============================================================================

-- Insert security zone mappings for all active floorplan areas and applications
INSERT INTO security_zone_mapping (id, area_id, area_name, security_zone, requires_escort, allowed_from_zones, created_at, updated_at, application_id, status)
SELECT
    NEWID() AS id,
    fma.id AS area_id,
    fma.name AS area_name,
    CASE
        -- Public Zones (no access restrictions)
        WHEN LOWER(fma.name) LIKE '%lobby%' THEN 1
        WHEN LOWER(fma.name) LIKE '%cafeteria%' THEN 1
        WHEN LOWER(fma.name) LIKE '%reception%' THEN 1
        WHEN LOWER(fma.name) LIKE '%parking%' THEN 1
        WHEN LOWER(fma.name) LIKE '%entrance%' THEN 1

        -- Secure Zones (requires badge access)
        WHEN LOWER(fma.name) LIKE '%banking%' THEN 2
        WHEN LOWER(fma.name) LIKE '%office%' THEN 2
        WHEN LOWER(fma.name) LIKE '%meeting%' THEN 2
        WHEN LOWER(fma.name) LIKE '%conference%' THEN 2
        WHEN LOWER(fma.name) LIKE '%pantry%' THEN 2
        WHEN LOWER(fma.name) LIKE '%workstation%' THEN 2
        WHEN LOWER(fma.name) LIKE '%desk%' THEN 2

        -- Restricted Zones (requires special authorization)
        WHEN LOWER(fma.name) LIKE '%server%' THEN 3
        WHEN LOWER(fma.name) LIKE '%server room%' THEN 3
        WHEN LOWER(fma.name) LIKE '%vault%' THEN 3
        WHEN LOWER(fma.name) LIKE '%storage%' THEN 3
        WHEN LOWER(fma.name) LIKE '%restricted%' THEN 3
        WHEN LOWER(fma.name) LIKE '%archive%' THEN 3

        -- Critical Zones (highest security, requires escort)
        WHEN LOWER(fma.name) LIKE '%data center%' THEN 4
        WHEN LOWER(fma.name) LIKE '%datacenter%' THEN 4
        WHEN LOWER(fma.name) LIKE '%control room%' THEN 4
        WHEN LOWER(fma.name) LIKE '%generator%' THEN 4
        WHEN LOWER(fma.name) LIKE '%electrical room%' THEN 4
        WHEN LOWER(fma.name) LIKE '%server room%' THEN 4

        -- Default to Public for unspecified areas
        ELSE 1
    END AS security_zone,
    CASE
        -- Areas that require escort
        WHEN LOWER(fma.name) LIKE '%server%' THEN 1
        WHEN LOWER(fma.name) LIKE '%server room%' THEN 1
        WHEN LOWER(fma.name) LIKE '%vault%' THEN 1
        WHEN LOWER(fma.name) LIKE '%data center%' THEN 1
        WHEN LOWER(fma.name) LIKE '%datacenter%' THEN 1
        WHEN LOWER(fma.name) LIKE '%control room%' THEN 1
        WHEN LOWER(fma.name) LIKE '%generator%' THEN 1
        WHEN LOWER(fma.name) LIKE '%electrical room%' THEN 1
        ELSE 0
    END AS requires_escort,
    NULL AS allowed_from_zones, -- NULL = use default transition rules from SecurityZoneExtensions
    GETDATE() AS created_at,
    GETDATE() AS updated_at,
    app.id AS application_id,
    1 AS status
FROM floorplan_masked_area fma
CROSS JOIN mst_application app
WHERE fma.status != 0
  AND app.application_status != 0
  -- Avoid duplicate inserts if script is run multiple times
  AND NOT EXISTS (
      SELECT 1 FROM security_zone_mapping szm
      WHERE szm.area_id = fma.id
        AND szm.application_id = app.id
        AND szm.status != 0
  );

-- ============================================================================
-- Verification Query
-- ============================================================================
-- Run this after seeding to verify the data

-- Count mappings by security zone
SELECT
    szm.security_zone,
    CASE szm.security_zone
        WHEN 1 THEN 'Public'
        WHEN 2 THEN 'Secure'
        WHEN 3 THEN 'Restricted'
        WHEN 4 THEN 'Critical'
    END AS zone_name,
    COUNT(*) AS area_count,
    SUM(CAST(szm.requires_escort AS INT)) AS requires_escort_count
FROM security_zone_mapping szm
WHERE szm.status != 0
GROUP BY szm.security_zone
ORDER BY szm.security_zone;

-- Show sample mappings
SELECT TOP 20
    szm.area_name,
    CASE szm.security_zone
        WHEN 1 THEN 'Public'
        WHEN 2 THEN 'Secure'
        WHEN 3 THEN 'Restricted'
        WHEN 4 THEN 'Critical'
    END AS zone_name,
    szm.requires_escort,
    app.application_name
FROM security_zone_mapping szm
JOIN mst_application app ON szm.application_id = app.id
WHERE szm.status != 0
ORDER BY szm.security_zone, szm.area_name;

-- ============================================================================
-- Manual Override Examples
-- ============================================================================
-- If you need to manually update specific areas, use these examples:

-- Example 1: Mark a specific area as Restricted
-- UPDATE security_zone_mapping
-- SET security_zone = 3, requires_escort = 1, updated_at = GETDATE()
-- WHERE area_name = 'Executive Boardroom' AND application_id = '<your-app-id>';

-- Example 2: Set allowed transition zones
-- UPDATE security_zone_mapping
-- SET allowed_from_zones = '1,2', updated_at = GETDATE()
-- WHERE area_name = 'Server Room' AND application_id = '<your-app-id>';
-- This means only from Public (1) or Secure (2) zones

-- ============================================================================
-- Security Zone Reference
-- ============================================================================
-- 1 = Public    - No access restrictions
-- 2 = Secure    - Requires badge access
-- 3 = Restricted - Requires special authorization
-- 4 = Critical   - Highest security, requires escort
