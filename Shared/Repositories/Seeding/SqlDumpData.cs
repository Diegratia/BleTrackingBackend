using System;

namespace Repositories.Seeding
{
    public static class SqlDumpData
    {
        public const string SqlQuery = @"
/*
 Navicat Premium Data Transfer

 Source Server         : localhost,1433
 Source Server Type    : SQL Server
 Source Server Version : 16001000 (16.00.1000)
 Source Host           : localhost:1433
 Source Catalog        : people_tracking_db
 Source Schema         : dbo

 Target Server Type    : SQL Server
 Target Server Version : 16001000 (16.00.1000)
 File Encoding         : 65001

 Date: 25/02/2026 13:39:13
*/


-- ----------------------------
-- Table structure for __EFMigrationsHistory
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type IN ('U'))
	DROP TABLE [dbo].[__EFMigrationsHistory]
GO

CREATE TABLE [dbo].[__EFMigrationsHistory] (
  [MigrationId] nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [ProductVersion] nvarchar(32) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL
)
GO

ALTER TABLE [dbo].[__EFMigrationsHistory] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of __EFMigrationsHistory
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20251001031442_deleteTimeGroup1', N'8.0.13'), (N'20251002023847_GeofenceDuration', N'8.0.13'), (N'20251003062056_BoundaryType', N'8.0.13'), (N'20251003090033_addColortoTrigger', N'8.0.13'), (N'20251008103428_addVisitornMembertoTrans', N'8.0.13'), (N'20251009063539_addEnumonEngine', N'8.0.13'), (N'20251009074001_deleterequiredEngine', N'8.0.13'), (N'20251010032212_isIntegrationonUser', N'8.0.13'), (N'20251010061824_addMemberatAlarmRecord', N'8.0.13'), (N'20251013073038_cardIsBlockAdd', N'8.0.13'), (N'20251014031802_addblockatMember', N'8.0.13'), (N'20251020031322_persontype', N'8.0.13'), (N'20251020042358_removeEngineIdAtFloorplan', N'8.0.13'), (N'20251020044408_addEngineIdAtFloorplan', N'8.0.13'), (N'20251105090608_Floorplan-Floorplanmaskedarea', N'8.0.13'), (N'20251106082142_IsUnassigned', N'8.0.13'), (N'20251106105022_FloorplanDeviceNullable', N'8.0.13'), (N'20251118035341_BlacklistVisitorMember', N'8.0.13'), (N'20251118043334_BlacklistReasonField', N'8.0.13'), (N'20251120050426_DeleteBlacklistArea', N'8.0.13'), (N'20251120080316_AddReaderType', N'8.0.13'), (N'20251124110137_addPath', N'8.0.13'), (N'20251125061710_addCardNumberatTrxVisitor', N'8.0.13'), (N'20251208071933_FloorTransisition', N'8.0.13'), (N'20251211045626_VisitorMemberAlarmTrigger', N'8.0.13'), (N'20251215065902_AddFilterPresets', N'8.0.13'), (N'20251215105524_AddFilterPresetsAtt', N'8.0.13'), (N'20251216084513_AlarmTriggersTimeStampLogic', N'8.0.13'), (N'20251219043829_AlarmCooldownSettings', N'8.0.13'), (N'20251223044421_AddSecurity', N'8.0.13'), (N'20260109084820_PatrolArea', N'8.0.13'), (N'20260112081632_PatrolRouteAreas', N'8.0.13'), (N'20260115073432_PatrolAreaTimeGroups', N'8.0.13'), (N'20260115073849_TimeGroupTypes', N'8.0.13'), (N'20260123041054_PatrolPlanningEntities', N'8.0.13'), (N'20260123102629_DelTimeGroupFormAssignment', N'8.0.13'), (N'20260127024657_PatrolAction', N'8.0.13'), (N'20260127100628_PatrolCaseEnum', N'8.0.13'), (N'20260129044840_PatrolAssignmentUpdate', N'8.0.13'), (N'20260130102525_PatrolSessionSnapshot', N'8.0.13'), (N'20260202050011_CardSwapTransactions', N'8.0.13'), (N'20260202064642_CardStatus', N'8.0.13'), (N'20260202074512_identitytypeswap', N'8.0.13'), (N'20260202090426_cardSwapMode', N'8.0.13'), (N'20260202094637_swapCardNullable', N'8.0.13'), (N'20260204234720_userBaseModel', N'8.0.13'), (N'20260205014528_AddUserBuildingAccess', N'8.0.13'), (N'20260205065802_RefactorToGroupBuildingAccess', N'8.0.13'), (N'20260205103659_buildingIdOnMonitringConfig', N'8.0.13'), (N'20260206025157_MonitoringConfigManyToManyBuilding', N'8.0.13'), (N'20260207045847_AddSecurityActionTimestamps', N'8.0.13'), (N'20260208010428_AddSecurityZoneMapping', N'8.0.13'), (N'20260208110748_deletesecuritymapping', N'8.0.13'), (N'20260209030211_RenameEnRouteToDispatched', N'8.0.13'), (N'20260209081846_AddAlarmAcceptedAndInvestigatedDone', N'8.0.13'), (N'20260212045421_PatrolApprovalV2_Snapshot', N'8.0.13'), (N'20260212045514_patrolCaseTimeStamp', N'8.0.13'), (N'20260212045810_PatrolCaseApprovalHeadTimestamps_Fix', N'8.0.13'), (N'20260212094405_AddThreatLevelToPatrolCase', N'8.0.13'), (N'20260212120000_PatrolApprovalV2', N'8.0.13'), (N'20260213034420_AddPermissionFlags', N'8.0.13'), (N'20260213054732_AddMonitoringConfigPermissionFlags', N'8.0.13'), (N'20260223031253_InestiagtedEnumResult', N'8.0.13'), (N'20260225021804_AddTrxVisitorExtendedTime', N'8.0.13')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for alarm_category_settings
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[alarm_category_settings]') AND type IN ('U'))
	DROP TABLE [dbo].[alarm_category_settings]
GO

CREATE TABLE [dbo].[alarm_category_settings] (
  [id] uniqueidentifier  NOT NULL,
  [alarm_category] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [alarm_level_priority] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [alarm_color] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [is_enabled] int  NULL,
  [remarks] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [notify_interval_sec] int  NULL
)
GO

ALTER TABLE [dbo].[alarm_category_settings] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of alarm_category_settings
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[alarm_category_settings] ([id], [alarm_category], [alarm_level_priority], [alarm_color], [is_enabled], [remarks], [application_id], [created_by], [created_at], [updated_by], [updated_at], [notify_interval_sec]) VALUES (N'6DB94852-A778-42C2-94BF-3CBCDB3E413D', N'blacklist', N'high', N'#FF7A00', N'0', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2025-10-02 07:35:27.1176832', N'superadmin', N'2026-02-06 10:21:02.6757605', N'5'), (N'7FD0AC7B-2C5D-4C79-B50B-43623A583289', N'geofence', N'high', N'#FFCC00', N'1', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2025-10-02 07:35:27.1306182', N'superadmin', N'2026-02-10 06:31:10.4116256', N'60'), (N'37D5A7F1-9B37-44E2-AD8D-64A7682820C2', N'help', N'high', N'#228B22', N'1', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2025-10-02 07:35:27.0813968', N'superadmin', N'2026-02-10 06:31:12.6302748', N'60'), (N'2C4F34A4-5D7E-49FE-BA1F-770BBF48E76D', N'overpopulating', N'medium', N'#D633FF', N'1', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2025-10-02 07:35:27.1385482', N'superadmin', N'2026-02-10 06:31:17.7701721', N'60'), (N'54FAE749-F5FB-4EB8-9E71-8A77E37DB8C4', N'stayonarea', N'high', N'#00CFFF', N'1', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2025-10-02 07:35:27.1478413', N'superadmin', N'2026-02-10 06:31:19.5034327', N'60'), (N'B5336DB5-0E74-49C9-B00E-92287573FD2B', N'boundary', N'high', N'#5D3FD3', N'1', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2025-10-02 07:35:27.1579273', N'superadmin', N'2026-02-10 06:31:06.1246109', N'60'), (N'AA6B2D57-A1BF-4C42-998B-95BCBF05443D', N'expired', N'high', N'#C8B560', N'1', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2025-10-02 07:35:27.1032562', N'superadmin', N'2026-02-10 06:31:08.1559330', N'60'), (N'8E93A51D-9636-485A-B509-A2A143567234', N'wrongzone', N'high', N'#5D3FD3', N'1', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2025-10-02 07:35:27.0885808', N'superadmin', N'2026-02-10 06:31:22.8016234', N'60'), (N'DE77E500-8C05-47C7-8EA3-A48C51E24A09', N'cardaccess', N'high', N'#000000', N'0', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2025-10-02 07:35:27.1676930', N'superadmin', N'2026-02-10 04:29:47.3272478', N'5'), (N'4A80919E-F519-4174-AF2F-AA0656731052', N'block', N'medium', N'#0047FF', N'1', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2025-10-02 07:35:27.0678392', N'superadmin', N'2026-02-10 06:31:04.2518356', N'60'), (N'C9ED98AE-399D-4119-8866-C6F95A7AE6C3', N'lost', N'high', N'#FFCC00', N'1', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2025-10-02 07:35:27.1076722', N'superadmin', N'2026-02-10 06:31:14.5711310', N'60')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for alarm_record_tracking
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[alarm_record_tracking]') AND type IN ('U'))
	DROP TABLE [dbo].[alarm_record_tracking]
GO

CREATE TABLE [dbo].[alarm_record_tracking] (
  [id] uniqueidentifier  NOT NULL,
  [timestamp] datetime2(7)  NULL,
  [visitor_id] uniqueidentifier  NULL,
  [ble_reader_id] uniqueidentifier  NULL,
  [alarm_triggers_id] uniqueidentifier  NULL,
  [floorplan_masked_area_id] uniqueidentifier  NULL,
  [alarm_record_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [action] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [idle_timestamp] datetime2(7)  NULL,
  [done_timestamp] datetime2(7)  NULL,
  [cancel_timestamp] datetime2(7)  NULL,
  [waiting_timestamp] datetime2(7)  NULL,
  [investigated_timestamp] datetime2(7)  NULL,
  [investigated_done_at] datetime2(7)  NULL,
  [idle_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [done_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [cancel_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [waiting_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [investigated_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [investigated_result] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [FloorplanMaskedAreaId1] uniqueidentifier  NULL,
  [MstApplicationId] uniqueidentifier  NULL,
  [MstBleReaderId] uniqueidentifier  NULL,
  [VisitorId1] uniqueidentifier  NULL,
  [_generate] bigint  IDENTITY(1,1) NOT NULL,
  [member_id] uniqueidentifier  NULL,
  [MstSecurityId] uniqueidentifier  NULL,
  [investigated_notes] nvarchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[alarm_record_tracking] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of alarm_record_tracking
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[alarm_record_tracking] ON
GO

SET IDENTITY_INSERT [dbo].[alarm_record_tracking] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for alarm_triggers
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[alarm_triggers]') AND type IN ('U'))
	DROP TABLE [dbo].[alarm_triggers]
GO

CREATE TABLE [dbo].[alarm_triggers] (
  [id] uniqueidentifier  NOT NULL,
  [beacon_id] nvarchar(16) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [floorplan_id] uniqueidentifier  NULL,
  [pos_x] real  NULL,
  [pos_y] real  NULL,
  [is_in_restricted_area] bit  NULL,
  [first_gateway_id] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [second_gateway_id] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [first_distance] real  NULL,
  [second_distance] real  NULL,
  [trigger_time] datetime2(7)  NULL,
  [alarm_record_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [action] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [is_active] bit  NULL,
  [idle_timestamp] datetime2(7)  NULL,
  [done_timestamp] datetime2(7)  NULL,
  [cancel_timestamp] datetime2(7)  NULL,
  [waiting_timestamp] datetime2(7)  NULL,
  [investigated_timestamp] datetime2(7)  NULL,
  [investigated_done_at] datetime2(7)  NULL,
  [idle_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [done_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [cancel_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [waiting_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [investigated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [investigated_result] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [alarm_color] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [member_id] uniqueidentifier  NULL,
  [visitor_id] uniqueidentifier  NULL,
  [action_updated_at] datetime2(7)  NULL,
  [last_seen_at] datetime2(7)  NULL,
  [last_notified_at] datetime2(7)  NULL,
  [MstSecurityId] uniqueidentifier  NULL,
  [security_id] uniqueidentifier  NULL,
  [acknowledged_at] datetime2(7)  NULL,
  [acknowledged_by] nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [arrived_at] datetime2(7)  NULL,
  [arrived_by] nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [dispatched_at] datetime2(7)  NULL,
  [dispatched_by] nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [accepted_at] datetime2(7)  NULL,
  [accepted_by] nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [investigated_done_by] nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [investigated_notes] nvarchar(4000) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[alarm_triggers] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of alarm_triggers
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for boundary
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[boundary]') AND type IN ('U'))
	DROP TABLE [dbo].[boundary]
GO

CREATE TABLE [dbo].[boundary] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [area_shape] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [color] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [remarks] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [floorplan_id] uniqueidentifier  NULL,
  [floor_id] uniqueidentifier  NULL,
  [is_active] int  NOT NULL,
  [status] int  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [boundary_type] int DEFAULT 0 NOT NULL
)
GO

ALTER TABLE [dbo].[boundary] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of boundary
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for card
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[card]') AND type IN ('U'))
	DROP TABLE [dbo].[card]
GO

CREATE TABLE [dbo].[card] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [remarks] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [card_number] nvarchar(450) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [qr_code] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [dmac] nvarchar(450) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [is_multi_masked_area] bit  NULL,
  [registered_masked_area_id] uniqueidentifier  NULL,
  [is_used] bit  NULL,
  [last_used_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [visitor_id] uniqueidentifier  NULL,
  [member_id] uniqueidentifier  NULL,
  [checkin_at] datetime2(7)  NULL,
  [checkout_at] datetime2(7)  NULL,
  [status_card] int  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [card_group_id] uniqueidentifier  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [card_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [security_id] uniqueidentifier  NULL
)
GO

ALTER TABLE [dbo].[card] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of card
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[card] ([id], [name], [remarks], [type], [card_number], [qr_code], [dmac], [is_multi_masked_area], [registered_masked_area_id], [is_used], [last_used_by], [visitor_id], [member_id], [checkin_at], [checkout_at], [status_card], [application_id], [card_group_id], [created_by], [created_at], [updated_by], [updated_at], [card_status], [security_id]) VALUES (N'656C9149-57FA-4389-8D3B-10C998FFC20B', N'Card Visitor', NULL, N'ble', N'677028', NULL, N'BC57291F5FD0', N'1', NULL, N'0', NULL, N'AEFDBCF4-9F00-42A1-B592-B12681836ACD', NULL, NULL, NULL, N'1', N'C926D20B-A746-4492-9924-EB7EEE76305C', NULL, N'system', N'2026-02-25 13:23:04.0000000', N'system', N'2026-02-25 13:23:04.0000000', NULL, NULL), (N'2A123537-27AD-4BB3-9352-8FDEAF2FF8FE', N'Card Security', NULL, N'ble', N'677013', NULL, N'BC57291F5FC1', N'1', NULL, N'1', NULL, NULL, NULL, NULL, NULL, N'1', N'C926D20B-A746-4492-9924-EB7EEE76305C', NULL, N'system', N'2026-02-25 13:23:04.0000000', N'system', N'2026-02-25 13:23:04.0000000', N'used', N'E3D48AA6-35F8-4659-A6C3-2E37663EEB45'), (N'5C56544F-0B8F-4A0F-B54A-CDF2DF60AB19', N'Card Member', NULL, N'ble', N'465757', NULL, N'BC572905D5B9', N'1', NULL, N'1', NULL, NULL, N'AB4B8247-0DA2-43B2-8CD0-35E15D83F76E', NULL, NULL, N'1', N'C926D20B-A746-4492-9924-EB7EEE76305C', NULL, N'system', N'2026-02-25 13:23:04.0000000', N'system', N'2026-02-25 13:23:04.0000000', N'used', NULL)
GO

COMMIT
GO


-- ----------------------------
-- Table structure for card_access_masked_areas
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[card_access_masked_areas]') AND type IN ('U'))
	DROP TABLE [dbo].[card_access_masked_areas]
GO

CREATE TABLE [dbo].[card_access_masked_areas] (
  [card_access_id] uniqueidentifier  NOT NULL,
  [masked_area_id] uniqueidentifier  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL
)
GO

ALTER TABLE [dbo].[card_access_masked_areas] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of card_access_masked_areas
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for card_access_time_groups
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[card_access_time_groups]') AND type IN ('U'))
	DROP TABLE [dbo].[card_access_time_groups]
GO

CREATE TABLE [dbo].[card_access_time_groups] (
  [card_access_id] uniqueidentifier  NOT NULL,
  [time_group_id] uniqueidentifier  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL
)
GO

ALTER TABLE [dbo].[card_access_time_groups] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of card_access_time_groups
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for card_accesses
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[card_accesses]') AND type IN ('U'))
	DROP TABLE [dbo].[card_accesses]
GO

CREATE TABLE [dbo].[card_accesses] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [access_number] int  NULL,
  [remarks] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [access_scope] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[card_accesses] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of card_accesses
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for card_card_accesses
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[card_card_accesses]') AND type IN ('U'))
	DROP TABLE [dbo].[card_card_accesses]
GO

CREATE TABLE [dbo].[card_card_accesses] (
  [card_id] uniqueidentifier  NOT NULL,
  [card_access_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL
)
GO

ALTER TABLE [dbo].[card_card_accesses] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of card_card_accesses
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for card_groups
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[card_groups]') AND type IN ('U'))
	DROP TABLE [dbo].[card_groups]
GO

CREATE TABLE [dbo].[card_groups] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [remarks] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[card_groups] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of card_groups
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for card_record
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[card_record]') AND type IN ('U'))
	DROP TABLE [dbo].[card_record]
GO

CREATE TABLE [dbo].[card_record] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [card_id] uniqueidentifier  NULL,
  [visitor_id] uniqueidentifier  NULL,
  [member_id] uniqueidentifier  NULL,
  [timestamp] datetime2(7)  NOT NULL,
  [checkin_at] datetime2(7)  NULL,
  [checkout_at] datetime2(7)  NULL,
  [checkin_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [checkout_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [checkout_masked_area] uniqueidentifier  NULL,
  [checkin_masked_area] uniqueidentifier  NULL,
  [visitor_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [status] int  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [CardId1] uniqueidentifier  NULL,
  [MstMemberId] uniqueidentifier  NULL,
  [VisitorId1] uniqueidentifier  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [MstSecurityId] uniqueidentifier  NULL
)
GO

ALTER TABLE [dbo].[card_record] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of card_record
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[card_record] ([id], [name], [card_id], [visitor_id], [member_id], [timestamp], [checkin_at], [checkout_at], [checkin_by], [checkout_by], [checkout_masked_area], [checkin_masked_area], [visitor_type], [status], [application_id], [CardId1], [MstMemberId], [VisitorId1], [created_by], [created_at], [updated_by], [updated_at], [MstSecurityId]) VALUES (N'0AD02F4A-8349-36D5-5187-9CD8C6CE3C6C', N'visitor', N'656C9149-57FA-4389-8D3B-10C998FFC20B', N'AEFDBCF4-9F00-42A1-B592-B12681836ACD', NULL, N'2025-02-25 03:02:56.0000000', N'2025-02-25 03:02:56.0000000', NULL, N'system', NULL, NULL, NULL, N'visitor', N'1', N'C926D20B-A746-4492-9924-EB7EEE76305C', NULL, NULL, NULL, N'system', N'2025-02-25 03:02:56.0000000', N'system', N'2025-02-25 03:02:56.0000000', NULL)
GO

COMMIT
GO


-- ----------------------------
-- Table structure for card_swap_transaction
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[card_swap_transaction]') AND type IN ('U'))
	DROP TABLE [dbo].[card_swap_transaction]
GO

CREATE TABLE [dbo].[card_swap_transaction] (
  [id] uniqueidentifier  NOT NULL,
  [from_card_id] uniqueidentifier  NULL,
  [to_card_id] uniqueidentifier  NULL,
  [trx_visitor_id] uniqueidentifier  NULL,
  [visitor_id] uniqueidentifier  NOT NULL,
  [swap_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [card_swap_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [masked_area_id] uniqueidentifier  NULL,
  [swap_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [swap_chain_id] uniqueidentifier  NULL,
  [swap_sequence] int  NOT NULL,
  [identity_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [identity_value] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [completed_at] datetime2(7)  NULL,
  [executed_at] datetime2(7)  NULL,
  [swap_mode] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[card_swap_transaction] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of card_swap_transaction
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for evacuation_alerts
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[evacuation_alerts]') AND type IN ('U'))
	DROP TABLE [dbo].[evacuation_alerts]
GO

CREATE TABLE [dbo].[evacuation_alerts] (
  [id] uniqueidentifier  NOT NULL,
  [title] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [description] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [alert_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [trigger_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [triggered_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [started_at] datetime2(7)  NULL,
  [completed_at] datetime2(7)  NULL,
  [completion_notes] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [completed_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [total_required] int  NOT NULL,
  [total_evacuated] int  NOT NULL,
  [total_confirmed] int  NOT NULL,
  [total_safe] int  NOT NULL,
  [total_remaining] int  NOT NULL,
  [status] int  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[evacuation_alerts] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of evacuation_alerts
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for evacuation_assembly_points
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[evacuation_assembly_points]') AND type IN ('U'))
	DROP TABLE [dbo].[evacuation_assembly_points]
GO

CREATE TABLE [dbo].[evacuation_assembly_points] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [area_shape] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [color] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [remarks] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [floorplan_masked_area_id] uniqueidentifier  NULL,
  [floorplan_id] uniqueidentifier  NULL,
  [floor_id] uniqueidentifier  NULL,
  [is_active] int  NOT NULL,
  [status] int  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[evacuation_assembly_points] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of evacuation_assembly_points
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for evacuation_transactions
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[evacuation_transactions]') AND type IN ('U'))
	DROP TABLE [dbo].[evacuation_transactions]
GO

CREATE TABLE [dbo].[evacuation_transactions] (
  [id] uniqueidentifier  NOT NULL,
  [evacuation_alert_id] uniqueidentifier  NOT NULL,
  [evacuation_assembly_point_id] uniqueidentifier  NOT NULL,
  [person_category] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [member_id] uniqueidentifier  NULL,
  [visitor_id] uniqueidentifier  NULL,
  [security_id] uniqueidentifier  NULL,
  [card_id] uniqueidentifier  NULL,
  [person_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [detected_at] datetime2(7)  NOT NULL,
  [last_detected_at] datetime2(7)  NULL,
  [confirmed_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [confirmed_at] datetime2(7)  NULL,
  [confirmation_notes] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [status] int  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[evacuation_transactions] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of evacuation_transactions
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for floorplan_device
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[floorplan_device]') AND type IN ('U'))
	DROP TABLE [dbo].[floorplan_device]
GO

CREATE TABLE [dbo].[floorplan_device] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [type] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [floorplan_id] uniqueidentifier  NOT NULL,
  [access_cctv_id] uniqueidentifier  NULL,
  [ble_reader_id] uniqueidentifier  NULL,
  [access_control_id] uniqueidentifier  NULL,
  [pos_x] real  NOT NULL,
  [pos_y] real  NOT NULL,
  [pos_px_x] real  NOT NULL,
  [pos_px_y] real  NOT NULL,
  [floorplan_masked_area_id] uniqueidentifier  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [device_status] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [status] int  NOT NULL,
  [FloorplanMaskedAreaId1] uniqueidentifier  NULL,
  [MstAccessCctvId] uniqueidentifier  NULL,
  [MstAccessControlId] uniqueidentifier  NULL,
  [MstApplicationId] uniqueidentifier  NULL,
  [MstBleReaderId] uniqueidentifier  NULL,
  [_generate] bigint  IDENTITY(1,1) NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [path] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[floorplan_device] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of floorplan_device
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[floorplan_device] ON
GO

INSERT INTO [dbo].[floorplan_device] ([id], [name], [type], [floorplan_id], [access_cctv_id], [ble_reader_id], [access_control_id], [pos_x], [pos_y], [pos_px_x], [pos_px_y], [floorplan_masked_area_id], [application_id], [device_status], [status], [FloorplanMaskedAreaId1], [MstAccessCctvId], [MstAccessControlId], [MstApplicationId], [MstBleReaderId], [_generate], [created_by], [created_at], [updated_by], [updated_at], [path]) VALUES (N'5346393C-E792-4A86-A004-0A9B10F0D76E', N'Meeting - F130', N'blereader', N'B47CD4BC-FE62-49BC-BFB5-CC6787840A15', NULL, N'AD49DFE5-39FC-42C1-86C0-517CFD312E7A', NULL, N'1122990873953435648', N'167894274510159872', N'708.9999', N'106', N'D37074FA-953C-4F82-8DE5-290B53126FF9', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'active', N'1', NULL, NULL, NULL, NULL, NULL, N'4', N'superadmin', N'2026-02-25 05:53:54.9523399', N'superadmin', N'2026-02-25 05:53:54.9523404', NULL), (N'41DCE813-2F96-44CA-BF78-D5E38008ABBB', N'Meeting - 42A4', N'blereader', N'B47CD4BC-FE62-49BC-BFB5-CC6787840A15', NULL, N'C9B224E1-1430-4C01-8DD0-8F6862A6000B', NULL, N'1824662086282641408', N'152055191036755968', N'1152', N'96', N'D37074FA-953C-4F82-8DE5-290B53126FF9', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'active', N'1', NULL, NULL, NULL, NULL, NULL, N'5', N'superadmin', N'2026-02-25 05:54:14.6495321', N'superadmin', N'2026-02-25 05:54:14.6495326', NULL), (N'CAE8A85B-4BAC-4196-AA01-DB2C1688B85F', N'PANTRY - 66D8', N'blereader', N'B47CD4BC-FE62-49BC-BFB5-CC6787840A15', NULL, N'74C75A9A-B8FB-43DA-BD0D-B3DA863F07CB', NULL, N'560703479367073792', N'388057510738657280', N'354', N'245', N'D37074FA-953C-4F82-8DE5-290B53126FF9', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'active', N'1', NULL, NULL, NULL, NULL, NULL, N'3', N'superadmin', N'2026-02-25 05:53:27.7990214', N'superadmin', N'2026-02-25 05:53:27.7990218', NULL), (N'6F0E8DD2-02AD-446D-BA5C-EB0F77284A19', N'PANTRY - F078', N'blereader', N'B47CD4BC-FE62-49BC-BFB5-CC6787840A15', NULL, N'081D8300-BBCB-4087-8A69-67FB873C1175', NULL, N'31678171241775104', N'33262080018612224', N'20.00001', N'21.00001', N'D37074FA-953C-4F82-8DE5-290B53126FF9', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'active', N'1', NULL, NULL, NULL, NULL, NULL, N'2', N'superadmin', N'2026-02-25 05:53:04.6356740', N'superadmin', N'2026-02-25 05:53:04.6356744', NULL)
GO

SET IDENTITY_INSERT [dbo].[floorplan_device] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for floorplan_masked_area
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[floorplan_masked_area]') AND type IN ('U'))
	DROP TABLE [dbo].[floorplan_masked_area]
GO

CREATE TABLE [dbo].[floorplan_masked_area] (
  [id] uniqueidentifier  NOT NULL,
  [floorplan_id] uniqueidentifier  NOT NULL,
  [floor_id] uniqueidentifier  NOT NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [area_shape] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [color_area] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [restricted_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [status] int DEFAULT 1 NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [MstFloorId] uniqueidentifier  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [allow_floor_change] bit DEFAULT CONVERT([bit],(0)) NOT NULL
)
GO

ALTER TABLE [dbo].[floorplan_masked_area] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of floorplan_masked_area
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[floorplan_masked_area] ([id], [floorplan_id], [floor_id], [name], [area_shape], [color_area], [restricted_status], [status], [application_id], [MstFloorId], [created_by], [created_at], [updated_by], [updated_at], [allow_floor_change]) VALUES (N'D37074FA-953C-4F82-8DE5-290B53126FF9', N'B47CD4BC-FE62-49BC-BFB5-CC6787840A15', N'2E1DA05D-82FC-4FDC-852A-5539F3A00FB1', N'Lobby Area', N'[
  {
    ""id"": ""1"",
    ""x"": 3980614611679466.5,
    ""y"": 4916511301124669,
    ""x_px"": 2.5131599240912235,
    ""y_px"": 3.1040380377629644
  },
  {
    ""id"": ""2"",
    ""x"": 1994454074505690600,
    ""y"": 4394482010497458,
    ""x_px"": 1259.1980232854976,
    ""y_px"": 2.7744549908242524
  },
  {
    ""id"": ""2503"",
    ""x"": 1990949732033738000,
    ""y"": 340420044158405950,
    ""x_px"": 1256.985557643895,
    ""y_px"": 214.9240998679128
  },
  {
    ""id"": ""2602"",
    ""x"": 1825174312549336300,
    ""y"": 341594592490333500,
    ""x_px"": 1152.3232928204654,
    ""y_px"": 215.6656506294579
  },
  {
    ""id"": ""3"",
    ""x"": 1825543412811803100,
    ""y"": 434846303116748900,
    ""x_px"": 1152.5563241681577,
    ""y_px"": 274.5400921068209
  },
  {
    ""id"": ""4"",
    ""x"": 6854574488400113,
    ""y"": 439283483462108700,
    ""x_px"": 4.327633690134386,
    ""y_px"": 277.34150467944335
  }
]', N'#FF0000', N'non-restrict', N'1', N'C926D20B-A746-4492-9924-EB7EEE76305C', NULL, N'System', N'2026-02-25 02:19:33.5160298', NULL, N'0001-01-01 00:00:00.0000000', N'0')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for geofence
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[geofence]') AND type IN ('U'))
	DROP TABLE [dbo].[geofence]
GO

CREATE TABLE [dbo].[geofence] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [area_shape] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [color] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [remarks] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [floorplan_id] uniqueidentifier  NULL,
  [floor_id] uniqueidentifier  NULL,
  [is_active] int  NOT NULL,
  [status] int  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[geofence] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of geofence
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for group_building_access
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[group_building_access]') AND type IN ('U'))
	DROP TABLE [dbo].[group_building_access]
GO

CREATE TABLE [dbo].[group_building_access] (
  [id] uniqueidentifier  NOT NULL,
  [group_id] uniqueidentifier  NOT NULL,
  [building_id] uniqueidentifier  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [status] int  NOT NULL
)
GO

ALTER TABLE [dbo].[group_building_access] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of group_building_access
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for monitoring_config
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[monitoring_config]') AND type IN ('U'))
	DROP TABLE [dbo].[monitoring_config]
GO

CREATE TABLE [dbo].[monitoring_config] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [description] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [config] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [building_id] uniqueidentifier  NULL
)
GO

ALTER TABLE [dbo].[monitoring_config] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of monitoring_config
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for monitoring_config_building_access
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[monitoring_config_building_access]') AND type IN ('U'))
	DROP TABLE [dbo].[monitoring_config_building_access]
GO

CREATE TABLE [dbo].[monitoring_config_building_access] (
  [id] uniqueidentifier  NOT NULL,
  [monitoring_config_id] uniqueidentifier  NOT NULL,
  [building_id] uniqueidentifier  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [status] int DEFAULT 1 NOT NULL
)
GO

ALTER TABLE [dbo].[monitoring_config_building_access] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of monitoring_config_building_access
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_access_cctv
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_access_cctv]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_access_cctv]
GO

CREATE TABLE [dbo].[mst_access_cctv] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [rtsp] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [integration_id] uniqueidentifier  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int DEFAULT 1 NOT NULL,
  [MstApplicationId] uniqueidentifier  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [is_assigned] bit  NULL
)
GO

ALTER TABLE [dbo].[mst_access_cctv] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_access_cctv
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_access_control
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_access_control]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_access_control]
GO

CREATE TABLE [dbo].[mst_access_control] (
  [id] uniqueidentifier  NOT NULL,
  [controller_brand_id] uniqueidentifier  NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [description] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [channel] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [door_id] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [raw] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [integration_id] uniqueidentifier  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int DEFAULT 1 NOT NULL,
  [MstApplicationId] uniqueidentifier  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [is_assigned] bit  NULL
)
GO

ALTER TABLE [dbo].[mst_access_control] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_access_control
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_application
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_application]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_application]
GO

CREATE TABLE [dbo].[mst_application] (
  [id] uniqueidentifier  NOT NULL,
  [application_name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [organization_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS DEFAULT N'Single' NOT NULL,
  [organization_address] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [application_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS DEFAULT N'' NOT NULL,
  [application_registered] datetime2(7)  NOT NULL,
  [application_expired] datetime2(7)  NOT NULL,
  [host_name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [host_phone] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [host_email] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [host_address] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [application_custom_name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [application_custom_domain] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [application_custom_port] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [license_code] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [license_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [application_status] int DEFAULT 1 NOT NULL,
  [_generate] bigint  IDENTITY(1,1) NOT NULL
)
GO

ALTER TABLE [dbo].[mst_application] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_application
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[mst_application] ON
GO

INSERT INTO [dbo].[mst_application] ([id], [application_name], [organization_type], [organization_address], [application_type], [application_registered], [application_expired], [host_name], [host_phone], [host_email], [host_address], [application_custom_name], [application_custom_domain], [application_custom_port], [license_code], [license_type], [application_status], [_generate]) VALUES (N'C926D20B-A746-4492-9924-EB7EEE76305C', N'BIO PEOPLE TRACKING', N'Single', N'Jl. Default No 1', N'tracking', N'2026-02-25 02:19:32.5010278', N'2036-02-25 02:19:32.5010143', N'Admin', N'08123456789', N'admin@example.com', N'Jl. Host No 1', N'BioPeopleTracking', N'localhost', N'8080', N'PERPETUAL-001', N'perpetual', N'1', N'1')
GO

SET IDENTITY_INSERT [dbo].[mst_application] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_ble_reader
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_ble_reader]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_ble_reader]
GO

CREATE TABLE [dbo].[mst_ble_reader] (
  [id] uniqueidentifier  NOT NULL,
  [brand_id] uniqueidentifier  NOT NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [ip] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [gmac] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [status] int DEFAULT 1 NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [is_assigned] bit  NULL,
  [reader_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[mst_ble_reader] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_ble_reader
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[mst_ble_reader] ([id], [brand_id], [name], [ip], [gmac], [status], [application_id], [created_by], [created_at], [updated_by], [updated_at], [is_assigned], [reader_type]) VALUES (N'AD49DFE5-39FC-42C1-86C0-517CFD312E7A', N'824264AF-036F-4AB1-8179-469F1BCC8813', N'Meeting - F130', N'192.168.1.248', N'304A2657F130', N'1', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'superadmin', N'2026-02-25 04:54:32.1200948', N'superadmin', N'2026-02-25 05:53:54.9451031', N'1', N'indoor'), (N'76728A83-A805-4F13-B3D8-53BE45437B22', N'824264AF-036F-4AB1-8179-469F1BCC8813', N'Lobby Reader 1', N'192.168.1.100', N'0123456789ABCDEF', N'0', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2026-02-25 02:19:33.5978419', N'superadmin', N'2026-02-25 04:54:46.7505535', N'0', N'indoor'), (N'081D8300-BBCB-4087-8A69-67FB873C1175', N'824264AF-036F-4AB1-8179-469F1BCC8813', N'PANTRY - F078', N'192.168.1.249', N'304A2657F078', N'1', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'superadmin', N'2026-02-25 04:53:19.6296915', N'superadmin', N'2026-02-25 05:53:04.4175129', N'1', N'indoor'), (N'C9B224E1-1430-4C01-8DD0-8F6862A6000B', N'824264AF-036F-4AB1-8179-469F1BCC8813', N'Meeting - 42A4', N'192.168.1.246', N'304A265742A4', N'1', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'superadmin', N'2026-02-25 04:54:10.4993901', N'superadmin', N'2026-02-25 05:54:14.6423618', N'1', N'indoor'), (N'74C75A9A-B8FB-43DA-BD0D-B3DA863F07CB', N'824264AF-036F-4AB1-8179-469F1BCC8813', N'PANTRY - 66D8', N'192.168.1.246', N'304A265766D8', N'1', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'superadmin', N'2026-02-25 04:53:46.1604477', N'superadmin', N'2026-02-25 05:53:27.7919859', N'1', N'indoor')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_brand
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_brand]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_brand]
GO

CREATE TABLE [dbo].[mst_brand] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [tag] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NULL,
  [_generate] int  IDENTITY(1,1) NOT NULL
)
GO

ALTER TABLE [dbo].[mst_brand] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_brand
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[mst_brand] ON
GO

INSERT INTO [dbo].[mst_brand] ([id], [name], [tag], [application_id], [status], [_generate]) VALUES (N'824264AF-036F-4AB1-8179-469F1BCC8813', N'Bio - Initial', N'People Tracking Tag', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'1', N'1')
GO

SET IDENTITY_INSERT [dbo].[mst_brand] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_building
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_building]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_building]
GO

CREATE TABLE [dbo].[mst_building] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [image] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [ApplicationId] uniqueidentifier  NOT NULL,
  [status] int DEFAULT 1 NOT NULL,
  [MstApplicationId] uniqueidentifier  NULL,
  [_generate] bigint  IDENTITY(1,1) NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[mst_building] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_building
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[mst_building] ON
GO

INSERT INTO [dbo].[mst_building] ([id], [name], [image], [ApplicationId], [status], [MstApplicationId], [_generate], [created_by], [created_at], [updated_by], [updated_at]) VALUES (N'4103979C-F5B5-41BF-9ED1-AA17A9B04E66', N'Main Building', N'/Uploads/BuildingImages/8e9f3fc7-e428-45ea-9bbf-f6e4f6e39527.jpg', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'1', NULL, N'1', NULL, N'0001-01-01 00:00:00.0000000', N'superadmin', N'2026-02-25 04:13:21.1201695')
GO

SET IDENTITY_INSERT [dbo].[mst_building] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_department
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_department]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_department]
GO

CREATE TABLE [dbo].[mst_department] (
  [id] uniqueidentifier  NOT NULL,
  [code] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [department_host] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int DEFAULT 1 NOT NULL,
  [MstApplicationId] uniqueidentifier  NULL,
  [_generate] int  IDENTITY(1,1) NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[mst_department] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_department
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[mst_department] ON
GO

INSERT INTO [dbo].[mst_department] ([id], [code], [name], [department_host], [application_id], [status], [MstApplicationId], [_generate], [created_by], [created_at], [updated_by], [updated_at]) VALUES (N'F99CF1F7-789E-4C75-A044-BDB10C773881', N'1', N'BIO - Department', N'BIO - Host', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'1', NULL, N'1', N'superadmin', N'2026-02-25 06:20:47.5386797', N'superadmin', N'2026-02-25 06:20:47.5387008')
GO

SET IDENTITY_INSERT [dbo].[mst_department] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_district
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_district]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_district]
GO

CREATE TABLE [dbo].[mst_district] (
  [id] uniqueidentifier  NOT NULL,
  [code] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [district_host] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int DEFAULT 1 NOT NULL,
  [MstApplicationId] uniqueidentifier  NULL,
  [_generate] int  IDENTITY(1,1) NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[mst_district] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_district
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[mst_district] ON
GO

INSERT INTO [dbo].[mst_district] ([id], [code], [name], [district_host], [application_id], [status], [MstApplicationId], [_generate], [created_by], [created_at], [updated_by], [updated_at]) VALUES (N'0CF11396-56F5-4946-9DD8-F02B0EF6F1AD', N'1', N'BIO - District', N'BIO - Host', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'1', NULL, N'1', N'superadmin', N'2026-02-25 06:20:59.0543407', N'superadmin', N'2026-02-25 06:20:59.0543644')
GO

SET IDENTITY_INSERT [dbo].[mst_district] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_engine
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_engine]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_engine]
GO

CREATE TABLE [dbo].[mst_engine] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [engine_tracking_id] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [port] int  NULL,
  [status] int  NULL,
  [is_live] int  NULL,
  [last_live] datetime2(7)  NULL,
  [service_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_at] datetime2(7) DEFAULT '0001-01-01T00:00:00.0000000' NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7) DEFAULT '0001-01-01T00:00:00.0000000' NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[mst_engine] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_engine
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[mst_engine] ([id], [name], [engine_tracking_id], [port], [status], [is_live], [last_live], [service_status], [application_id], [created_at], [created_by], [updated_at], [updated_by]) VALUES (N'9D2F5FC9-3E73-9C95-D0BD-8FD0FC715B0D', N'people_tracking_enigne', N'Tracking-People-Engine-0025926f-a33e-4b70-9d2b-cb88ddc6ba93', N'0', N'1', N'1', N'2008-12-12 22:26:28.0000000', N'online', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'2023-12-06 23:47:54.0000000', N'system', N'2009-02-23 14:23:32.0000000', N'system')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_floor
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_floor]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_floor]
GO

CREATE TABLE [dbo].[mst_floor] (
  [id] uniqueidentifier  NOT NULL,
  [building_id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [status] int DEFAULT 1 NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [_generate] int  IDENTITY(1,1) NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[mst_floor] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_floor
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[mst_floor] ON
GO

INSERT INTO [dbo].[mst_floor] ([id], [building_id], [name], [status], [application_id], [_generate], [created_by], [created_at], [updated_by], [updated_at]) VALUES (N'2E1DA05D-82FC-4FDC-852A-5539F3A00FB1', N'4103979C-F5B5-41BF-9ED1-AA17A9B04E66', N'1st Floor', N'1', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'1', NULL, N'0001-01-01 00:00:00.0000000', NULL, N'0001-01-01 00:00:00.0000000')
GO

SET IDENTITY_INSERT [dbo].[mst_floor] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_floorplan
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_floorplan]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_floorplan]
GO

CREATE TABLE [dbo].[mst_floorplan] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [floorplan_image] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [pixel_x] real  NOT NULL,
  [pixel_y] real  NOT NULL,
  [floor_x] real  NOT NULL,
  [floor_y] real  NOT NULL,
  [meter_per_px] real  NOT NULL,
  [floor_id] uniqueidentifier  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int DEFAULT 1 NOT NULL,
  [MstApplicationId] uniqueidentifier  NULL,
  [MstFloorId] uniqueidentifier  NULL,
  [_generate] bigint  IDENTITY(1,1) NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [engine_id] uniqueidentifier  NULL
)
GO

ALTER TABLE [dbo].[mst_floorplan] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_floorplan
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[mst_floorplan] ON
GO

INSERT INTO [dbo].[mst_floorplan] ([id], [name], [floorplan_image], [pixel_x], [pixel_y], [floor_x], [floor_y], [meter_per_px], [floor_id], [application_id], [status], [MstApplicationId], [MstFloorId], [_generate], [created_by], [created_at], [updated_by], [updated_at], [engine_id]) VALUES (N'B47CD4BC-FE62-49BC-BFB5-CC6787840A15', N'1st Floorplan', N'/Uploads/FloorplanImages/a75de0db-9afb-4e42-864b-62c683fce6d8.png', N'1401', N'307', N'17', N'6', N'1583908239966208', N'2E1DA05D-82FC-4FDC-852A-5539F3A00FB1', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'1', NULL, NULL, N'1', N'System', N'2026-02-25 02:19:33.4563801', N'superadmin', N'2026-02-25 04:12:26.5113211', N'9D2F5FC9-3E73-9C95-D0BD-8FD0FC715B0D')
GO

SET IDENTITY_INSERT [dbo].[mst_floorplan] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_integration
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_integration]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_integration]
GO

CREATE TABLE [dbo].[mst_integration] (
  [id] uniqueidentifier  NOT NULL,
  [brand_id] uniqueidentifier  NOT NULL,
  [integration_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [api_type_auth] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [api_url] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [api_auth_username] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [api_auth_passwd] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [api_key_field] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [api_key_value] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int DEFAULT 1 NOT NULL,
  [_generate] int  IDENTITY(1,1) NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[mst_integration] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_integration
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[mst_integration] ON
GO

INSERT INTO [dbo].[mst_integration] ([id], [brand_id], [integration_type], [api_type_auth], [api_url], [api_auth_username], [api_auth_passwd], [api_key_field], [api_key_value], [application_id], [status], [_generate], [created_by], [created_at], [updated_by], [updated_at]) VALUES (N'217C6483-B609-4237-9A74-D2F85052473A', N'824264AF-036F-4AB1-8179-469F1BCC8813', N'api', N'apikey', NULL, NULL, NULL, N'X-BIOPEOPLETRACKING-API-KEY', N'FujDuGTsyEXVwkKrtRgn52APwAVRGmPOiIRX8cffynDvIW35bJaGeH3NcH6HcSeK', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'1', N'1', N'System', N'2026-02-25 03:00:38.5346472', N'System', N'2026-02-25 03:00:38.5346686')
GO

SET IDENTITY_INSERT [dbo].[mst_integration] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_member
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_member]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_member]
GO

CREATE TABLE [dbo].[mst_member] (
  [id] uniqueidentifier  NOT NULL,
  [person_id] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [organization_id] uniqueidentifier  NULL,
  [department_id] uniqueidentifier  NULL,
  [district_id] uniqueidentifier  NULL,
  [identity_id] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [card_number] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [ble_card_number] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [phone] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [email] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [gender] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [address] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [face_image] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [upload_fr] int  NOT NULL,
  [upload_fr_error] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [birth_date] date  NULL,
  [join_date] date  NULL,
  [exit_date] date  NULL,
  [head_member1] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [head_member2] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status_employee] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [status] int DEFAULT 1 NOT NULL,
  [MstApplicationId] uniqueidentifier  NULL,
  [MstDepartmentId] uniqueidentifier  NULL,
  [MstDistrictId] uniqueidentifier  NULL,
  [MstOrganizationId] uniqueidentifier  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [blacklist_at] datetime2(7)  NULL,
  [is_blacklist] bit  NULL,
  [blacklist_reason] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[mst_member] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_member
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[mst_member] ([id], [person_id], [organization_id], [department_id], [district_id], [identity_id], [card_number], [ble_card_number], [name], [phone], [email], [gender], [address], [face_image], [upload_fr], [upload_fr_error], [birth_date], [join_date], [exit_date], [head_member1], [head_member2], [application_id], [status_employee], [status], [MstApplicationId], [MstDepartmentId], [MstDistrictId], [MstOrganizationId], [created_by], [created_at], [updated_by], [updated_at], [blacklist_at], [is_blacklist], [blacklist_reason]) VALUES (N'AB4B8247-0DA2-43B2-8CD0-35E15D83F76E', N'EMP001', N'9AD17645-8D52-414B-A770-82C8FC1E187E', N'F99CF1F7-789E-4C75-A044-BDB10C773881', N'0CF11396-56F5-4946-9DD8-F02B0EF6F1AD', N'1', N'465757', N'BC572905D5B9', N'Alice Employee', N'1', N'alice@email.com', N'female', N'alice street', NULL, N'0', NULL, N'2002-02-14', N'2009-02-14', NULL, NULL, NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', NULL, N'1', NULL, NULL, NULL, NULL, NULL, N'2009-02-14 13:52:55.0000000', NULL, N'2009-02-14 13:52:55.0000000', NULL, NULL, NULL)
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_organization
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_organization]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_organization]
GO

CREATE TABLE [dbo].[mst_organization] (
  [id] uniqueidentifier  NOT NULL,
  [code] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [organization_host] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int DEFAULT 1 NOT NULL,
  [MstApplicationId] uniqueidentifier  NULL,
  [_generate] int  IDENTITY(1,1) NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[mst_organization] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_organization
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[mst_organization] ON
GO

INSERT INTO [dbo].[mst_organization] ([id], [code], [name], [organization_host], [application_id], [status], [MstApplicationId], [_generate], [created_by], [created_at], [updated_by], [updated_at]) VALUES (N'9AD17645-8D52-414B-A770-82C8FC1E187E', N'1', N'BIO - Org', N'BIO - Host', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'1', NULL, N'3', N'superadmin', N'2026-02-25 06:20:32.3877279', N'superadmin', N'2026-02-25 06:20:32.3878359')
GO

SET IDENTITY_INSERT [dbo].[mst_organization] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for mst_security
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[mst_security]') AND type IN ('U'))
	DROP TABLE [dbo].[mst_security]
GO

CREATE TABLE [dbo].[mst_security] (
  [id] uniqueidentifier  NOT NULL,
  [person_id] nvarchar(450) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [organization_id] uniqueidentifier  NULL,
  [department_id] uniqueidentifier  NULL,
  [district_id] uniqueidentifier  NULL,
  [identity_id] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [card_number] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [ble_card_number] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [phone] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [email] nvarchar(450) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [gender] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [address] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [face_image] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [upload_fr] int  NOT NULL,
  [upload_fr_error] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [birth_date] date  NULL,
  [join_date] date  NULL,
  [exit_date] date  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status_employee] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [status] int DEFAULT 1 NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [security_head_1] uniqueidentifier  NULL,
  [security_head_2] uniqueidentifier  NULL
)
GO

ALTER TABLE [dbo].[mst_security] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of mst_security
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[mst_security] ([id], [person_id], [organization_id], [department_id], [district_id], [identity_id], [card_number], [ble_card_number], [name], [phone], [email], [gender], [address], [face_image], [upload_fr], [upload_fr_error], [birth_date], [join_date], [exit_date], [application_id], [status_employee], [status], [created_by], [created_at], [updated_by], [updated_at], [security_head_1], [security_head_2]) VALUES (N'E3D48AA6-35F8-4659-A6C3-2E37663EEB45', N'SEC001', N'9AD17645-8D52-414B-A770-82C8FC1E187E', N'F99CF1F7-789E-4C75-A044-BDB10C773881', N'0CF11396-56F5-4946-9DD8-F02B0EF6F1AD', N'2', N'677013', N'BC57291F5FC1', N'Bob Security', NULL, NULL, N'male', NULL, NULL, N'0', NULL, N'2002-02-25', N'2026-02-25', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'active', N'1', N'system', N'2026-02-25 00:00:00.0000000', N'system', N'2026-02-25 00:00:00.0000000', NULL, NULL)
GO

COMMIT
GO


-- ----------------------------
-- Table structure for overpopulating
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[overpopulating]') AND type IN ('U'))
	DROP TABLE [dbo].[overpopulating]
GO

CREATE TABLE [dbo].[overpopulating] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [area_shape] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [color] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [remarks] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [floorplan_id] uniqueidentifier  NULL,
  [floor_id] uniqueidentifier  NULL,
  [is_active] int  NOT NULL,
  [status] int  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [max_capacity] int  NULL
)
GO

ALTER TABLE [dbo].[overpopulating] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of overpopulating
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for patrol_area
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[patrol_area]') AND type IN ('U'))
	DROP TABLE [dbo].[patrol_area]
GO

CREATE TABLE [dbo].[patrol_area] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [area_shape] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [color] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [remarks] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [floorplan_id] uniqueidentifier  NULL,
  [floor_id] uniqueidentifier  NULL,
  [is_active] int  NOT NULL,
  [status] int  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[patrol_area] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of patrol_area
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for patrol_assignment
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[patrol_assignment]') AND type IN ('U'))
	DROP TABLE [dbo].[patrol_assignment]
GO

CREATE TABLE [dbo].[patrol_assignment] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [description] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [patrol_route_id] uniqueidentifier  NULL,
  [start_date] datetime2(7)  NULL,
  [end_date] datetime2(7)  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL,
  [time_group_id] uniqueidentifier  NULL,
  [approval_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS DEFAULT N'withoutapproval' NOT NULL
)
GO

ALTER TABLE [dbo].[patrol_assignment] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of patrol_assignment
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for patrol_assignment_security
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[patrol_assignment_security]') AND type IN ('U'))
	DROP TABLE [dbo].[patrol_assignment_security]
GO

CREATE TABLE [dbo].[patrol_assignment_security] (
  [id] uniqueidentifier  NOT NULL,
  [patrol_assignment_id] uniqueidentifier  NOT NULL,
  [security_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL
)
GO

ALTER TABLE [dbo].[patrol_assignment_security] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of patrol_assignment_security
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for patrol_case
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[patrol_case]') AND type IN ('U'))
	DROP TABLE [dbo].[patrol_case]
GO

CREATE TABLE [dbo].[patrol_case] (
  [id] uniqueidentifier  NOT NULL,
  [title] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [description] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [case_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [case_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [patrol_session_id] uniqueidentifier  NULL,
  [security_id] uniqueidentifier  NULL,
  [patrol_assignment_id] uniqueidentifier  NULL,
  [patrol_route_id] uniqueidentifier  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL,
  [approved_by_head_1_at] datetime2(7)  NULL,
  [approved_by_head_2_at] datetime2(7)  NULL,
  [threat_level] int  NULL,
  [security_head_1] uniqueidentifier  NULL,
  [security_head_2] uniqueidentifier  NULL,
  [approval_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS DEFAULT N'withoutapproval' NOT NULL,
  [approved_by_head_1_id] uniqueidentifier  NULL,
  [approved_by_head_2_id] uniqueidentifier  NULL
)
GO

ALTER TABLE [dbo].[patrol_case] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of patrol_case
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for patrol_case_attachment
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[patrol_case_attachment]') AND type IN ('U'))
	DROP TABLE [dbo].[patrol_case_attachment]
GO

CREATE TABLE [dbo].[patrol_case_attachment] (
  [id] uniqueidentifier  NOT NULL,
  [patrol_case_id] uniqueidentifier  NULL,
  [file_url] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [file_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [mime_type] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [uploaded_at] datetime2(7)  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL
)
GO

ALTER TABLE [dbo].[patrol_case_attachment] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of patrol_case_attachment
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for patrol_checkpoint_log
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[patrol_checkpoint_log]') AND type IN ('U'))
	DROP TABLE [dbo].[patrol_checkpoint_log]
GO

CREATE TABLE [dbo].[patrol_checkpoint_log] (
  [id] uniqueidentifier  NOT NULL,
  [patrol_session_id] uniqueidentifier  NULL,
  [patrol_area_id] uniqueidentifier  NULL,
  [order_index] int  NULL,
  [arrived_at] datetime2(7)  NULL,
  [left_at] datetime2(7)  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL,
  [area_name_snapshot] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [distance_from_prev_meters] float(53)  NULL
)
GO

ALTER TABLE [dbo].[patrol_checkpoint_log] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of patrol_checkpoint_log
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for patrol_route
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[patrol_route]') AND type IN ('U'))
	DROP TABLE [dbo].[patrol_route]
GO

CREATE TABLE [dbo].[patrol_route] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [description] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [status] int  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL
)
GO

ALTER TABLE [dbo].[patrol_route] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of patrol_route
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for patrol_route_areas
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[patrol_route_areas]') AND type IN ('U'))
	DROP TABLE [dbo].[patrol_route_areas]
GO

CREATE TABLE [dbo].[patrol_route_areas] (
  [patrol_route_id] uniqueidentifier  NOT NULL,
  [patrol_area_id] uniqueidentifier  NOT NULL,
  [order_index] int  NOT NULL,
  [estimated_distance] real  NOT NULL,
  [estimated_time] int  NOT NULL,
  [start_area_id] uniqueidentifier  NULL,
  [end_area_id] uniqueidentifier  NULL,
  [status] int  NOT NULL,
  [id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL
)
GO

ALTER TABLE [dbo].[patrol_route_areas] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of patrol_route_areas
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for patrol_session
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[patrol_session]') AND type IN ('U'))
	DROP TABLE [dbo].[patrol_session]
GO

CREATE TABLE [dbo].[patrol_session] (
  [id] uniqueidentifier  NOT NULL,
  [patrol_route_id] uniqueidentifier  NOT NULL,
  [patrol_assignment_id] uniqueidentifier  NOT NULL,
  [security_id] uniqueidentifier  NOT NULL,
  [started_at] datetime2(7)  NOT NULL,
  [ended_at] datetime2(7)  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL,
  [patrol_assignment_name_snap] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [patrol_route_name_snap] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [security_card_number_snap] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [security_identity_id_snap] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [security_name_snap] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [time_group_id] uniqueidentifier  NULL,
  [time_group_name_snap] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[patrol_session] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of patrol_session
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for refresh_token
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[refresh_token]') AND type IN ('U'))
	DROP TABLE [dbo].[refresh_token]
GO

CREATE TABLE [dbo].[refresh_token] (
  [id] uniqueidentifier  NOT NULL,
  [user_id] uniqueidentifier  NOT NULL,
  [token] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [expiry_date] datetime2(7)  NOT NULL,
  [created_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[refresh_token] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of refresh_token
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[refresh_token] ([id], [user_id], [token], [expiry_date], [created_at]) VALUES (N'67D5116A-9275-41C7-99F0-060BF8BB3FFB', N'B53F464B-68B4-4831-AB9A-8F3E56D9FA33', N'YJK4l8itqYQhZA7aC1zaN3zKhzlPcnHteUGOSEuoHNY=', N'2026-03-04 05:49:14.4605525', N'2026-02-25 05:49:14.4606038'), (N'C767E32B-02C5-4087-B7F0-0DFBA0E6A886', N'B53F464B-68B4-4831-AB9A-8F3E56D9FA33', N'l1fls2KKWlZbPtO2Wk+m5vMFv+Fzh2ErttiGxsnQNlg=', N'2026-03-04 04:50:07.8090193', N'2026-02-25 04:50:07.8090521'), (N'A7F14434-6184-43C2-8641-45B1B3D74CF4', N'B53F464B-68B4-4831-AB9A-8F3E56D9FA33', N'uGUWGCZrXzpmaih9H8dSKE7A9RD5bd4N39EZAyxjYeY=', N'2026-03-04 04:26:58.4879287', N'2026-02-25 04:26:58.4896313'), (N'079E8F9D-9F17-4DC8-A0B0-4993A4EB5F4D', N'B53F464B-68B4-4831-AB9A-8F3E56D9FA33', N'fmST6IFa8bfwlYkh/CQ0r+Db3jyn3TsoIHSwWuOyRbs=', N'2026-03-04 04:05:58.7117914', N'2026-02-25 04:05:58.7118323'), (N'7D5D512D-52D2-47D4-A39F-508ED4F0267D', N'B53F464B-68B4-4831-AB9A-8F3E56D9FA33', N'YkabTi2RUK4x0srwvhEgCxC+/bJPSanYS/MEWiom3UU=', N'2026-03-04 05:49:10.6061841', N'2026-02-25 05:49:10.6065637'), (N'A6E23C88-7036-4F81-B06E-64DFE5036274', N'B53F464B-68B4-4831-AB9A-8F3E56D9FA33', N'keiVeUcG/UKiVLfhykWtXTtnhcZZ1NgMukq7XA9pbik=', N'2026-03-04 04:29:00.6389667', N'2026-02-25 04:29:00.6390724'), (N'0EDD0D65-90EC-401A-8EAA-6C868346780C', N'B53F464B-68B4-4831-AB9A-8F3E56D9FA33', N'fGQVvNxVDt0m2cr8shgSPhwKAc+qwCPfI56j3TeeMA0=', N'2026-03-04 06:17:27.7504375', N'2026-02-25 06:17:27.7508126'), (N'A23B91BF-EAA3-4F3E-97BE-8B4DD38F710E', N'B53F464B-68B4-4831-AB9A-8F3E56D9FA33', N'UwBExM/um5aSKjerv8eg+qQE0G7J6OVBK492BoIUr+M=', N'2026-03-04 04:50:07.7834321', N'2026-02-25 04:50:07.7873794'), (N'08D99A69-4F0E-46AC-8E67-9860AA5ABEEA', N'B53F464B-68B4-4831-AB9A-8F3E56D9FA33', N'PE+bR2FK5zqa9l3QP2c2eC4zlNxF1+QYCPReC99GdzM=', N'2026-03-04 05:49:14.6572813', N'2026-02-25 05:49:14.6573101'), (N'DEE49D46-797D-4950-92A1-BAAF35794BB8', N'B53F464B-68B4-4831-AB9A-8F3E56D9FA33', N'ja+Ov4Xw88haHWlQCsytBoeH6tOVNgQuUWhlDGxXX5E=', N'2026-03-04 04:05:58.7107390', N'2026-02-25 04:05:58.7111330')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for security_group
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[security_group]') AND type IN ('U'))
	DROP TABLE [dbo].[security_group]
GO

CREATE TABLE [dbo].[security_group] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [description] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL
)
GO

ALTER TABLE [dbo].[security_group] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of security_group
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for security_group_member
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[security_group_member]') AND type IN ('U'))
	DROP TABLE [dbo].[security_group_member]
GO

CREATE TABLE [dbo].[security_group_member] (
  [id] uniqueidentifier  NOT NULL,
  [security_group_id] uniqueidentifier  NULL,
  [security_id] uniqueidentifier  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL
)
GO

ALTER TABLE [dbo].[security_group_member] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of security_group_member
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for stay_on_area
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[stay_on_area]') AND type IN ('U'))
	DROP TABLE [dbo].[stay_on_area]
GO

CREATE TABLE [dbo].[stay_on_area] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [area_shape] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [color] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [remarks] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [floorplan_id] uniqueidentifier  NULL,
  [floor_id] uniqueidentifier  NULL,
  [is_active] int  NOT NULL,
  [status] int  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [max_duration] int  NULL
)
GO

ALTER TABLE [dbo].[stay_on_area] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of stay_on_area
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for time_block
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[time_block]') AND type IN ('U'))
	DROP TABLE [dbo].[time_block]
GO

CREATE TABLE [dbo].[time_block] (
  [id] uniqueidentifier  NOT NULL,
  [day_of_week] int  NULL,
  [start_time] time(7)  NULL,
  [end_time] time(7)  NULL,
  [time_group_id] uniqueidentifier  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL
)
GO

ALTER TABLE [dbo].[time_block] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of time_block
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for time_group
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[time_group]') AND type IN ('U'))
	DROP TABLE [dbo].[time_group]
GO

CREATE TABLE [dbo].[time_group] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [description] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [status] int  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [schedule_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL
)
GO

ALTER TABLE [dbo].[time_group] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of time_group
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for tracking_report_presets
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[tracking_report_presets]') AND type IN ('U'))
	DROP TABLE [dbo].[tracking_report_presets]
GO

CREATE TABLE [dbo].[tracking_report_presets] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [time_range] nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [custom_from_date] datetime2(7)  NULL,
  [custom_to_date] datetime2(7)  NULL,
  [status] int  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [area_id] uniqueidentifier  NULL,
  [building_id] uniqueidentifier  NULL,
  [floor_id] uniqueidentifier  NULL,
  [floorplan_id] uniqueidentifier  NULL,
  [member_id] uniqueidentifier  NULL,
  [visitor_id] uniqueidentifier  NULL
)
GO

ALTER TABLE [dbo].[tracking_report_presets] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of tracking_report_presets
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for tracking_transaction
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[tracking_transaction]') AND type IN ('U'))
	DROP TABLE [dbo].[tracking_transaction]
GO

CREATE TABLE [dbo].[tracking_transaction] (
  [id] uniqueidentifier  NOT NULL,
  [trans_time] datetime2(7)  NULL,
  [reader_id] uniqueidentifier  NULL,
  [card_id] uniqueidentifier  NULL,
  [floorplan_masked_area_id] uniqueidentifier  NULL,
  [coordinate_x] real  NULL,
  [coordinate_y] real  NULL,
  [coordinate_px_x] real  NULL,
  [coordinate_px_y] real  NULL,
  [alarm_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [battery] bigint  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [FloorplanMaskedAreaId1] uniqueidentifier  NULL,
  [MstBleReaderId] uniqueidentifier  NULL,
  [member_id] uniqueidentifier  NULL,
  [visitor_id] uniqueidentifier  NULL
)
GO

ALTER TABLE [dbo].[tracking_transaction] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of tracking_transaction
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for trx_visitor
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[trx_visitor]') AND type IN ('U'))
	DROP TABLE [dbo].[trx_visitor]
GO

CREATE TABLE [dbo].[trx_visitor] (
  [id] uniqueidentifier  NOT NULL,
  [checked_in_at] datetime2(7)  NULL,
  [checked_out_at] datetime2(7)  NULL,
  [deny_at] datetime2(7)  NULL,
  [block_at] datetime2(7)  NULL,
  [unblock_at] datetime2(7)  NULL,
  [checkin_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [checkout_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [deny_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [deny_reason] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [block_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [block_reason] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [visitor_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [visitor_active_status] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [invitation_created_at] datetime2(7)  NOT NULL,
  [visitor_group_code] bigint  NULL,
  [visitor_number] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [visitor_code] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [vehicle_plate_number] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [remarks] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [visitor_period_start] datetime2(7)  NULL,
  [visitor_period_end] datetime2(7)  NULL,
  [is_invitation_accepted] bit  NULL,
  [invitation_code] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [invitation_token_expired_at] datetime2(7)  NULL,
  [masked_area_id] uniqueidentifier  NULL,
  [parking_id] uniqueidentifier  NULL,
  [visitor_id] uniqueidentifier  NULL,
  [purpose_person_id] uniqueidentifier  NULL,
  [member_identity] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [TrxStatus] int  NOT NULL,
  [is_member] int  NULL,
  [agenda] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [VisitorId1] uniqueidentifier  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [person_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [card_number] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [extended_visitor_time] int  NULL
)
GO

ALTER TABLE [dbo].[trx_visitor] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of trx_visitor
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[trx_visitor] ([id], [checked_in_at], [checked_out_at], [deny_at], [block_at], [unblock_at], [checkin_by], [checkout_by], [deny_by], [deny_reason], [block_by], [block_reason], [visitor_status], [visitor_active_status], [invitation_created_at], [visitor_group_code], [visitor_number], [visitor_code], [vehicle_plate_number], [remarks], [visitor_period_start], [visitor_period_end], [is_invitation_accepted], [invitation_code], [invitation_token_expired_at], [masked_area_id], [parking_id], [visitor_id], [purpose_person_id], [member_identity], [application_id], [TrxStatus], [is_member], [agenda], [VisitorId1], [created_by], [created_at], [updated_by], [updated_at], [person_type], [card_number], [extended_visitor_time]) VALUES (N'A0A13591-38DA-7F69-729B-6A2DD990EB9E', N'2025-02-25 03:02:56.0000000', NULL, NULL, NULL, NULL, N'system', NULL, NULL, NULL, NULL, NULL, N'checkin', N'active', N'2009-02-14 13:52:55.0000000', N'877', N'SSH serves to prevent such vulnerabilities and allows you to access a remote server''s shell without compromising security. Navicat Data Modeler enables you to build high-quality conceptual, logical and physical data models for a wide variety of audiences. A query is used to extract data from the database in a readable format according to the user''s request. You cannot save people, you can just love them. Difficult circumstances serve as a textbook of life for people. In the Objects tab, you can use the List List, Detail Detail and ER Diagram ER Diagram buttons to change the object view. There is no way to happiness. Happiness is the way. How we spend our days is, of course, how we spend our lives. Export Wizard allows you to export data from tables, collections, views, or query results to any available formats. I destroy my enemies when I make them my friends. Always keep your eyes open. Keep watching. Because whatever you see can inspire you. Always keep your eyes open. Keep watching. Because whatever you see can inspire you. Typically, it is employed as an encrypted version of Telnet. In the Objects tab, you can use the List List, Detail Detail and ER Diagram ER Diagram buttons to change the object view. It is used while your ISPs do not allow direct connections, but allows establishing HTTP connections. Typically, it is employed as an encrypted version of Telnet. The first step is as good as half over. Export Wizard allows you to export data from tables, collections, views, or query results to any available formats. It provides strong authentication and secure encrypted communications between two hosts, known as SSH Port Forwarding (Tunneling), over an insecure network. Monitored servers include MySQL, MariaDB and SQL Server, and compatible with cloud databases like Amazon RDS, Amazon Aurora, Oracle Cloud, Google Cloud and Microsoft Azure. The Navigation pane employs tree structure which allows you to take action upon the database and their objects through their pop-up menus quickly and easily. Success consists of going from failure to failure without loss of enthusiasm. SQL Editor allows you to create and edit SQL text, prepare and execute selected queries. In the middle of winter I at last discovered that there was in me an invincible summer. Champions keep playing until they get it right. Instead of wondering when your next vacation is, maybe you should set up a life you don’t need to escape from. To connect to a database or schema, simply double-click it in the pane. All journeys have secret destinations of which the traveler is unaware. Sometimes you win, sometimes you learn. The Navigation pane employs tree structure which allows you to take action upon the database and their objects through their pop-up menus quickly and easily. Navicat Monitor is a safe, simple and agentless remote server monitoring tool that is packed with powerful features to make your monitoring effective as possible. To connect to a database or schema, simply double-click it in the pane. If the Show objects under schema in navigation pane option is checked at the Preferences window, all database objects are also displayed in the pane. To successfully establish a new connection to local/remote server - no matter via SSL, SSH or HTTP, set the database login information in the General tab. Flexible settings enable you to set up a custom key for comparison and synchronization. If you wait, all that happens is you get older. It is used while your ISPs do not allow direct connections, but allows establishing HTTP connections. Instead of wondering when your next vacation is, maybe you should set up a life you don’t need to escape from. It is used while your ISPs do not allow direct connections, but allows establishing HTTP connections. It is used while your ISPs do not allow direct connections, but allows establishing HTTP connections. After logged in the Navicat Cloud feature, the Navigation pane will be divided into Navicat Cloud and My Connections sections. Navicat authorizes you to make connection to remote servers running on different platforms (i.e. Windows, macOS, Linux and UNIX), and supports PAM and GSSAPI authentication. You will succeed because most people are lazy. Typically, it is employed as an encrypted version of Telnet. Instead of wondering when your next vacation is, maybe you should set up a life you don’t need to escape from. Such sessions are also susceptible to session hijacking, where a malicious user takes over your session once you have authenticated. A comfort zone is a beautiful place, but nothing ever grows there. All journeys have secret destinations of which the traveler is unaware. If it scares you, it might be a good thing to try. Navicat Data Modeler enables you to build high-quality conceptual, logical and physical data models for a wide variety of audiences. Export Wizard allows you to export data from tables, collections, views, or query results to any available formats. It collects process metrics such as CPU load, RAM usage, and a variety of other resources over SSH/SNMP. The repository database can be an existing MySQL, MariaDB, PostgreSQL, SQL Server, or Amazon RDS instance. The Information Pane shows the detailed object information, project activities, the DDL of database objects, object dependencies, membership of users/roles and preview. If you wait, all that happens is you get older. The Navigation pane employs tree structure which allows you to take action upon the database and their objects through their pop-up menus quickly and easily. Navicat authorizes you to make connection to remote servers running on different platforms (i.e. Windows, macOS, Linux and UNIX), and supports PAM and GSSAPI authentication. Navicat authorizes you to make connection to remote servers running on different platforms (i.e. Windows, macOS, Linux and UNIX), and supports PAM and GSSAPI authentication. All the Navicat Cloud objects are located under different projects. You can share the project to other Navicat Cloud accounts for collaboration. In the middle of winter I at last discovered that there was in me an invincible summer. I will greet this day with love in my heart. You cannot save people, you can just love them. I will greet this day with love in my heart. You cannot save people, you can just love them. Actually it is just in an idea when feel oneself can achieve and cannot achieve. In the Objects tab, you can use the List List, Detail Detail and ER Diagram ER Diagram buttons to change the object view. The Navigation pane employs tree structure which allows you to take action upon the database and their objects through their pop-up menus quickly and easily.', N'The past has no power over the present moment. The Main Window consists of several toolbars and panes for you to work on connections, database objects and advanced tools. Anyone who has ever made anything of importance was disciplined. To clear or reload various internal caches, flush tables, or acquire locks, control-click your connection in the Navigation pane and select Flush and choose the flush option. You must have the reload privilege to use this feature. All journeys have secret destinations of which the traveler is unaware. Anyone who has ever made anything of importance was disciplined. If the plan doesn’t work, change the plan, but never the goal. Anyone who has ever made anything of importance was disciplined. Navicat Monitor is a safe, simple and agentless remote server monitoring tool that is packed with powerful features to make your monitoring effective as possible. You cannot save people, you can just love them. Monitored servers include MySQL, MariaDB and SQL Server, and compatible with cloud databases like Amazon RDS, Amazon Aurora, Oracle Cloud, Google Cloud and Microsoft Azure. The reason why a great man is great is that he resolves to be a great man. If your Internet Service Provider (ISP) does not provide direct access to its server, Secure Tunneling Protocol (SSH) / HTTP is another solution. The past has no power over the present moment. SSH serves to prevent such vulnerabilities and allows you to access a remote server''s shell without compromising security. Navicat 15 has added support for the system-wide dark mode. The repository database can be an existing MySQL, MariaDB, PostgreSQL, SQL Server, or Amazon RDS instance. Import Wizard allows you to import data to tables/collections from CSV, TXT, XML, DBF and more. Flexible settings enable you to set up a custom key for comparison and synchronization. A man is not old until regrets take the place of dreams. What you get by achieving your goals is not as important as what you become by achieving your goals. If opportunity doesn’t knock, build a door. Navicat Monitor requires a repository to store alerts and metrics for historical analysis. All journeys have secret destinations of which the traveler is unaware. In other words, Navicat provides the ability for data in different databases and/or schemas to be kept up-to-date so that each repository contains the same information. Import Wizard allows you to import data to tables/collections from CSV, TXT, XML, DBF and more. All journeys have secret destinations of which the traveler is unaware. In a Telnet session, all communications, including username and password, are transmitted in plain-text, allowing anyone to listen-in on your session and steal passwords and other information. The Synchronize to Database function will give you a full picture of all database differences. In the middle of winter I at last discovered that there was in me an invincible summer. All the Navicat Cloud objects are located under different projects. You can share the project to other Navicat Cloud accounts for collaboration. Navicat Data Modeler is a powerful and cost-effective database design tool which helps you build high-quality conceptual, logical and physical data models. To clear or reload various internal caches, flush tables, or acquire locks, control-click your connection in the Navigation pane and select Flush and choose the flush option. You must have the reload privilege to use this feature. It can also manage cloud databases such as Amazon Redshift, Amazon RDS, Alibaba Cloud. Features in Navicat are sophisticated enough to provide professional developers for all their specific needs, yet easy to learn for users who are new to database server. You will succeed because most people are lazy. With its well-designed Graphical User Interface(GUI), Navicat lets you quickly and easily create, organize, access and share information in a secure and easy way. SQL Editor allows you to create and edit SQL text, prepare and execute selected queries. Success consists of going from failure to failure without loss of enthusiasm. In the middle of winter I at last discovered that there was in me an invincible summer. Secure SHell (SSH) is a program to log in into another computer over a network, execute commands on a remote server, and move files from one machine to another. Champions keep playing until they get it right. The On Startup feature allows you to control what tabs appear when you launch Navicat. To open a query using an external editor, control-click it and select Open with External Editor. You can set the file path of an external editor in Preferences. Instead of wondering when your next vacation is, maybe you should set up a life you don’t need to escape from. You will succeed because most people are lazy. If opportunity doesn’t knock, build a door. How we spend our days is, of course, how we spend our lives. After comparing data, the window shows the number of records that will be inserted, updated or deleted in the target. I may not have gone where I intended to go, but I think I have ended up where I needed to be. The Information Pane shows the detailed object information, project activities, the DDL of database objects, object dependencies, membership of users/roles and preview. In the Objects tab, you can use the List List, Detail Detail and ER Diagram ER Diagram buttons to change the object view. SQL Editor allows you to create and edit SQL text, prepare and execute selected queries. If opportunity doesn’t knock, build a door. How we spend our days is, of course, how we spend our lives. If it scares you, it might be a good thing to try. Difficult circumstances serve as a textbook of life for people. It collects process metrics such as CPU load, RAM usage, and a variety of other resources over SSH/SNMP. Navicat authorizes you to make connection to remote servers running on different platforms (i.e. Windows, macOS, Linux and UNIX), and supports PAM and GSSAPI authentication. Always keep your eyes open. Keep watching. Because whatever you see can inspire you. Remember that failure is an event, not a person.', N'A query is used to extract data from the database in a readable format according to the user''s request. Navicat provides a wide range advanced features, such as compelling code editing capabilities, smart code-completion, SQL formatting, and more. To start working with your server in Navicat, you should first establish a connection or several connections using the Connection window.', N'The reason why a great man is great is that he resolves to be a great man. It provides strong authentication and secure encrypted communications between two hosts, known as SSH Port Forwarding (Tunneling), over an insecure network. If the plan doesn’t work, change the plan, but never the goal. A man is not old until regrets take the place of dreams. Such sessions are also susceptible to session hijacking, where a malicious user takes over your session once you have authenticated. It wasn’t raining when Noah built the ark. The Synchronize to Database function will give you a full picture of all database differences. It provides strong authentication and secure encrypted communications between two hosts, known as SSH Port Forwarding (Tunneling), over an insecure network. I will greet this day with love in my heart. Remember that failure is an event, not a person. Creativity is intelligence having fun.', N'2021-12-10 08:02:48.0000000', N'2021-04-29 22:35:38.0000000', N'1', N'Secure SHell (SSH) is a program to log in into another computer over a network, execute commands on a remote server, and move files from one machine to another. The repository database can be an existing MySQL, MariaDB, PostgreSQL, SQL Server, or Amazon RDS instance. In the Objects tab, you can use the List List, Detail Detail and ER Diagram ER Diagram buttons to change the object view. Sometimes you win, sometimes you learn. Navicat Cloud provides a cloud service for synchronizing connections, queries, model files and virtual group information from Navicat, other Navicat family members, different machines and different platforms. The past has no power over the present moment. Typically, it is employed as an encrypted version of Telnet. A query is used to extract data from the database in a readable format according to the user''s request. Optimism is the one quality more associated with success and happiness than any other. The repository database can be an existing MySQL, MariaDB, PostgreSQL, SQL Server, or Amazon RDS instance. Sometimes you win, sometimes you learn. In the middle of winter I at last discovered that there was in me an invincible summer. Navicat 15 has added support for the system-wide dark mode. Instead of wondering when your next vacation is, maybe you should set up a life you don’t need to escape from. The Synchronize to Database function will give you a full picture of all database differences.', N'2018-10-04 11:23:21.0000000', N'D37074FA-953C-4F82-8DE5-290B53126FF9', NULL, N'AEFDBCF4-9F00-42A1-B592-B12681836ACD', N'AB4B8247-0DA2-43B2-8CD0-35E15D83F76E', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', N'1', N'0', N'You cannot save people, you can just love them. After logged in the Navicat Cloud feature, the Navigation pane will be divided into Navicat Cloud and My Connections sections. A query is used to extract data from the database in a readable format according to the user''s request. It wasn’t raining when Noah built the ark. Remember that failure is an event, not a person. To clear or reload various internal caches, flush tables, or acquire locks, control-click your connection in the Navigation pane and select Flush and choose the flush option. You must have the reload privilege to use this feature. The Synchronize to Database function will give you a full picture of all database differences. To connect to a database or schema, simply double-click it in the pane. Navicat Monitor can be installed on any local computer or virtual machine and does not require any software installation on the servers being monitored. Navicat allows you to transfer data from one database and/or schema to another with detailed analytical process. Import Wizard allows you to import data to tables/collections from CSV, TXT, XML, DBF and more. The repository database can be an existing MySQL, MariaDB, PostgreSQL, SQL Server, or Amazon RDS instance. To successfully establish a new connection to local/remote server - no matter via SSL or SSH, set the database login information in the General tab. Sometimes you win, sometimes you learn. You cannot save people, you can just love them. Monitored servers include MySQL, MariaDB and SQL Server, and compatible with cloud databases like Amazon RDS, Amazon Aurora, Oracle Cloud, Google Cloud and Microsoft Azure. A man’s best friends are his ten fingers. All journeys have secret destinations of which the traveler is unaware. Navicat Cloud provides a cloud service for synchronizing connections, queries, model files and virtual group information from Navicat, other Navicat family members, different machines and different platforms. What you get by achieving your goals is not as important as what you become by achieving your goals. The Main Window consists of several toolbars and panes for you to work on connections, database objects and advanced tools. To successfully establish a new connection to local/remote server - no matter via SSL, SSH or HTTP, set the database login information in the General tab. If the plan doesn’t work, change the plan, but never the goal. In the middle of winter I at last discovered that there was in me an invincible summer. Navicat provides a wide range advanced features, such as compelling code editing capabilities, smart code-completion, SQL formatting, and more. To clear or reload various internal caches, flush tables, or acquire locks, control-click your connection in the Navigation pane and select Flush and choose the flush option. You must have the reload privilege to use this feature. In a Telnet session, all communications, including username and password, are transmitted in plain-text, allowing anyone to listen-in on your session and steal passwords and other information. If you wait, all that happens is you get older. A man is not old until regrets take the place of dreams. It can also manage cloud databases such as Amazon Redshift, Amazon RDS, Alibaba Cloud. Features in Navicat are sophisticated enough to provide professional developers for all their specific needs, yet easy to learn for users who are new to database server. The Synchronize to Database function will give you a full picture of all database differences. The Information Pane shows the detailed object information, project activities, the DDL of database objects, object dependencies, membership of users/roles and preview. Actually it is just in an idea when feel oneself can achieve and cannot achieve. If you wait, all that happens is you get older. I will greet this day with love in my heart. The past has no power over the present moment. The On Startup feature allows you to control what tabs appear when you launch Navicat. Secure SHell (SSH) is a program to log in into another computer over a network, execute commands on a remote server, and move files from one machine to another. A query is used to extract data from the database in a readable format according to the user''s request. If the Show objects under schema in navigation pane option is checked at the Preferences window, all database objects are also displayed in the pane. Anyone who has ever made anything of importance was disciplined. Typically, it is employed as an encrypted version of Telnet. Navicat Monitor requires a repository to store alerts and metrics for historical analysis. If it scares you, it might be a good thing to try. After comparing data, the window shows the number of records that will be inserted, updated or deleted in the target. Navicat Cloud provides a cloud service for synchronizing connections, queries, model files and virtual group information from Navicat, other Navicat family members, different machines and different platforms. Monitored servers include MySQL, MariaDB and SQL Server, and compatible with cloud databases like Amazon RDS, Amazon Aurora, Oracle Cloud, Google Cloud and Microsoft Azure. To start working with your server in Navicat, you should first establish a connection or several connections using the Connection window. To start working with your server in Navicat, you should first establish a connection or several connections using the Connection window. SQL Editor allows you to create and edit SQL text, prepare and execute selected queries. Typically, it is employed as an encrypted version of Telnet. Navicat Data Modeler is a powerful and cost-effective database design tool which helps you build high-quality conceptual, logical and physical data models. The past has no power over the present moment. To open a query using an external editor, control-click it and select Open with External Editor. You can set the file path of an external editor in Preferences.', N'AEFDBCF4-9F00-42A1-B592-B12681836ACD', N'system', N'2025-02-25 03:02:56.0000000', N'The On Startup feature allows you to control what tabs appear when you launch Navicat. It can also manage cloud databases such as Amazon Redshift, Amazon RDS, Alibaba Cloud. Features in Navicat are sophisticated enough to provide professional developers for all their specific needs, yet easy to learn for users who are new to database server. Navicat authorizes you to make connection to remote servers running on different platforms (i.e. Windows, macOS, Linux and UNIX), and supports PAM and GSSAPI authentication.', N'2025-02-25 03:02:56.0000000', N'Visitor', N'677028', NULL)
GO

COMMIT
GO


-- ----------------------------
-- Table structure for user
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[user]') AND type IN ('U'))
	DROP TABLE [dbo].[user]
GO

CREATE TABLE [dbo].[user] (
  [id] uniqueidentifier  NOT NULL,
  [username] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [password] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [is_created_password] int  NOT NULL,
  [email] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [is_email_confirmation] int  NOT NULL,
  [email_confirmation_code] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [email_confirmation_expired_at] datetime2(7)  NOT NULL,
  [email_confirmation_at] datetime2(7)  NOT NULL,
  [last_login_at] datetime2(7)  NOT NULL,
  [status] int  NOT NULL,
  [group_id] uniqueidentifier  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [UserGroupId] uniqueidentifier  NULL,
  [is_integration] bit DEFAULT CONVERT([bit],(0)) NOT NULL,
  [created_at] datetime2(7) DEFAULT '0001-01-01T00:00:00.0000000' NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7) DEFAULT '0001-01-01T00:00:00.0000000' NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [can_alarm_action] bit  NULL,
  [can_approve_patrol] bit  NULL,
  [can_create_monitoring_config] bit  NULL,
  [can_update_monitoring_config] bit  NULL
)
GO

ALTER TABLE [dbo].[user] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of user
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[user] ([id], [username], [password], [is_created_password], [email], [is_email_confirmation], [email_confirmation_code], [email_confirmation_expired_at], [email_confirmation_at], [last_login_at], [status], [group_id], [application_id], [UserGroupId], [is_integration], [created_at], [created_by], [updated_at], [updated_by], [can_alarm_action], [can_approve_patrol], [can_create_monitoring_config], [can_update_monitoring_config]) VALUES (N'B53F464B-68B4-4831-AB9A-8F3E56D9FA33', N'superadmin', N'$2a$11$xPT6YN1K4QsRPCqX/uajXOZlsdeHrT7Ryhk3dRRXPjh0naia9h8wy', N'1', N'superadmin@test.com', N'1', N'CONFIRMED', N'2026-02-25 02:19:32.9353733', N'2026-02-25 02:19:32.9353905', N'2026-02-25 06:17:27.5023360', N'1', N'4451A28C-FA8D-4606-927D-022E1B51AC2E', N'C926D20B-A746-4492-9924-EB7EEE76305C', NULL, N'0', N'0001-01-01 00:00:00.0000000', NULL, N'0001-01-01 00:00:00.0000000', NULL, NULL, NULL, NULL, NULL), (N'FFD6BE83-1794-4DE8-B55A-CFF4943E3725', N'systemadmin', N'$2a$11$/F0sGUAbUdlZJhsyllhGheb6eDjLCtNTLd6ixvdJO7lf/b8XZFcFa', N'1', N'systemadmin@test.com', N'1', N'CONFIRMED', N'2026-02-25 02:19:33.1412997', N'2026-02-25 02:19:33.1413005', N'2026-02-25 02:19:33.1413005', N'1', N'682D7E20-13E4-4D0E-8A06-0BAF00E15CB0', N'C926D20B-A746-4492-9924-EB7EEE76305C', NULL, N'0', N'0001-01-01 00:00:00.0000000', NULL, N'0001-01-01 00:00:00.0000000', NULL, NULL, NULL, NULL, NULL)
GO

COMMIT
GO


-- ----------------------------
-- Table structure for user_building_access
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[user_building_access]') AND type IN ('U'))
	DROP TABLE [dbo].[user_building_access]
GO

CREATE TABLE [dbo].[user_building_access] (
  [id] uniqueidentifier  NOT NULL,
  [user_id] uniqueidentifier  NOT NULL,
  [building_id] uniqueidentifier  NOT NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [status] int  NOT NULL
)
GO

ALTER TABLE [dbo].[user_building_access] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of user_building_access
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for user_group
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[user_group]') AND type IN ('U'))
	DROP TABLE [dbo].[user_group]
GO

CREATE TABLE [dbo].[user_group] (
  [id] uniqueidentifier  NOT NULL,
  [name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [level_priority] int  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NOT NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [status] int  NULL,
  [_generate] bigint  IDENTITY(1,1) NOT NULL,
  [is_head] bit DEFAULT CONVERT([bit],(0)) NOT NULL
)
GO

ALTER TABLE [dbo].[user_group] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of user_group
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[user_group] ON
GO

INSERT INTO [dbo].[user_group] ([id], [name], [level_priority], [application_id], [created_by], [created_at], [updated_by], [updated_at], [status], [_generate], [is_head]) VALUES (N'4451A28C-FA8D-4606-927D-022E1B51AC2E', N'Super Admin', N'1', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2026-02-25 02:19:32.7045135', N'System', N'2026-02-25 02:19:32.7045135', N'1', N'1', N'0'), (N'682D7E20-13E4-4D0E-8A06-0BAF00E15CB0', N'System', N'0', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'System', N'2026-02-25 02:19:32.7044709', N'System', N'2026-02-25 02:19:32.7044811', N'1', N'2', N'0'), (N'3607AC98-8B2E-4F04-B06B-1B3F1A513984', N'Primary', N'3', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'superadmin', N'2026-02-25 06:28:08.8663579', N'superadmin', N'2026-02-25 06:28:08.8663586', N'1', N'4', N'0'), (N'2318C568-06B9-4418-A9C5-42CFCD8340DB', N'Secondary', N'4', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'superadmin', N'2026-02-25 06:28:16.0808994', N'superadmin', N'2026-02-25 06:28:16.0809001', N'1', N'5', N'0'), (N'943A7596-BB4B-40A4-9D17-7B319C50576B', N'UserCreated', N'5', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'superadmin', N'2026-02-25 06:28:22.9583547', N'superadmin', N'2026-02-25 06:28:22.9583551', N'1', N'6', N'0'), (N'6CEC490D-96DD-4E8B-AADC-A3BFF73E2CE9', N'Primary Admin', N'2', N'C926D20B-A746-4492-9924-EB7EEE76305C', N'superadmin', N'2026-02-25 06:28:02.8719031', N'superadmin', N'2026-02-25 06:28:02.8719612', N'1', N'3', N'0')
GO

SET IDENTITY_INSERT [dbo].[user_group] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for visitor
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[visitor]') AND type IN ('U'))
	DROP TABLE [dbo].[visitor]
GO

CREATE TABLE [dbo].[visitor] (
  [id] uniqueidentifier  NOT NULL,
  [person_id] nvarchar(450) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [identity_id] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [identity_type] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [card_number] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [ble_card_number] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [name] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [phone] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [email] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [gender] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [address] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [organization_name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [district_name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [department_name] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [visitor_group_code] bigint  NULL,
  [visitor_number] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [visitor_code] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [is_vip] bit  NULL,
  [status] int  NOT NULL,
  [face_image] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [upload_fr] int  NOT NULL,
  [upload_fr_error] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [application_id] uniqueidentifier  NOT NULL,
  [MstApplicationId] uniqueidentifier  NULL,
  [created_by] nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [created_at] datetime2(7)  NOT NULL,
  [updated_by] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [updated_at] datetime2(7)  NOT NULL,
  [blacklist_reason] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS  NULL,
  [is_blacklist] bit  NULL
)
GO

ALTER TABLE [dbo].[visitor] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of visitor
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[visitor] ([id], [person_id], [identity_id], [identity_type], [card_number], [ble_card_number], [name], [phone], [email], [gender], [address], [organization_name], [district_name], [department_name], [visitor_group_code], [visitor_number], [visitor_code], [is_vip], [status], [face_image], [upload_fr], [upload_fr_error], [application_id], [MstApplicationId], [created_by], [created_at], [updated_by], [updated_at], [blacklist_reason], [is_blacklist]) VALUES (N'AEFDBCF4-9F00-42A1-B592-B12681836ACD', N'VIS001', NULL, NULL, NULL, NULL, N'Charlie Visitor', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'1', NULL, N'0', NULL, N'C926D20B-A746-4492-9924-EB7EEE76305C', NULL, NULL, N'0001-01-01 00:00:00.0000000', NULL, N'0001-01-01 00:00:00.0000000', NULL, NULL)
GO

COMMIT
GO


-- ----------------------------
-- Primary Key structure for table __EFMigrationsHistory
-- ----------------------------
ALTER TABLE [dbo].[__EFMigrationsHistory] ADD CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table alarm_category_settings
-- ----------------------------
ALTER TABLE [dbo].[alarm_category_settings] ADD CONSTRAINT [PK_alarm_category_settings] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for alarm_record_tracking
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[alarm_record_tracking]', RESEED, 1)
GO


-- ----------------------------
-- Indexes structure for table alarm_record_tracking
-- ----------------------------
CREATE UNIQUE NONCLUSTERED INDEX [alarm_record_tracking__generate_unique]
ON [dbo].[alarm_record_tracking] (
  [_generate] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_record_tracking_alarm_triggers_id]
ON [dbo].[alarm_record_tracking] (
  [alarm_triggers_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_record_tracking_application_id]
ON [dbo].[alarm_record_tracking] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_record_tracking_ble_reader_id]
ON [dbo].[alarm_record_tracking] (
  [ble_reader_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_record_tracking_floorplan_masked_area_id]
ON [dbo].[alarm_record_tracking] (
  [floorplan_masked_area_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_record_tracking_FloorplanMaskedAreaId1]
ON [dbo].[alarm_record_tracking] (
  [FloorplanMaskedAreaId1] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_record_tracking_MstApplicationId]
ON [dbo].[alarm_record_tracking] (
  [MstApplicationId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_record_tracking_MstBleReaderId]
ON [dbo].[alarm_record_tracking] (
  [MstBleReaderId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_record_tracking_visitor_id]
ON [dbo].[alarm_record_tracking] (
  [visitor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_record_tracking_VisitorId1]
ON [dbo].[alarm_record_tracking] (
  [VisitorId1] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_record_tracking_member_id]
ON [dbo].[alarm_record_tracking] (
  [member_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_record_tracking_MstSecurityId]
ON [dbo].[alarm_record_tracking] (
  [MstSecurityId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table alarm_record_tracking
-- ----------------------------
ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [PK_alarm_record_tracking] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table alarm_triggers
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_alarm_triggers_application_id]
ON [dbo].[alarm_triggers] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_triggers_floorplan_id]
ON [dbo].[alarm_triggers] (
  [floorplan_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_triggers_member_id]
ON [dbo].[alarm_triggers] (
  [member_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_triggers_visitor_id]
ON [dbo].[alarm_triggers] (
  [visitor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_triggers_MstSecurityId]
ON [dbo].[alarm_triggers] (
  [MstSecurityId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_alarm_triggers_security_id]
ON [dbo].[alarm_triggers] (
  [security_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table alarm_triggers
-- ----------------------------
ALTER TABLE [dbo].[alarm_triggers] ADD CONSTRAINT [PK_alarm_triggers] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table boundary
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_boundary_application_id]
ON [dbo].[boundary] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_boundary_floor_id]
ON [dbo].[boundary] (
  [floor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_boundary_floorplan_id]
ON [dbo].[boundary] (
  [floorplan_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table boundary
-- ----------------------------
ALTER TABLE [dbo].[boundary] ADD CONSTRAINT [PK_boundary] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table card
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_card_application_id]
ON [dbo].[card] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_card_group_id]
ON [dbo].[card] (
  [card_group_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_card_number]
ON [dbo].[card] (
  [card_number] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_dmac]
ON [dbo].[card] (
  [dmac] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_member_id]
ON [dbo].[card] (
  [member_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_registered_masked_area_id]
ON [dbo].[card] (
  [registered_masked_area_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_visitor_id]
ON [dbo].[card] (
  [visitor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_security_id]
ON [dbo].[card] (
  [security_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table card
-- ----------------------------
ALTER TABLE [dbo].[card] ADD CONSTRAINT [PK_card] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table card_access_masked_areas
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_card_access_masked_areas_masked_area_id]
ON [dbo].[card_access_masked_areas] (
  [masked_area_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table card_access_masked_areas
-- ----------------------------
ALTER TABLE [dbo].[card_access_masked_areas] ADD CONSTRAINT [PK_card_access_masked_areas] PRIMARY KEY CLUSTERED ([card_access_id], [masked_area_id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table card_access_time_groups
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_card_access_time_groups_time_group_id]
ON [dbo].[card_access_time_groups] (
  [time_group_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table card_access_time_groups
-- ----------------------------
ALTER TABLE [dbo].[card_access_time_groups] ADD CONSTRAINT [PK_card_access_time_groups] PRIMARY KEY CLUSTERED ([card_access_id], [time_group_id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table card_accesses
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_card_accesses_application_id]
ON [dbo].[card_accesses] (
  [application_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table card_accesses
-- ----------------------------
ALTER TABLE [dbo].[card_accesses] ADD CONSTRAINT [PK_card_accesses] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table card_card_accesses
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_card_card_accesses_card_access_id]
ON [dbo].[card_card_accesses] (
  [card_access_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table card_card_accesses
-- ----------------------------
ALTER TABLE [dbo].[card_card_accesses] ADD CONSTRAINT [PK_card_card_accesses] PRIMARY KEY CLUSTERED ([card_id], [card_access_id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table card_groups
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_card_groups_application_id]
ON [dbo].[card_groups] (
  [application_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table card_groups
-- ----------------------------
ALTER TABLE [dbo].[card_groups] ADD CONSTRAINT [PK_card_groups] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table card_record
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_card_record_application_id]
ON [dbo].[card_record] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_record_card_id]
ON [dbo].[card_record] (
  [card_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_record_CardId1]
ON [dbo].[card_record] (
  [CardId1] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_record_member_id]
ON [dbo].[card_record] (
  [member_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_record_MstMemberId]
ON [dbo].[card_record] (
  [MstMemberId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_record_visitor_id]
ON [dbo].[card_record] (
  [visitor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_record_VisitorId1]
ON [dbo].[card_record] (
  [VisitorId1] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_record_MstSecurityId]
ON [dbo].[card_record] (
  [MstSecurityId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table card_record
-- ----------------------------
ALTER TABLE [dbo].[card_record] ADD CONSTRAINT [PK_card_record] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table card_swap_transaction
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_card_swap_transaction_application_id]
ON [dbo].[card_swap_transaction] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_swap_transaction_executed_at]
ON [dbo].[card_swap_transaction] (
  [executed_at] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_swap_transaction_from_card_id]
ON [dbo].[card_swap_transaction] (
  [from_card_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_swap_transaction_masked_area_id]
ON [dbo].[card_swap_transaction] (
  [masked_area_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_swap_transaction_swap_chain_id]
ON [dbo].[card_swap_transaction] (
  [swap_chain_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_swap_transaction_swap_sequence]
ON [dbo].[card_swap_transaction] (
  [swap_sequence] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_swap_transaction_to_card_id]
ON [dbo].[card_swap_transaction] (
  [to_card_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_swap_transaction_trx_visitor_id]
ON [dbo].[card_swap_transaction] (
  [trx_visitor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_card_swap_transaction_visitor_id]
ON [dbo].[card_swap_transaction] (
  [visitor_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table card_swap_transaction
-- ----------------------------
ALTER TABLE [dbo].[card_swap_transaction] ADD CONSTRAINT [PK_card_swap_transaction] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table evacuation_alerts
-- ----------------------------
ALTER TABLE [dbo].[evacuation_alerts] ADD CONSTRAINT [PK_evacuation_alerts] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table evacuation_assembly_points
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_evacuation_assembly_points_floor_id]
ON [dbo].[evacuation_assembly_points] (
  [floor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_evacuation_assembly_points_floorplan_id]
ON [dbo].[evacuation_assembly_points] (
  [floorplan_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_evacuation_assembly_points_floorplan_masked_area_id]
ON [dbo].[evacuation_assembly_points] (
  [floorplan_masked_area_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table evacuation_assembly_points
-- ----------------------------
ALTER TABLE [dbo].[evacuation_assembly_points] ADD CONSTRAINT [PK_evacuation_assembly_points] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table evacuation_transactions
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_evacuation_transactions_card_id]
ON [dbo].[evacuation_transactions] (
  [card_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_evacuation_transactions_evacuation_alert_id]
ON [dbo].[evacuation_transactions] (
  [evacuation_alert_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_evacuation_transactions_evacuation_assembly_point_id]
ON [dbo].[evacuation_transactions] (
  [evacuation_assembly_point_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_evacuation_transactions_member_id]
ON [dbo].[evacuation_transactions] (
  [member_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_evacuation_transactions_security_id]
ON [dbo].[evacuation_transactions] (
  [security_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_evacuation_transactions_visitor_id]
ON [dbo].[evacuation_transactions] (
  [visitor_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table evacuation_transactions
-- ----------------------------
ALTER TABLE [dbo].[evacuation_transactions] ADD CONSTRAINT [PK_evacuation_transactions] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for floorplan_device
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[floorplan_device]', RESEED, 5)
GO


-- ----------------------------
-- Indexes structure for table floorplan_device
-- ----------------------------
CREATE UNIQUE NONCLUSTERED INDEX [IX_floorplan_device__generate]
ON [dbo].[floorplan_device] (
  [_generate] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_device_access_cctv_id]
ON [dbo].[floorplan_device] (
  [access_cctv_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_device_access_control_id]
ON [dbo].[floorplan_device] (
  [access_control_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_device_application_id]
ON [dbo].[floorplan_device] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_device_ble_reader_id]
ON [dbo].[floorplan_device] (
  [ble_reader_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_device_floorplan_id]
ON [dbo].[floorplan_device] (
  [floorplan_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_device_floorplan_masked_area_id]
ON [dbo].[floorplan_device] (
  [floorplan_masked_area_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_device_FloorplanMaskedAreaId1]
ON [dbo].[floorplan_device] (
  [FloorplanMaskedAreaId1] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_device_MstAccessCctvId]
ON [dbo].[floorplan_device] (
  [MstAccessCctvId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_device_MstAccessControlId]
ON [dbo].[floorplan_device] (
  [MstAccessControlId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_device_MstApplicationId]
ON [dbo].[floorplan_device] (
  [MstApplicationId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_device_MstBleReaderId]
ON [dbo].[floorplan_device] (
  [MstBleReaderId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table floorplan_device
-- ----------------------------
ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [PK_floorplan_device] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table floorplan_masked_area
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_floorplan_masked_area_application_id]
ON [dbo].[floorplan_masked_area] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_masked_area_floor_id]
ON [dbo].[floorplan_masked_area] (
  [floor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_masked_area_floorplan_id]
ON [dbo].[floorplan_masked_area] (
  [floorplan_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_floorplan_masked_area_MstFloorId]
ON [dbo].[floorplan_masked_area] (
  [MstFloorId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table floorplan_masked_area
-- ----------------------------
ALTER TABLE [dbo].[floorplan_masked_area] ADD CONSTRAINT [PK_floorplan_masked_area] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table geofence
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_geofence_application_id]
ON [dbo].[geofence] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_geofence_floor_id]
ON [dbo].[geofence] (
  [floor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_geofence_floorplan_id]
ON [dbo].[geofence] (
  [floorplan_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table geofence
-- ----------------------------
ALTER TABLE [dbo].[geofence] ADD CONSTRAINT [PK_geofence] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table group_building_access
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_group_building_access_building_id]
ON [dbo].[group_building_access] (
  [building_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_group_building_access_group_id]
ON [dbo].[group_building_access] (
  [group_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table group_building_access
-- ----------------------------
ALTER TABLE [dbo].[group_building_access] ADD CONSTRAINT [PK_group_building_access] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table monitoring_config
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_monitoring_config_application_id]
ON [dbo].[monitoring_config] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_monitoring_config_building_id]
ON [dbo].[monitoring_config] (
  [building_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table monitoring_config
-- ----------------------------
ALTER TABLE [dbo].[monitoring_config] ADD CONSTRAINT [PK_monitoring_config] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table monitoring_config_building_access
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_monitoring_config_building_access_building_id]
ON [dbo].[monitoring_config_building_access] (
  [building_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_monitoring_config_building_access_monitoring_config_id]
ON [dbo].[monitoring_config_building_access] (
  [monitoring_config_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table monitoring_config_building_access
-- ----------------------------
ALTER TABLE [dbo].[monitoring_config_building_access] ADD CONSTRAINT [PK_monitoring_config_building_access] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table mst_access_cctv
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_access_cctv_application_id]
ON [dbo].[mst_access_cctv] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_access_cctv_integration_id]
ON [dbo].[mst_access_cctv] (
  [integration_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_access_cctv_MstApplicationId]
ON [dbo].[mst_access_cctv] (
  [MstApplicationId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_access_cctv
-- ----------------------------
ALTER TABLE [dbo].[mst_access_cctv] ADD CONSTRAINT [PK_mst_access_cctv] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table mst_access_control
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_access_control_application_id]
ON [dbo].[mst_access_control] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_access_control_controller_brand_id]
ON [dbo].[mst_access_control] (
  [controller_brand_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_access_control_integration_id]
ON [dbo].[mst_access_control] (
  [integration_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_access_control_MstApplicationId]
ON [dbo].[mst_access_control] (
  [MstApplicationId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_access_control
-- ----------------------------
ALTER TABLE [dbo].[mst_access_control] ADD CONSTRAINT [PK_mst_access_control] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for mst_application
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[mst_application]', RESEED, 1)
GO


-- ----------------------------
-- Primary Key structure for table mst_application
-- ----------------------------
ALTER TABLE [dbo].[mst_application] ADD CONSTRAINT [PK_mst_application] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table mst_ble_reader
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_ble_reader_application_id]
ON [dbo].[mst_ble_reader] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_ble_reader_brand_id]
ON [dbo].[mst_ble_reader] (
  [brand_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_ble_reader
-- ----------------------------
ALTER TABLE [dbo].[mst_ble_reader] ADD CONSTRAINT [PK_mst_ble_reader] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for mst_brand
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[mst_brand]', RESEED, 1)
GO


-- ----------------------------
-- Indexes structure for table mst_brand
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_brand_application_id]
ON [dbo].[mst_brand] (
  [application_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_brand
-- ----------------------------
ALTER TABLE [dbo].[mst_brand] ADD CONSTRAINT [PK_mst_brand] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for mst_building
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[mst_building]', RESEED, 1)
GO


-- ----------------------------
-- Indexes structure for table mst_building
-- ----------------------------
CREATE UNIQUE NONCLUSTERED INDEX [IX_mst_building__generate]
ON [dbo].[mst_building] (
  [_generate] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_building_ApplicationId]
ON [dbo].[mst_building] (
  [ApplicationId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_building_MstApplicationId]
ON [dbo].[mst_building] (
  [MstApplicationId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_building
-- ----------------------------
ALTER TABLE [dbo].[mst_building] ADD CONSTRAINT [PK_mst_building] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for mst_department
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[mst_department]', RESEED, 1)
GO


-- ----------------------------
-- Indexes structure for table mst_department
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_department_application_id]
ON [dbo].[mst_department] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_department_MstApplicationId]
ON [dbo].[mst_department] (
  [MstApplicationId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_department
-- ----------------------------
ALTER TABLE [dbo].[mst_department] ADD CONSTRAINT [PK_mst_department] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for mst_district
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[mst_district]', RESEED, 1)
GO


-- ----------------------------
-- Indexes structure for table mst_district
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_district_application_id]
ON [dbo].[mst_district] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_district_MstApplicationId]
ON [dbo].[mst_district] (
  [MstApplicationId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_district
-- ----------------------------
ALTER TABLE [dbo].[mst_district] ADD CONSTRAINT [PK_mst_district] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table mst_engine
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_engine_application_id]
ON [dbo].[mst_engine] (
  [application_id] ASC
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_mst_engine_engine_tracking_id]
ON [dbo].[mst_engine] (
  [engine_tracking_id] ASC
)
WHERE ([engine_tracking_id] IS NOT NULL)
GO


-- ----------------------------
-- Primary Key structure for table mst_engine
-- ----------------------------
ALTER TABLE [dbo].[mst_engine] ADD CONSTRAINT [PK_mst_engine] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for mst_floor
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[mst_floor]', RESEED, 1)
GO


-- ----------------------------
-- Indexes structure for table mst_floor
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_floor_application_id]
ON [dbo].[mst_floor] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_floor_building_id]
ON [dbo].[mst_floor] (
  [building_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_floor
-- ----------------------------
ALTER TABLE [dbo].[mst_floor] ADD CONSTRAINT [PK_mst_floor] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for mst_floorplan
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[mst_floorplan]', RESEED, 1)
GO


-- ----------------------------
-- Indexes structure for table mst_floorplan
-- ----------------------------
CREATE UNIQUE NONCLUSTERED INDEX [IX_mst_floorplan__generate]
ON [dbo].[mst_floorplan] (
  [_generate] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_floorplan_application_id]
ON [dbo].[mst_floorplan] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_floorplan_floor_id]
ON [dbo].[mst_floorplan] (
  [floor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_floorplan_MstApplicationId]
ON [dbo].[mst_floorplan] (
  [MstApplicationId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_floorplan_MstFloorId]
ON [dbo].[mst_floorplan] (
  [MstFloorId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_floorplan_engine_id]
ON [dbo].[mst_floorplan] (
  [engine_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_floorplan
-- ----------------------------
ALTER TABLE [dbo].[mst_floorplan] ADD CONSTRAINT [PK_mst_floorplan] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for mst_integration
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[mst_integration]', RESEED, 1)
GO


-- ----------------------------
-- Indexes structure for table mst_integration
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_integration_application_id]
ON [dbo].[mst_integration] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_integration_brand_id]
ON [dbo].[mst_integration] (
  [brand_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_integration
-- ----------------------------
ALTER TABLE [dbo].[mst_integration] ADD CONSTRAINT [PK_mst_integration] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table mst_member
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_member_application_id]
ON [dbo].[mst_member] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_member_department_id]
ON [dbo].[mst_member] (
  [department_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_member_district_id]
ON [dbo].[mst_member] (
  [district_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_member_email]
ON [dbo].[mst_member] (
  [email] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_member_MstApplicationId]
ON [dbo].[mst_member] (
  [MstApplicationId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_member_MstDepartmentId]
ON [dbo].[mst_member] (
  [MstDepartmentId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_member_MstDistrictId]
ON [dbo].[mst_member] (
  [MstDistrictId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_member_MstOrganizationId]
ON [dbo].[mst_member] (
  [MstOrganizationId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_member_organization_id]
ON [dbo].[mst_member] (
  [organization_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_member_person_id]
ON [dbo].[mst_member] (
  [person_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_member
-- ----------------------------
ALTER TABLE [dbo].[mst_member] ADD CONSTRAINT [PK_mst_member] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for mst_organization
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[mst_organization]', RESEED, 3)
GO


-- ----------------------------
-- Indexes structure for table mst_organization
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_organization_application_id]
ON [dbo].[mst_organization] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_organization_MstApplicationId]
ON [dbo].[mst_organization] (
  [MstApplicationId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_organization
-- ----------------------------
ALTER TABLE [dbo].[mst_organization] ADD CONSTRAINT [PK_mst_organization] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table mst_security
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_mst_security_application_id]
ON [dbo].[mst_security] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_security_department_id]
ON [dbo].[mst_security] (
  [department_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_security_district_id]
ON [dbo].[mst_security] (
  [district_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_security_email]
ON [dbo].[mst_security] (
  [email] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_security_organization_id]
ON [dbo].[mst_security] (
  [organization_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_security_person_id]
ON [dbo].[mst_security] (
  [person_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_security_security_head_1]
ON [dbo].[mst_security] (
  [security_head_1] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_mst_security_security_head_2]
ON [dbo].[mst_security] (
  [security_head_2] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table mst_security
-- ----------------------------
ALTER TABLE [dbo].[mst_security] ADD CONSTRAINT [PK_mst_security] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table overpopulating
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_overpopulating_application_id]
ON [dbo].[overpopulating] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_overpopulating_floor_id]
ON [dbo].[overpopulating] (
  [floor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_overpopulating_floorplan_id]
ON [dbo].[overpopulating] (
  [floorplan_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table overpopulating
-- ----------------------------
ALTER TABLE [dbo].[overpopulating] ADD CONSTRAINT [PK_overpopulating] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table patrol_area
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_patrol_area_application_id]
ON [dbo].[patrol_area] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_area_floor_id]
ON [dbo].[patrol_area] (
  [floor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_area_floorplan_id]
ON [dbo].[patrol_area] (
  [floorplan_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table patrol_area
-- ----------------------------
ALTER TABLE [dbo].[patrol_area] ADD CONSTRAINT [PK_patrol_area] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table patrol_assignment
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_patrol_assignment_application_id]
ON [dbo].[patrol_assignment] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_assignment_patrol_route_id]
ON [dbo].[patrol_assignment] (
  [patrol_route_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_assignment_time_group_id]
ON [dbo].[patrol_assignment] (
  [time_group_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table patrol_assignment
-- ----------------------------
ALTER TABLE [dbo].[patrol_assignment] ADD CONSTRAINT [PK_patrol_assignment] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table patrol_assignment_security
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_patrol_assignment_security_application_id]
ON [dbo].[patrol_assignment_security] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_assignment_security_patrol_assignment_id]
ON [dbo].[patrol_assignment_security] (
  [patrol_assignment_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_assignment_security_security_id]
ON [dbo].[patrol_assignment_security] (
  [security_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table patrol_assignment_security
-- ----------------------------
ALTER TABLE [dbo].[patrol_assignment_security] ADD CONSTRAINT [PK_patrol_assignment_security] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table patrol_case
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_patrol_case_application_id]
ON [dbo].[patrol_case] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_case_security_head_1]
ON [dbo].[patrol_case] (
  [security_head_1] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_case_patrol_assignment_id]
ON [dbo].[patrol_case] (
  [patrol_assignment_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_case_patrol_route_id]
ON [dbo].[patrol_case] (
  [patrol_route_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_case_patrol_session_id]
ON [dbo].[patrol_case] (
  [patrol_session_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_case_security_id]
ON [dbo].[patrol_case] (
  [security_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_case_security_head_2]
ON [dbo].[patrol_case] (
  [security_head_2] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_case_approved_by_head_1_id]
ON [dbo].[patrol_case] (
  [approved_by_head_1_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_case_approved_by_head_2_id]
ON [dbo].[patrol_case] (
  [approved_by_head_2_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table patrol_case
-- ----------------------------
ALTER TABLE [dbo].[patrol_case] ADD CONSTRAINT [PK_patrol_case] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table patrol_case_attachment
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_patrol_case_attachment_application_id]
ON [dbo].[patrol_case_attachment] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_case_attachment_patrol_case_id]
ON [dbo].[patrol_case_attachment] (
  [patrol_case_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table patrol_case_attachment
-- ----------------------------
ALTER TABLE [dbo].[patrol_case_attachment] ADD CONSTRAINT [PK_patrol_case_attachment] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table patrol_checkpoint_log
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_patrol_checkpoint_log_application_id]
ON [dbo].[patrol_checkpoint_log] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_checkpoint_log_patrol_area_id]
ON [dbo].[patrol_checkpoint_log] (
  [patrol_area_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_checkpoint_log_patrol_session_id]
ON [dbo].[patrol_checkpoint_log] (
  [patrol_session_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table patrol_checkpoint_log
-- ----------------------------
ALTER TABLE [dbo].[patrol_checkpoint_log] ADD CONSTRAINT [PK_patrol_checkpoint_log] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table patrol_route
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_patrol_route_application_id]
ON [dbo].[patrol_route] (
  [application_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table patrol_route
-- ----------------------------
ALTER TABLE [dbo].[patrol_route] ADD CONSTRAINT [PK_patrol_route] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table patrol_route_areas
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_patrol_route_areas_application_id]
ON [dbo].[patrol_route_areas] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_route_areas_patrol_area_id]
ON [dbo].[patrol_route_areas] (
  [patrol_area_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_route_areas_patrol_route_id]
ON [dbo].[patrol_route_areas] (
  [patrol_route_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table patrol_route_areas
-- ----------------------------
ALTER TABLE [dbo].[patrol_route_areas] ADD CONSTRAINT [PK_patrol_route_areas] PRIMARY KEY CLUSTERED ([patrol_area_id], [patrol_route_id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table patrol_session
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_patrol_session_application_id]
ON [dbo].[patrol_session] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_session_patrol_assignment_id]
ON [dbo].[patrol_session] (
  [patrol_assignment_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_session_patrol_route_id]
ON [dbo].[patrol_session] (
  [patrol_route_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_session_security_id]
ON [dbo].[patrol_session] (
  [security_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_patrol_session_time_group_id]
ON [dbo].[patrol_session] (
  [time_group_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table patrol_session
-- ----------------------------
ALTER TABLE [dbo].[patrol_session] ADD CONSTRAINT [PK_patrol_session] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table refresh_token
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_refresh_token_user_id]
ON [dbo].[refresh_token] (
  [user_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table refresh_token
-- ----------------------------
ALTER TABLE [dbo].[refresh_token] ADD CONSTRAINT [PK_refresh_token] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table security_group
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_security_group_application_id]
ON [dbo].[security_group] (
  [application_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table security_group
-- ----------------------------
ALTER TABLE [dbo].[security_group] ADD CONSTRAINT [PK_security_group] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table security_group_member
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_security_group_member_application_id]
ON [dbo].[security_group_member] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_security_group_member_security_group_id]
ON [dbo].[security_group_member] (
  [security_group_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_security_group_member_security_id]
ON [dbo].[security_group_member] (
  [security_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table security_group_member
-- ----------------------------
ALTER TABLE [dbo].[security_group_member] ADD CONSTRAINT [PK_security_group_member] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table stay_on_area
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_stay_on_area_application_id]
ON [dbo].[stay_on_area] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_stay_on_area_floor_id]
ON [dbo].[stay_on_area] (
  [floor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_stay_on_area_floorplan_id]
ON [dbo].[stay_on_area] (
  [floorplan_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table stay_on_area
-- ----------------------------
ALTER TABLE [dbo].[stay_on_area] ADD CONSTRAINT [PK_stay_on_area] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table time_block
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_time_block_application_id]
ON [dbo].[time_block] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_time_block_time_group_id]
ON [dbo].[time_block] (
  [time_group_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table time_block
-- ----------------------------
ALTER TABLE [dbo].[time_block] ADD CONSTRAINT [PK_time_block] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table time_group
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_time_group_application_id]
ON [dbo].[time_group] (
  [application_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table time_group
-- ----------------------------
ALTER TABLE [dbo].[time_group] ADD CONSTRAINT [PK_time_group] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table tracking_report_presets
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_tracking_report_presets_application_id]
ON [dbo].[tracking_report_presets] (
  [application_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table tracking_report_presets
-- ----------------------------
ALTER TABLE [dbo].[tracking_report_presets] ADD CONSTRAINT [PK_tracking_report_presets] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table tracking_transaction
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_tracking_transaction_application_id]
ON [dbo].[tracking_transaction] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_tracking_transaction_card_id]
ON [dbo].[tracking_transaction] (
  [card_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_tracking_transaction_floorplan_masked_area_id]
ON [dbo].[tracking_transaction] (
  [floorplan_masked_area_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_tracking_transaction_FloorplanMaskedAreaId1]
ON [dbo].[tracking_transaction] (
  [FloorplanMaskedAreaId1] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_tracking_transaction_MstBleReaderId]
ON [dbo].[tracking_transaction] (
  [MstBleReaderId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_tracking_transaction_reader_id]
ON [dbo].[tracking_transaction] (
  [reader_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_tracking_transaction_member_id]
ON [dbo].[tracking_transaction] (
  [member_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_tracking_transaction_visitor_id]
ON [dbo].[tracking_transaction] (
  [visitor_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table tracking_transaction
-- ----------------------------
ALTER TABLE [dbo].[tracking_transaction] ADD CONSTRAINT [PK_tracking_transaction] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table trx_visitor
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_trx_visitor_application_id]
ON [dbo].[trx_visitor] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_trx_visitor_masked_area_id]
ON [dbo].[trx_visitor] (
  [masked_area_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_trx_visitor_purpose_person_id]
ON [dbo].[trx_visitor] (
  [purpose_person_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_trx_visitor_visitor_id]
ON [dbo].[trx_visitor] (
  [visitor_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_trx_visitor_visitor_id_visitor_period_start_visitor_period_end]
ON [dbo].[trx_visitor] (
  [visitor_id] ASC,
  [visitor_period_start] ASC,
  [visitor_period_end] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_trx_visitor_visitor_id_visitor_status]
ON [dbo].[trx_visitor] (
  [visitor_id] ASC,
  [visitor_status] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_trx_visitor_visitor_period_start]
ON [dbo].[trx_visitor] (
  [visitor_period_start] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_trx_visitor_visitor_status]
ON [dbo].[trx_visitor] (
  [visitor_status] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_trx_visitor_VisitorId1]
ON [dbo].[trx_visitor] (
  [VisitorId1] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table trx_visitor
-- ----------------------------
ALTER TABLE [dbo].[trx_visitor] ADD CONSTRAINT [PK_trx_visitor] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table user
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_user_application_id]
ON [dbo].[user] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_user_group_id]
ON [dbo].[user] (
  [group_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_user_UserGroupId]
ON [dbo].[user] (
  [UserGroupId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table user
-- ----------------------------
ALTER TABLE [dbo].[user] ADD CONSTRAINT [PK_user] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table user_building_access
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_user_building_access_building_id]
ON [dbo].[user_building_access] (
  [building_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_user_building_access_user_id]
ON [dbo].[user_building_access] (
  [user_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table user_building_access
-- ----------------------------
ALTER TABLE [dbo].[user_building_access] ADD CONSTRAINT [PK_user_building_access] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for user_group
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[user_group]', RESEED, 6)
GO


-- ----------------------------
-- Indexes structure for table user_group
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_user_group_application_id]
ON [dbo].[user_group] (
  [application_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table user_group
-- ----------------------------
ALTER TABLE [dbo].[user_group] ADD CONSTRAINT [PK_user_group] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Indexes structure for table visitor
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_visitor_application_id]
ON [dbo].[visitor] (
  [application_id] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_visitor_email]
ON [dbo].[visitor] (
  [email] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_visitor_MstApplicationId]
ON [dbo].[visitor] (
  [MstApplicationId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_visitor_person_id]
ON [dbo].[visitor] (
  [person_id] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table visitor
-- ----------------------------
ALTER TABLE [dbo].[visitor] ADD CONSTRAINT [PK_visitor] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Foreign Keys structure for table alarm_record_tracking
-- ----------------------------
ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [FK_alarm_record_tracking_mst_member_member_id] FOREIGN KEY ([member_id]) REFERENCES [dbo].[mst_member] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [FK_alarm_record_tracking_mst_security_MstSecurityId] FOREIGN KEY ([MstSecurityId]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [FK_alarm_record_tracking_alarm_triggers_alarm_triggers_id] FOREIGN KEY ([alarm_triggers_id]) REFERENCES [dbo].[alarm_triggers] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [FK_alarm_record_tracking_floorplan_masked_area_FloorplanMaskedAreaId1] FOREIGN KEY ([FloorplanMaskedAreaId1]) REFERENCES [dbo].[floorplan_masked_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [FK_alarm_record_tracking_floorplan_masked_area_floorplan_masked_area_id] FOREIGN KEY ([floorplan_masked_area_id]) REFERENCES [dbo].[floorplan_masked_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [FK_alarm_record_tracking_mst_application_MstApplicationId] FOREIGN KEY ([MstApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [FK_alarm_record_tracking_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [FK_alarm_record_tracking_mst_ble_reader_MstBleReaderId] FOREIGN KEY ([MstBleReaderId]) REFERENCES [dbo].[mst_ble_reader] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [FK_alarm_record_tracking_mst_ble_reader_ble_reader_id] FOREIGN KEY ([ble_reader_id]) REFERENCES [dbo].[mst_ble_reader] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [FK_alarm_record_tracking_visitor_VisitorId1] FOREIGN KEY ([VisitorId1]) REFERENCES [dbo].[visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_record_tracking] ADD CONSTRAINT [FK_alarm_record_tracking_visitor_visitor_id] FOREIGN KEY ([visitor_id]) REFERENCES [dbo].[visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table alarm_triggers
-- ----------------------------
ALTER TABLE [dbo].[alarm_triggers] ADD CONSTRAINT [FK_alarm_triggers_mst_member_member_id] FOREIGN KEY ([member_id]) REFERENCES [dbo].[mst_member] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_triggers] ADD CONSTRAINT [FK_alarm_triggers_visitor_visitor_id] FOREIGN KEY ([visitor_id]) REFERENCES [dbo].[visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_triggers] ADD CONSTRAINT [FK_alarm_triggers_mst_security_MstSecurityId] FOREIGN KEY ([MstSecurityId]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_triggers] ADD CONSTRAINT [FK_alarm_triggers_mst_security_security_id] FOREIGN KEY ([security_id]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_triggers] ADD CONSTRAINT [FK_alarm_triggers_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[alarm_triggers] ADD CONSTRAINT [FK_alarm_triggers_mst_floorplan_floorplan_id] FOREIGN KEY ([floorplan_id]) REFERENCES [dbo].[mst_floorplan] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table boundary
-- ----------------------------
ALTER TABLE [dbo].[boundary] ADD CONSTRAINT [FK_boundary_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[boundary] ADD CONSTRAINT [FK_boundary_mst_floor_floor_id] FOREIGN KEY ([floor_id]) REFERENCES [dbo].[mst_floor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[boundary] ADD CONSTRAINT [FK_boundary_mst_floorplan_floorplan_id] FOREIGN KEY ([floorplan_id]) REFERENCES [dbo].[mst_floorplan] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table card
-- ----------------------------
ALTER TABLE [dbo].[card] ADD CONSTRAINT [FK_card_mst_security_security_id] FOREIGN KEY ([security_id]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card] ADD CONSTRAINT [FK_card_card_groups_card_group_id] FOREIGN KEY ([card_group_id]) REFERENCES [dbo].[card_groups] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card] ADD CONSTRAINT [FK_card_floorplan_masked_area_registered_masked_area_id] FOREIGN KEY ([registered_masked_area_id]) REFERENCES [dbo].[floorplan_masked_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card] ADD CONSTRAINT [FK_card_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card] ADD CONSTRAINT [FK_card_mst_member_member_id] FOREIGN KEY ([member_id]) REFERENCES [dbo].[mst_member] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card] ADD CONSTRAINT [FK_card_visitor_visitor_id] FOREIGN KEY ([visitor_id]) REFERENCES [dbo].[visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table card_access_masked_areas
-- ----------------------------
ALTER TABLE [dbo].[card_access_masked_areas] ADD CONSTRAINT [FK_card_access_masked_areas_card_accesses_card_access_id] FOREIGN KEY ([card_access_id]) REFERENCES [dbo].[card_accesses] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_access_masked_areas] ADD CONSTRAINT [FK_card_access_masked_areas_floorplan_masked_area_masked_area_id] FOREIGN KEY ([masked_area_id]) REFERENCES [dbo].[floorplan_masked_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table card_access_time_groups
-- ----------------------------
ALTER TABLE [dbo].[card_access_time_groups] ADD CONSTRAINT [FK_card_access_time_groups_card_accesses_card_access_id] FOREIGN KEY ([card_access_id]) REFERENCES [dbo].[card_accesses] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_access_time_groups] ADD CONSTRAINT [FK_card_access_time_groups_time_group_time_group_id] FOREIGN KEY ([time_group_id]) REFERENCES [dbo].[time_group] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table card_accesses
-- ----------------------------
ALTER TABLE [dbo].[card_accesses] ADD CONSTRAINT [FK_card_accesses_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table card_card_accesses
-- ----------------------------
ALTER TABLE [dbo].[card_card_accesses] ADD CONSTRAINT [FK_card_card_accesses_card_accesses_card_access_id] FOREIGN KEY ([card_access_id]) REFERENCES [dbo].[card_accesses] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_card_accesses] ADD CONSTRAINT [FK_card_card_accesses_card_card_id] FOREIGN KEY ([card_id]) REFERENCES [dbo].[card] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table card_groups
-- ----------------------------
ALTER TABLE [dbo].[card_groups] ADD CONSTRAINT [FK_card_groups_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table card_record
-- ----------------------------
ALTER TABLE [dbo].[card_record] ADD CONSTRAINT [FK_card_record_card_CardId1] FOREIGN KEY ([CardId1]) REFERENCES [dbo].[card] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_record] ADD CONSTRAINT [FK_card_record_card_card_id] FOREIGN KEY ([card_id]) REFERENCES [dbo].[card] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_record] ADD CONSTRAINT [FK_card_record_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_record] ADD CONSTRAINT [FK_card_record_mst_member_MstMemberId] FOREIGN KEY ([MstMemberId]) REFERENCES [dbo].[mst_member] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_record] ADD CONSTRAINT [FK_card_record_mst_member_member_id] FOREIGN KEY ([member_id]) REFERENCES [dbo].[mst_member] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_record] ADD CONSTRAINT [FK_card_record_visitor_VisitorId1] FOREIGN KEY ([VisitorId1]) REFERENCES [dbo].[visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_record] ADD CONSTRAINT [FK_card_record_visitor_visitor_id] FOREIGN KEY ([visitor_id]) REFERENCES [dbo].[visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_record] ADD CONSTRAINT [FK_card_record_mst_security_MstSecurityId] FOREIGN KEY ([MstSecurityId]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table card_swap_transaction
-- ----------------------------
ALTER TABLE [dbo].[card_swap_transaction] ADD CONSTRAINT [FK_card_swap_transaction_visitor_visitor_id] FOREIGN KEY ([visitor_id]) REFERENCES [dbo].[visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_swap_transaction] ADD CONSTRAINT [FK_card_swap_transaction_card_from_card_id] FOREIGN KEY ([from_card_id]) REFERENCES [dbo].[card] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_swap_transaction] ADD CONSTRAINT [FK_card_swap_transaction_card_to_card_id] FOREIGN KEY ([to_card_id]) REFERENCES [dbo].[card] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_swap_transaction] ADD CONSTRAINT [FK_card_swap_transaction_floorplan_masked_area_masked_area_id] FOREIGN KEY ([masked_area_id]) REFERENCES [dbo].[floorplan_masked_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_swap_transaction] ADD CONSTRAINT [FK_card_swap_transaction_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[card_swap_transaction] ADD CONSTRAINT [FK_card_swap_transaction_trx_visitor_trx_visitor_id] FOREIGN KEY ([trx_visitor_id]) REFERENCES [dbo].[trx_visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table evacuation_assembly_points
-- ----------------------------
ALTER TABLE [dbo].[evacuation_assembly_points] ADD CONSTRAINT [FK_evacuation_assembly_points_floorplan_masked_area_floorplan_masked_area_id] FOREIGN KEY ([floorplan_masked_area_id]) REFERENCES [dbo].[floorplan_masked_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[evacuation_assembly_points] ADD CONSTRAINT [FK_evacuation_assembly_points_mst_floor_floor_id] FOREIGN KEY ([floor_id]) REFERENCES [dbo].[mst_floor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[evacuation_assembly_points] ADD CONSTRAINT [FK_evacuation_assembly_points_mst_floorplan_floorplan_id] FOREIGN KEY ([floorplan_id]) REFERENCES [dbo].[mst_floorplan] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table evacuation_transactions
-- ----------------------------
ALTER TABLE [dbo].[evacuation_transactions] ADD CONSTRAINT [FK_evacuation_transactions_card_card_id] FOREIGN KEY ([card_id]) REFERENCES [dbo].[card] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[evacuation_transactions] ADD CONSTRAINT [FK_evacuation_transactions_evacuation_alerts_evacuation_alert_id] FOREIGN KEY ([evacuation_alert_id]) REFERENCES [dbo].[evacuation_alerts] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[evacuation_transactions] ADD CONSTRAINT [FK_evacuation_transactions_evacuation_assembly_points_evacuation_assembly_point_id] FOREIGN KEY ([evacuation_assembly_point_id]) REFERENCES [dbo].[evacuation_assembly_points] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[evacuation_transactions] ADD CONSTRAINT [FK_evacuation_transactions_mst_member_member_id] FOREIGN KEY ([member_id]) REFERENCES [dbo].[mst_member] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[evacuation_transactions] ADD CONSTRAINT [FK_evacuation_transactions_mst_security_security_id] FOREIGN KEY ([security_id]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[evacuation_transactions] ADD CONSTRAINT [FK_evacuation_transactions_visitor_visitor_id] FOREIGN KEY ([visitor_id]) REFERENCES [dbo].[visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table floorplan_device
-- ----------------------------
ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [FK_floorplan_device_floorplan_masked_area_FloorplanMaskedAreaId1] FOREIGN KEY ([FloorplanMaskedAreaId1]) REFERENCES [dbo].[floorplan_masked_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [FK_floorplan_device_floorplan_masked_area_floorplan_masked_area_id] FOREIGN KEY ([floorplan_masked_area_id]) REFERENCES [dbo].[floorplan_masked_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [FK_floorplan_device_mst_access_cctv_MstAccessCctvId] FOREIGN KEY ([MstAccessCctvId]) REFERENCES [dbo].[mst_access_cctv] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [FK_floorplan_device_mst_access_cctv_access_cctv_id] FOREIGN KEY ([access_cctv_id]) REFERENCES [dbo].[mst_access_cctv] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [FK_floorplan_device_mst_access_control_MstAccessControlId] FOREIGN KEY ([MstAccessControlId]) REFERENCES [dbo].[mst_access_control] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [FK_floorplan_device_mst_access_control_access_control_id] FOREIGN KEY ([access_control_id]) REFERENCES [dbo].[mst_access_control] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [FK_floorplan_device_mst_application_MstApplicationId] FOREIGN KEY ([MstApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [FK_floorplan_device_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [FK_floorplan_device_mst_ble_reader_MstBleReaderId] FOREIGN KEY ([MstBleReaderId]) REFERENCES [dbo].[mst_ble_reader] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [FK_floorplan_device_mst_ble_reader_ble_reader_id] FOREIGN KEY ([ble_reader_id]) REFERENCES [dbo].[mst_ble_reader] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_device] ADD CONSTRAINT [FK_floorplan_device_mst_floorplan_floorplan_id] FOREIGN KEY ([floorplan_id]) REFERENCES [dbo].[mst_floorplan] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table floorplan_masked_area
-- ----------------------------
ALTER TABLE [dbo].[floorplan_masked_area] ADD CONSTRAINT [FK_floorplan_masked_area_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_masked_area] ADD CONSTRAINT [FK_floorplan_masked_area_mst_floor_MstFloorId] FOREIGN KEY ([MstFloorId]) REFERENCES [dbo].[mst_floor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_masked_area] ADD CONSTRAINT [FK_floorplan_masked_area_mst_floor_floor_id] FOREIGN KEY ([floor_id]) REFERENCES [dbo].[mst_floor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[floorplan_masked_area] ADD CONSTRAINT [FK_floorplan_masked_area_mst_floorplan_floorplan_id] FOREIGN KEY ([floorplan_id]) REFERENCES [dbo].[mst_floorplan] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table geofence
-- ----------------------------
ALTER TABLE [dbo].[geofence] ADD CONSTRAINT [FK_geofence_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[geofence] ADD CONSTRAINT [FK_geofence_mst_floor_floor_id] FOREIGN KEY ([floor_id]) REFERENCES [dbo].[mst_floor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[geofence] ADD CONSTRAINT [FK_geofence_mst_floorplan_floorplan_id] FOREIGN KEY ([floorplan_id]) REFERENCES [dbo].[mst_floorplan] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table group_building_access
-- ----------------------------
ALTER TABLE [dbo].[group_building_access] ADD CONSTRAINT [FK_group_building_access_mst_building_building_id] FOREIGN KEY ([building_id]) REFERENCES [dbo].[mst_building] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[group_building_access] ADD CONSTRAINT [FK_group_building_access_user_group_group_id] FOREIGN KEY ([group_id]) REFERENCES [dbo].[user_group] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table monitoring_config
-- ----------------------------
ALTER TABLE [dbo].[monitoring_config] ADD CONSTRAINT [FK_monitoring_config_mst_building_building_id] FOREIGN KEY ([building_id]) REFERENCES [dbo].[mst_building] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[monitoring_config] ADD CONSTRAINT [FK_monitoring_config_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table monitoring_config_building_access
-- ----------------------------
ALTER TABLE [dbo].[monitoring_config_building_access] ADD CONSTRAINT [FK_monitoring_config_building_access_monitoring_config_monitoring_config_id] FOREIGN KEY ([monitoring_config_id]) REFERENCES [dbo].[monitoring_config] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[monitoring_config_building_access] ADD CONSTRAINT [FK_monitoring_config_building_access_mst_building_building_id] FOREIGN KEY ([building_id]) REFERENCES [dbo].[mst_building] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_access_cctv
-- ----------------------------
ALTER TABLE [dbo].[mst_access_cctv] ADD CONSTRAINT [FK_mst_access_cctv_mst_application_MstApplicationId] FOREIGN KEY ([MstApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_access_cctv] ADD CONSTRAINT [FK_mst_access_cctv_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_access_cctv] ADD CONSTRAINT [FK_mst_access_cctv_mst_integration_integration_id] FOREIGN KEY ([integration_id]) REFERENCES [dbo].[mst_integration] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_access_control
-- ----------------------------
ALTER TABLE [dbo].[mst_access_control] ADD CONSTRAINT [FK_mst_access_control_mst_application_MstApplicationId] FOREIGN KEY ([MstApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_access_control] ADD CONSTRAINT [FK_mst_access_control_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_access_control] ADD CONSTRAINT [FK_mst_access_control_mst_brand_controller_brand_id] FOREIGN KEY ([controller_brand_id]) REFERENCES [dbo].[mst_brand] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_access_control] ADD CONSTRAINT [FK_mst_access_control_mst_integration_integration_id] FOREIGN KEY ([integration_id]) REFERENCES [dbo].[mst_integration] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_ble_reader
-- ----------------------------
ALTER TABLE [dbo].[mst_ble_reader] ADD CONSTRAINT [FK_mst_ble_reader_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_ble_reader] ADD CONSTRAINT [FK_mst_ble_reader_mst_brand_brand_id] FOREIGN KEY ([brand_id]) REFERENCES [dbo].[mst_brand] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_brand
-- ----------------------------
ALTER TABLE [dbo].[mst_brand] ADD CONSTRAINT [FK_mst_brand_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_building
-- ----------------------------
ALTER TABLE [dbo].[mst_building] ADD CONSTRAINT [FK_mst_building_mst_application_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_building] ADD CONSTRAINT [FK_mst_building_mst_application_MstApplicationId] FOREIGN KEY ([MstApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_department
-- ----------------------------
ALTER TABLE [dbo].[mst_department] ADD CONSTRAINT [FK_mst_department_mst_application_MstApplicationId] FOREIGN KEY ([MstApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_department] ADD CONSTRAINT [FK_mst_department_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_district
-- ----------------------------
ALTER TABLE [dbo].[mst_district] ADD CONSTRAINT [FK_mst_district_mst_application_MstApplicationId] FOREIGN KEY ([MstApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_district] ADD CONSTRAINT [FK_mst_district_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_engine
-- ----------------------------
ALTER TABLE [dbo].[mst_engine] ADD CONSTRAINT [FK_mst_engine_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_floor
-- ----------------------------
ALTER TABLE [dbo].[mst_floor] ADD CONSTRAINT [FK_mst_floor_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_floor] ADD CONSTRAINT [FK_mst_floor_mst_building_building_id] FOREIGN KEY ([building_id]) REFERENCES [dbo].[mst_building] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_floorplan
-- ----------------------------
ALTER TABLE [dbo].[mst_floorplan] ADD CONSTRAINT [FK_mst_floorplan_mst_engine_engine_id] FOREIGN KEY ([engine_id]) REFERENCES [dbo].[mst_engine] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_floorplan] ADD CONSTRAINT [FK_mst_floorplan_mst_application_MstApplicationId] FOREIGN KEY ([MstApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_floorplan] ADD CONSTRAINT [FK_mst_floorplan_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_floorplan] ADD CONSTRAINT [FK_mst_floorplan_mst_floor_MstFloorId] FOREIGN KEY ([MstFloorId]) REFERENCES [dbo].[mst_floor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_floorplan] ADD CONSTRAINT [FK_mst_floorplan_mst_floor_floor_id] FOREIGN KEY ([floor_id]) REFERENCES [dbo].[mst_floor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_integration
-- ----------------------------
ALTER TABLE [dbo].[mst_integration] ADD CONSTRAINT [FK_mst_integration_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_integration] ADD CONSTRAINT [FK_mst_integration_mst_brand_brand_id] FOREIGN KEY ([brand_id]) REFERENCES [dbo].[mst_brand] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_member
-- ----------------------------
ALTER TABLE [dbo].[mst_member] ADD CONSTRAINT [FK_mst_member_mst_application_MstApplicationId] FOREIGN KEY ([MstApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_member] ADD CONSTRAINT [FK_mst_member_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_member] ADD CONSTRAINT [FK_mst_member_mst_department_MstDepartmentId] FOREIGN KEY ([MstDepartmentId]) REFERENCES [dbo].[mst_department] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_member] ADD CONSTRAINT [FK_mst_member_mst_department_department_id] FOREIGN KEY ([department_id]) REFERENCES [dbo].[mst_department] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_member] ADD CONSTRAINT [FK_mst_member_mst_district_MstDistrictId] FOREIGN KEY ([MstDistrictId]) REFERENCES [dbo].[mst_district] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_member] ADD CONSTRAINT [FK_mst_member_mst_district_district_id] FOREIGN KEY ([district_id]) REFERENCES [dbo].[mst_district] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_member] ADD CONSTRAINT [FK_mst_member_mst_organization_MstOrganizationId] FOREIGN KEY ([MstOrganizationId]) REFERENCES [dbo].[mst_organization] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_member] ADD CONSTRAINT [FK_mst_member_mst_organization_organization_id] FOREIGN KEY ([organization_id]) REFERENCES [dbo].[mst_organization] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_organization
-- ----------------------------
ALTER TABLE [dbo].[mst_organization] ADD CONSTRAINT [FK_mst_organization_mst_application_MstApplicationId] FOREIGN KEY ([MstApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_organization] ADD CONSTRAINT [FK_mst_organization_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table mst_security
-- ----------------------------
ALTER TABLE [dbo].[mst_security] ADD CONSTRAINT [FK_mst_security_mst_security_security_head_1] FOREIGN KEY ([security_head_1]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_security] ADD CONSTRAINT [FK_mst_security_mst_security_security_head_2] FOREIGN KEY ([security_head_2]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_security] ADD CONSTRAINT [FK_mst_security_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_security] ADD CONSTRAINT [FK_mst_security_mst_department_department_id] FOREIGN KEY ([department_id]) REFERENCES [dbo].[mst_department] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_security] ADD CONSTRAINT [FK_mst_security_mst_district_district_id] FOREIGN KEY ([district_id]) REFERENCES [dbo].[mst_district] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[mst_security] ADD CONSTRAINT [FK_mst_security_mst_organization_organization_id] FOREIGN KEY ([organization_id]) REFERENCES [dbo].[mst_organization] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table overpopulating
-- ----------------------------
ALTER TABLE [dbo].[overpopulating] ADD CONSTRAINT [FK_overpopulating_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[overpopulating] ADD CONSTRAINT [FK_overpopulating_mst_floor_floor_id] FOREIGN KEY ([floor_id]) REFERENCES [dbo].[mst_floor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[overpopulating] ADD CONSTRAINT [FK_overpopulating_mst_floorplan_floorplan_id] FOREIGN KEY ([floorplan_id]) REFERENCES [dbo].[mst_floorplan] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table patrol_area
-- ----------------------------
ALTER TABLE [dbo].[patrol_area] ADD CONSTRAINT [FK_patrol_area_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_area] ADD CONSTRAINT [FK_patrol_area_mst_floor_floor_id] FOREIGN KEY ([floor_id]) REFERENCES [dbo].[mst_floor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_area] ADD CONSTRAINT [FK_patrol_area_mst_floorplan_floorplan_id] FOREIGN KEY ([floorplan_id]) REFERENCES [dbo].[mst_floorplan] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table patrol_assignment
-- ----------------------------
ALTER TABLE [dbo].[patrol_assignment] ADD CONSTRAINT [FK_patrol_assignment_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_assignment] ADD CONSTRAINT [FK_patrol_assignment_patrol_route_patrol_route_id] FOREIGN KEY ([patrol_route_id]) REFERENCES [dbo].[patrol_route] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_assignment] ADD CONSTRAINT [FK_patrol_assignment_time_group_time_group_id] FOREIGN KEY ([time_group_id]) REFERENCES [dbo].[time_group] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table patrol_assignment_security
-- ----------------------------
ALTER TABLE [dbo].[patrol_assignment_security] ADD CONSTRAINT [FK_patrol_assignment_security_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_assignment_security] ADD CONSTRAINT [FK_patrol_assignment_security_mst_security_security_id] FOREIGN KEY ([security_id]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_assignment_security] ADD CONSTRAINT [FK_patrol_assignment_security_patrol_assignment_patrol_assignment_id] FOREIGN KEY ([patrol_assignment_id]) REFERENCES [dbo].[patrol_assignment] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table patrol_case
-- ----------------------------
ALTER TABLE [dbo].[patrol_case] ADD CONSTRAINT [FK_patrol_case_mst_security_approved_by_head_1_id] FOREIGN KEY ([approved_by_head_1_id]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_case] ADD CONSTRAINT [FK_patrol_case_mst_security_approved_by_head_2_id] FOREIGN KEY ([approved_by_head_2_id]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_case] ADD CONSTRAINT [FK_patrol_case_mst_security_security_head_1] FOREIGN KEY ([security_head_1]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_case] ADD CONSTRAINT [FK_patrol_case_mst_security_security_head_2] FOREIGN KEY ([security_head_2]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_case] ADD CONSTRAINT [FK_patrol_case_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_case] ADD CONSTRAINT [FK_patrol_case_mst_security_security_id] FOREIGN KEY ([security_id]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_case] ADD CONSTRAINT [FK_patrol_case_patrol_assignment_patrol_assignment_id] FOREIGN KEY ([patrol_assignment_id]) REFERENCES [dbo].[patrol_assignment] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_case] ADD CONSTRAINT [FK_patrol_case_patrol_route_patrol_route_id] FOREIGN KEY ([patrol_route_id]) REFERENCES [dbo].[patrol_route] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_case] ADD CONSTRAINT [FK_patrol_case_patrol_session_patrol_session_id] FOREIGN KEY ([patrol_session_id]) REFERENCES [dbo].[patrol_session] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table patrol_case_attachment
-- ----------------------------
ALTER TABLE [dbo].[patrol_case_attachment] ADD CONSTRAINT [FK_patrol_case_attachment_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_case_attachment] ADD CONSTRAINT [FK_patrol_case_attachment_patrol_case_patrol_case_id] FOREIGN KEY ([patrol_case_id]) REFERENCES [dbo].[patrol_case] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table patrol_checkpoint_log
-- ----------------------------
ALTER TABLE [dbo].[patrol_checkpoint_log] ADD CONSTRAINT [FK_patrol_checkpoint_log_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_checkpoint_log] ADD CONSTRAINT [FK_patrol_checkpoint_log_patrol_area_patrol_area_id] FOREIGN KEY ([patrol_area_id]) REFERENCES [dbo].[patrol_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_checkpoint_log] ADD CONSTRAINT [FK_patrol_checkpoint_log_patrol_session_patrol_session_id] FOREIGN KEY ([patrol_session_id]) REFERENCES [dbo].[patrol_session] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table patrol_route
-- ----------------------------
ALTER TABLE [dbo].[patrol_route] ADD CONSTRAINT [FK_patrol_route_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table patrol_route_areas
-- ----------------------------
ALTER TABLE [dbo].[patrol_route_areas] ADD CONSTRAINT [FK_patrol_route_areas_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_route_areas] ADD CONSTRAINT [FK_patrol_route_areas_patrol_area_patrol_area_id] FOREIGN KEY ([patrol_area_id]) REFERENCES [dbo].[patrol_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_route_areas] ADD CONSTRAINT [FK_patrol_route_areas_patrol_route_patrol_route_id] FOREIGN KEY ([patrol_route_id]) REFERENCES [dbo].[patrol_route] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table patrol_session
-- ----------------------------
ALTER TABLE [dbo].[patrol_session] ADD CONSTRAINT [FK_patrol_session_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_session] ADD CONSTRAINT [FK_patrol_session_mst_security_security_id] FOREIGN KEY ([security_id]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_session] ADD CONSTRAINT [FK_patrol_session_patrol_assignment_patrol_assignment_id] FOREIGN KEY ([patrol_assignment_id]) REFERENCES [dbo].[patrol_assignment] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_session] ADD CONSTRAINT [FK_patrol_session_patrol_route_patrol_route_id] FOREIGN KEY ([patrol_route_id]) REFERENCES [dbo].[patrol_route] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[patrol_session] ADD CONSTRAINT [FK_patrol_session_time_group_time_group_id] FOREIGN KEY ([time_group_id]) REFERENCES [dbo].[time_group] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table refresh_token
-- ----------------------------
ALTER TABLE [dbo].[refresh_token] ADD CONSTRAINT [FK_refresh_token_user_user_id] FOREIGN KEY ([user_id]) REFERENCES [dbo].[user] ([id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table security_group
-- ----------------------------
ALTER TABLE [dbo].[security_group] ADD CONSTRAINT [FK_security_group_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table security_group_member
-- ----------------------------
ALTER TABLE [dbo].[security_group_member] ADD CONSTRAINT [FK_security_group_member_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[security_group_member] ADD CONSTRAINT [FK_security_group_member_mst_security_security_id] FOREIGN KEY ([security_id]) REFERENCES [dbo].[mst_security] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[security_group_member] ADD CONSTRAINT [FK_security_group_member_security_group_security_group_id] FOREIGN KEY ([security_group_id]) REFERENCES [dbo].[security_group] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table stay_on_area
-- ----------------------------
ALTER TABLE [dbo].[stay_on_area] ADD CONSTRAINT [FK_stay_on_area_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[stay_on_area] ADD CONSTRAINT [FK_stay_on_area_mst_floor_floor_id] FOREIGN KEY ([floor_id]) REFERENCES [dbo].[mst_floor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[stay_on_area] ADD CONSTRAINT [FK_stay_on_area_mst_floorplan_floorplan_id] FOREIGN KEY ([floorplan_id]) REFERENCES [dbo].[mst_floorplan] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table time_block
-- ----------------------------
ALTER TABLE [dbo].[time_block] ADD CONSTRAINT [FK_time_block_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[time_block] ADD CONSTRAINT [FK_time_block_time_group_time_group_id] FOREIGN KEY ([time_group_id]) REFERENCES [dbo].[time_group] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table time_group
-- ----------------------------
ALTER TABLE [dbo].[time_group] ADD CONSTRAINT [FK_time_group_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table tracking_report_presets
-- ----------------------------
ALTER TABLE [dbo].[tracking_report_presets] ADD CONSTRAINT [FK_tracking_report_presets_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table tracking_transaction
-- ----------------------------
ALTER TABLE [dbo].[tracking_transaction] ADD CONSTRAINT [FK_tracking_transaction_card_card_id] FOREIGN KEY ([card_id]) REFERENCES [dbo].[card] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[tracking_transaction] ADD CONSTRAINT [FK_tracking_transaction_floorplan_masked_area_FloorplanMaskedAreaId1] FOREIGN KEY ([FloorplanMaskedAreaId1]) REFERENCES [dbo].[floorplan_masked_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[tracking_transaction] ADD CONSTRAINT [FK_tracking_transaction_floorplan_masked_area_floorplan_masked_area_id] FOREIGN KEY ([floorplan_masked_area_id]) REFERENCES [dbo].[floorplan_masked_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[tracking_transaction] ADD CONSTRAINT [FK_tracking_transaction_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[tracking_transaction] ADD CONSTRAINT [FK_tracking_transaction_mst_ble_reader_MstBleReaderId] FOREIGN KEY ([MstBleReaderId]) REFERENCES [dbo].[mst_ble_reader] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[tracking_transaction] ADD CONSTRAINT [FK_tracking_transaction_mst_ble_reader_reader_id] FOREIGN KEY ([reader_id]) REFERENCES [dbo].[mst_ble_reader] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[tracking_transaction] ADD CONSTRAINT [FK_tracking_transaction_mst_member_member_id] FOREIGN KEY ([member_id]) REFERENCES [dbo].[mst_member] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[tracking_transaction] ADD CONSTRAINT [FK_tracking_transaction_visitor_visitor_id] FOREIGN KEY ([visitor_id]) REFERENCES [dbo].[visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table trx_visitor
-- ----------------------------
ALTER TABLE [dbo].[trx_visitor] ADD CONSTRAINT [FK_trx_visitor_visitor_VisitorId1] FOREIGN KEY ([VisitorId1]) REFERENCES [dbo].[visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[trx_visitor] ADD CONSTRAINT [FK_trx_visitor_visitor_visitor_id] FOREIGN KEY ([visitor_id]) REFERENCES [dbo].[visitor] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[trx_visitor] ADD CONSTRAINT [FK_trx_visitor_floorplan_masked_area_masked_area_id] FOREIGN KEY ([masked_area_id]) REFERENCES [dbo].[floorplan_masked_area] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[trx_visitor] ADD CONSTRAINT [FK_trx_visitor_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[trx_visitor] ADD CONSTRAINT [FK_trx_visitor_mst_member_purpose_person_id] FOREIGN KEY ([purpose_person_id]) REFERENCES [dbo].[mst_member] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table user
-- ----------------------------
ALTER TABLE [dbo].[user] ADD CONSTRAINT [FK_user_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[user] ADD CONSTRAINT [FK_user_user_group_UserGroupId] FOREIGN KEY ([UserGroupId]) REFERENCES [dbo].[user_group] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[user] ADD CONSTRAINT [FK_user_user_group_group_id] FOREIGN KEY ([group_id]) REFERENCES [dbo].[user_group] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table user_building_access
-- ----------------------------
ALTER TABLE [dbo].[user_building_access] ADD CONSTRAINT [FK_user_building_access_mst_building_building_id] FOREIGN KEY ([building_id]) REFERENCES [dbo].[mst_building] ([id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[user_building_access] ADD CONSTRAINT [FK_user_building_access_user_user_id] FOREIGN KEY ([user_id]) REFERENCES [dbo].[user] ([id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table user_group
-- ----------------------------
ALTER TABLE [dbo].[user_group] ADD CONSTRAINT [FK_user_group_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table visitor
-- ----------------------------
ALTER TABLE [dbo].[visitor] ADD CONSTRAINT [FK_visitor_mst_application_MstApplicationId] FOREIGN KEY ([MstApplicationId]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[visitor] ADD CONSTRAINT [FK_visitor_mst_application_application_id] FOREIGN KEY ([application_id]) REFERENCES [dbo].[mst_application] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


";
    }
}
