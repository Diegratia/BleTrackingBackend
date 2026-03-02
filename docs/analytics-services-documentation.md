# Analytics Services Documentation

**Service:** Analytics (Port: 5031)
**Last Updated:** 2025-02-08
**Version:** 1.0

---

## Overview

Analytics service provides comprehensive tracking and alarm analytics for BLE tracking system. Includes visitor session tracking, alarm incident analytics, daily summaries, peak hours analysis, and more.

---

## Table of Contents

1. [Tracking Session Service](#tracking-session-service)
2. [Alarm Analytics Incident Service](#alarm-analytics-incident-service)
3. [Tracking Summary Service](#tracking-summary-service)
4. [Dashboard Service](#dashboard-service)
5. [Tracking Report Preset Service](#tracking-report-preset-service)
6. [Data Models & DTOs](#data-models--dtos)

---

## Tracking Session Service

### Service: `TrackingSessionService`

### Repository: `TrackingSessionRepository`

**Purpose:** Track visitor/member sessions in areas with dwell time analysis and incident integration.

---

### Endpoints

#### 1. Get Visitor Sessions (Grouped by Person)

**Endpoint:** `POST /api/TrackingSession/visitor-session`

**Authorization:** `[MinLevel(LevelPriority.PrimaryAdmin)]`

**Description:** Get visitor/member sessions grouped by person with optional incident integration and visual paths.

---

### Request Parameters

#### Body Parameters (JSON)

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `type` | `string` | `"visitor"` | Person type filter: `"visitor"` \| `"member"` \| `"security"` |
| `visitorId` | `Guid?` | `null` | Filter by specific visitor ID |
| `memberId` | `Guid?` | `null` | Filter by specific member ID |
| `buildingId` | `Guid?` | `null` | Filter by building ID |
| `floorId` | `Guid?` | `null` | Filter by floor ID |
| `areaId` | `Guid?` | `null` | Filter by area ID |
| `timeRange` | `string?` | `null` | Time range: `"daily"` \| `"weekly"` \| `"monthly"` |
| `from` | `DateTime?` | `null` | Start time (overrides timeRange) |
| `to` | `DateTime?` | `null` | End time (overrides timeRange) |
| `hasIncident` | `bool?` | `null` | Filter: `true` = only with incidents, `false` = only without, `null` = all |
| `includeIncident` | `bool` | `true` | Include full incident object in sessions |
| `includeSummary` | `bool` | `false` | Include summary statistics |
| `includeVisualPaths` | `bool` | `false` | Include visual paths for floorplan visualization |
| `timezone` | `string?` | `"UTC"` | Timezone for response datetimes (e.g., `"WIB"`, `"UTC+7"`) |

#### Query Parameters (Optional)

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `includeVisualPaths` | `bool` | `false` | Override for visual paths (backward compat) |
| `includeSummary` | `bool` | `false` | Override for summary (backward compat) |
| `includeIncident` | `bool` | `true` | Override for incident (backward compat) |
| `type` | `string` | - | Override for person type (backward compat) |
| `hasIncident` | `bool?` | `null` | Override for incident filter (backward compat) |

---

### Response Structure

```json
{
  "code": 200,
  "message": "Visitor sessions retrieved successfully",
  "data": {
    "persons": [
      {
        "personId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "personName": "John Doe",
        "personType": "Visitor",
        "identityId": "K123456",
        "cardId": "c1d2e3f4-a5b6-7890-1234-567890abcdef",
        "cardNumber": "K123456",

        "visitorId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "visitorName": "John Doe",
        "memberId": null,
        "memberName": null,

        "totalSessions": 3,
        "totalDurationMinutes": 84,
        "totalDurationFormatted": "1 hour 24 min",
        "totalIncidents": 1,
        "restrictedAreasVisited": 1,

        "areasVisited": ["Lobby", "Banking Hall", "Server Room"],
        "firstAreaEntered": "Lobby",
        "lastAreaExited": "Server Room",
        "currentArea": null,

        "sessions": [
          {
            "areaId": "f1e2d3c4-b5a6-9876-5432-10fedcba9876",
            "areaName": "Lobby",
            "buildingName": "Main Building",
            "floorName": "Floor 1",
            "floorplanId": "fp1-id",
            "floorplanName": "Floorplan 1",
            "floorplanImage": "https://example.com/images/floor1.png",

            "enterTime": "2025-02-08T08:30:15",
            "exitTime": "2025-02-08T08:35:20",
            "durationMinutes": 5,
            "durationFormatted": "5 min",
            "sessionStatus": "completed",

            "hasIncident": false,
            "incident": null
          },
          {
            "areaId": "area-server-room-id",
            "areaName": "Server Room",
            "buildingName": "Main Building",
            "floorName": "Floor 2",
            "floorplanId": "fp2-id",
            "floorplanName": "Floorplan 2",
            "floorplanImage": "https://example.com/images/floor2.png",

            "enterTime": "2025-02-08T09:16:00",
            "exitTime": "2025-02-08T09:45:00",
            "durationMinutes": 29,
            "durationFormatted": "29 min",
            "sessionStatus": "completed",

            "hasIncident": true,
            "incident": {
              "alarmTriggerId": "d4e5f6g7-h8i9-3456-7890-1bcdef234567",
              "alarmColor": "red",
              "alarmStatus": "Active",
              "actionStatus": "Investigated",
              "isActive": true,

              "triggerTime": "2025-02-08T09:16:30",
              "acknowledgedAt": "2025-02-08T09:16:45",
              "acknowledgedBy": "Admin User",
              "dispatchedAt": "2025-02-08T09:17:00",
              "dispatchedBy": "Security Guard A",
              "arrivedAt": "2025-02-08T09:19:00",
              "arrivedBy": "Security Guard A",
              "investigatedAt": "2025-02-08T09:25:00",
              "investigatedBy": "Security Guard A",
              "investigationResult": "Unauthorized access",
              "doneAt": "2025-02-08T09:45:00",
              "doneBy": "Security Guard A",

              "securityId": "security-123-id",
              "securityName": "Security Guard A",

              "responseTimeSeconds": 15,
              "responseTimeFormatted": "15 sec",
              "resolutionTimeSeconds": 1710,
              "resolutionTimeFormatted": "28 min 30 sec",

              "timelineSummary": "ACK by Admin User → DISPATCHED by Security Guard A → ARRIVED → INVESTIGATED → DONE"
            }
          }
        ]
      }
    ],

    "summary": {
      "totalDurationMinutes": 5000,
      "firstDetection": "2025-02-08T08:00:00",
      "lastDetection": "2025-02-08T18:00:00",
      "areasVisited": ["Lobby", "Server Room", "Banking Hall"],
      "totalDetections": 150,
      "totalSessions": 50,
      "uniqueVisitors": 20,
      "uniqueMembers": 10
    },

    "visualPaths": {
      "floorplans": {
        "fp-floor-1-id": {
          "floorplanId": "fp-floor-1-id",
          "floorplanName": "Floor 1",
          "floorplanImage": "https://example.com/images/floor1.png",
          "points": [
            { "x": 100, "y": 200, "time": "2025-02-08T08:30:15", "area": "Lobby", "personName": "John Doe", "personId": "john-id" },
            { "x": 300, "y": 400, "time": "2025-02-08T09:16:00", "area": "Server Room", "personName": "John Doe", "personId": "john-id" }
          ]
        }
      }
    }
  }
}
```

---

### Request Examples

#### Example 1: Get All Visitor Sessions (Default)

```bash
POST /api/TrackingSession/visitor-session
Content-Type: application/json

{
  "type": "visitor",
  "includeIncident": true
}
```

**Response:** All visitor sessions from last 24 hours with incident data.

---

#### Example 2: Get Sessions for Specific Visitor

```bash
POST /api/TrackingSession/visitor-session
Content-Type: application/json

{
  "type": "visitor",
  "visitorId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "includeIncident": true,
  "includeSummary": true
}
```

**Response:** All sessions for specific visitor with summary.

---

#### Example 3: Filter Sessions with Incidents Only

```bash
POST /api/TrackingSession/visitor-session
Content-Type: application/json

{
  "type": "visitor",
  "hasIncident": true,
  "includeIncident": true,
  "timeRange": "weekly"
}
```

**Response:** Only sessions that have incidents within this week.

---

#### Example 4: Get Member Sessions with Visual Paths

```bash
POST /api/TrackingSession/visitor-session
Content-Type: application/json

{
  "type": "member",
  "memberId": "xxx-xxx-xxx",
  "includeVisualPaths": true,
  "timezone": "WIB"
}
```

**Response:** Member sessions with visual paths for floorplan visualization.

---

#### Example 5: Get Security Sessions

```bash
POST /api/TrackingSession/visitor-session
Content-Type: application/json

{
  "type": "security",
  "timeRange": "daily",
  "includeIncident": false
}
```

**Response:** All security personnel movement today (without incident data).

---

#### Example 6: Custom Time Range

```bash
POST /api/TrackingSession/visitor-session
Content-Type: application/json

{
  "type": "visitor",
  "from": "2025-02-01T00:00:00",
  "to": "2025-02-07T23:59:59",
  "buildingId": "b1-building-id",
  "includeSummary": true
}
```

**Response:** Visitor sessions in specific building within custom date range with summary.

---

### Time Range Values

| Value | Description | Range (WIB) |
|-------|-------------|--------------|
| `"daily"` | Today | 00:00 - Now (WIB) |
| `"weekly"` | This week | Monday 00:00 - Now (WIB) |
| `"monthly"` | This month | 1st 00:00 - Now (WIB) |
| `null` | Default | Last 24 hours |

**Override:** Use `from` and `to` parameters to override timeRange.

---

### Person Type Values

| Value | Description | SQL Filter |
|-------|-------------|------------|
| `"visitor"` | Visitors only | `v.id IS NOT NULL AND m.id IS NULL` |
| `"member"` | Members only | `m.id IS NOT NULL AND v.id IS NULL` |
| `"security"` | Security personnel | `EXISTS (SELECT 1 FROM mst_security s WHERE s.id = c.security_id)` |

---

### Incident Marker Object Structure

When `hasIncident: true` and `includeIncident: true`, each session includes:

```typescript
{
  "incident": {
    // Basic Info
    "alarmTriggerId": Guid,
    "alarmColor": string,          // "red" | "yellow" | "green"
    "alarmStatus": string,         // "Active" | "Done" | "Cancelled"
    "actionStatus": string,        // "Acknowledged" | "Dispatched" | "Arrived" | "Investigated" | "Done"
    "isActive": boolean,

    // Timestamps
    "triggerTime": DateTime,
    "acknowledgedAt": DateTime | null,
    "dispatchedAt": DateTime | null,
    "arrivedAt": DateTime | null,
    "investigatedAt": DateTime | null,
    "doneAt": DateTime | null,

    // Actors
    "acknowledgedBy": string | null,
    "dispatchedBy": string | null,
    "arrivedBy": string | null,
    "investigatedBy": string | null,
    "doneBy": string | null,

    // Security
    "securityId": Guid | null,
    "securityName": string | null,

    // Investigation
    "investigationResult": string | null,

    // Metrics
    "responseTimeSeconds": int,          // Trigger → Acknowledged
    "responseTimeFormatted": string,      // "15 sec"
    "resolutionTimeSeconds": int,         // Trigger → Done
    "resolutionTimeFormatted": string,    // "28 min 30 sec"

    // Summary
    "timelineSummary": string             // "ACK by Admin → DISPATCHED → ARRIVED → INVESTIGATED → DONE"
  }
}
```

---

### Visual Paths Structure

When `includeVisualPaths: true`, response includes:

```typescript
{
  "visualPaths": {
    "floorplans": {
      "<floorplan-guid>": {
        "floorplanId": Guid,
        "floorplanName": string,
        "floorplanImage": string (URL),
        "points": [
          {
            "x": float (coordinate),
            "y": float (coordinate),
            "time": DateTime (UTC),
            "area": string (area name),
            "personName": string,
            "personId": Guid
          }
        ]
      }
    }
  }
}
```

**Usage:** Render points on floorplan image for journey visualization.

---

### Summary Object Structure

When `includeSummary: true`, response includes:

```typescript
{
  "summary": {
    "totalDurationMinutes": int,
    "firstDetection": DateTime,
    "lastDetection": DateTime,
    "areasVisited": string[],
    "totalDetections": int,
    "totalSessions": int,
    "uniqueVisitors": int,
    "uniqueMembers": int
  }
}
```

---

### Session Status Values

| Value | Description |
|-------|-------------|
| `"active"` | Person currently in area (exitTime = null) |
| `"completed"` | Person has left area (exitTime != null) |

---

### Usage Patterns

#### Pattern 1: Journey Replay with Incident Markers

```javascript
// Frontend: Display journey with incident markers
POST /api/TrackingSession/visitor-session
{
  "visitorId": "xxx",
  "includeIncident": true,
  "from": "2025-02-08T00:00:00",
  "to": "2025-02-08T23:59:59"
}

// Render journey
data.persons.forEach(person => {
  console.log(`${person.personName}: ${person.totalSessions} sessions, ${person.totalIncidents} incidents`);

  person.sessions.forEach(session => {
    if (session.hasIncident) {
      console.log(`⚠️ Incident in ${session.areaName}: ${session.incident.timelineSummary}`);
      renderIncidentMarker(session.areaId, session.incident);
    }
    renderPath(session);
  });
});
```

---

#### Pattern 2: Floorplan Visualization with Paths

```javascript
// Frontend: Render paths on floorplan
POST /api/TrackingSession/visitor-session
{
  "type": "visitor",
  "includeVisualPaths": true,
  "floorplanId": "fp-floor-1-id"
}

// Get floorplan data
const floorplan = response.data.visualPaths.floorplans['fp-floor-1-id'];

// Load floorplan image
document.getElementById('floorplan-img').src = floorplan.floorplanImage;

// Render all points (faded)
floorplan.points.forEach(point => {
  renderPoint(point.x, point.y, { opacity: 0.3 });
});

// User clicks session → highlight path
onSessionClick(session) {
  const sessionPoints = floorplan.points.filter(p =>
    p.personId === session.personId &&
    p.time >= session.enterTime &&
    p.time <= session.exitTime
  );

  sessionPoints.forEach(p => renderPoint(p.x, p.y, { opacity: 1.0, color: 'red' }));
}
```

---

#### Pattern 3: Incident Investigation

```javascript
// Frontend: Filter sessions with incidents
POST /api/TrackingSession/visitor-session
{
  "hasIncident": true,
  "includeIncident": true,
  "timeRange": "weekly"
}

// Display incident summary
data.persons.forEach(person => {
  person.sessions.forEach(session => {
    if (session.hasIncident) {
      console.log(`🚨 ${person.personName} triggered alarm in ${session.areaName}`);
      console.log(`   Response time: ${session.incident.responseTimeFormatted}`);
      console.log(`   Resolution: ${session.incident.investigationResult}`);
      console.log(`   Timeline: ${session.incident.timelineSummary}`);
    }
  });
});
```

---

#### Pattern 4: Daily Report

```javascript
// Frontend: Daily summary report
POST /api/TrackingSession/visitor-session
{
  "type": "visitor",
  "timeRange": "daily",
  "includeSummary": true,
  "includeIncident": true
}

// Display summary
const summary = response.data.summary;
console.log(`Total sessions: ${summary.totalSessions}`);
console.log(`Unique visitors: ${summary.uniqueVisitors}`);
console.log(`Total duration: ${summary.totalDurationMinutes} minutes`);
console.log(`Areas visited: ${summary.areasVisited.join(', ')}`);
```

---

## Alarm Analytics Incident Service

### Service: `AlarmAnalyticsIncidentService`

### Repository: `AlarmAnalyticsIncidentRepository`

**Purpose:** Comprehensive alarm incident analytics including daily summaries, hourly patterns, area charts, and visitor summaries.

---

### Endpoints

#### 1. Get Daily Incident Summary

**Endpoint:** `POST /api/AlarmAnalytics/daily-summary`

**Description:** Get daily incident summary with statistics by alarm status.

---

#### 2. Get Hourly Incident Pattern

**Endpoint:** `POST /api/AlarmAnalytics/hourly-pattern`

**Description:** Get hourly incident distribution pattern analysis.

---

#### 3. Get Area Incident Chart

**Endpoint:** `POST /api/AlarmAnalytics/area-chart`

**Description:** Get incidents distribution by area for chart visualization.

---

#### 4. Get Visitor Incident Summary

**Endpoint:** `POST /api/AlarmAnalytics/visitor-summary`

**Description:** Get incident summary for specific visitors.

---

### Incident Workflow States

Alarm incidents go through the following workflow states:

1. **Triggered** - Alarm triggered by beacon
2. **Acknowledged** - Alarm acknowledged by admin
3. **Dispatched** - Security personnel dispatched to location
4. **Arrived** - Security personnel arrived at location
5. **Waiting** - Put in waiting queue (optional)
6. **Investigated** - Investigation started
7. **Done** - Incident resolved
8. **Cancelled** - Alarm cancelled (optional)
9. **Idle** - Marked as idle (optional)

---

### Alarm Status Enum

```csharp
public enum AlarmRecordStatus
{
    Triggered = 1,
    Done = 2,
    Cancelled = 3
}
```

---

### Action Status Enum

```csharp
public enum ActionStatus
{
    Acknowledged = 1,
    Dispatched = 2,
    Arrived = 3,
    Investigated = 4,
    Done = 5
}
```

---

## Tracking Summary Service

### Service: `TrackingSummaryService`

### Repository: `TrackingSummaryRepository`

**Purpose:** High-level tracking analytics summaries including peak hours, dwell time analysis, and area occupancy metrics.

---

### Endpoints

#### 1. Get Peak Hours by Area

**Endpoint:** `POST /api/TrackingSession/peak-hours-by-area`

**Description:** Get peak traffic hours by area analysis.

---

## Dashboard Service

### Service: `DashboardService`

**Purpose:** Aggregate dashboard data for analytics overview.

---

## Tracking Report Preset Service

### Service: `TrackingReportPresetService`

**Purpose:** Save and load predefined report configurations for quick access to frequently used analytics queries.

---

## Data Models & DTOs

### IncidentMarkerRead

**File:** `Shared/Shared.Contracts/Analytics/IncidentMarkerRead.cs`

```csharp
public class IncidentMarkerRead
{
    // Basic Info
    public Guid AlarmTriggerId { get; set; }
    public string? AlarmColor { get; set; }
    public string? AlarmStatus { get; set; }
    public string? ActionStatus { get; set; }
    public bool IsActive { get; set; }

    // Timestamps
    public DateTime TriggerTime { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public DateTime? InvestigatedAt { get; set; }
    public DateTime? DoneAt { get; set; }

    // Actors
    public string? AcknowledgedBy { get; set; }
    public string? DispatchedBy { get; set; }
    public string? ArrivedBy { get; set; }
    public string? InvestigatedBy { get; set; }
    public string? DoneBy { get; set; }

    // Security
    public Guid? SecurityId { get; set; }
    public string? SecurityName { get; set; }

    // Investigation
    public string? InvestigationResult { get; set; }

    // Metrics
    public int ResponseTimeSeconds { get; set; }
    public string ResponseTimeFormatted { get; set; }
    public int ResolutionTimeSeconds { get; set; }
    public string ResolutionTimeFormatted { get; set; }

    // Summary
    public string TimelineSummary { get; set; }
}
```

---

### PersonSessionItemRead

**File:** `Shared/Shared.Contracts/Analytics/PersonSessionItemRead.cs`

```csharp
public class PersonSessionItemRead
{
    // Area & Location Info
    public Guid? AreaId { get; set; }
    public string? AreaName { get; set; }
    public Guid? BuildingId { get; set; }
    public string? BuildingName { get; set; }
    public Guid? FloorId { get; set; }
    public string? FloorName { get; set; }
    public Guid? FloorplanId { get; set; }
    public string? FloorplanName { get; set; }
    public string? FloorplanImage { get; set; }

    // Session Timing
    public DateTime EnterTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public int? DurationMinutes { get; set; }
    public string? DurationFormatted { get; set; }
    public string SessionStatus { get; set; }

    // Incident Info
    public bool HasIncident { get; set; }
    public IncidentMarkerRead? Incident { get; set; }
}
```

**Note:** Does NOT include person info - use parent `PersonSessionsRead` for person details.

---

### PersonSessionsRead

**File:** `Shared/Shared.Contracts/Analytics/GroupedSessionsRead.cs`

```csharp
public class PersonSessionsRead
{
    // Person Info
    public Guid? PersonId { get; set; }
    public string? PersonName { get; set; }
    public string? PersonType { get; set; }
    public string? IdentityId { get; set; }
    public Guid? CardId { get; set; }
    public string? CardNumber { get; set; }
    public Guid? VisitorId { get; set; }
    public string? VisitorName { get; set; }
    public Guid? MemberId { get; set; }
    public string? MemberName { get; set; }

    // Summary
    public int TotalSessions { get; set; }
    public int TotalDurationMinutes { get; set; }
    public string TotalDurationFormatted { get; set; }
    public int TotalIncidents { get; set; }
    public int RestrictedAreasVisited { get; set; }
    public List<string> AreasVisited { get; set; }
    public string? FirstAreaEntered { get; set; }
    public string? LastAreaExited { get; set; }
    public string? CurrentArea { get; set; }

    // Sessions (minimal, without person info)
    public List<PersonSessionItemRead> Sessions { get; set; }
}
```

---

### GroupedSessionsResponse

**File:** `Shared/Shared.Contracts/Analytics/GroupedSessionsRead.cs`

```csharp
public class GroupedSessionsResponse
{
    public List<PersonSessionsRead> Persons { get; set; }
    public VisitorSessionSummaryRead? Summary { get; set; }
    public VisualPathsRead? VisualPaths { get; set; }
}
```

---

### VisitorSessionRead

**File:** `Shared/Shared.Contracts/Analytics/VisitorSessionRead.cs`

**Note:** This is the full session DTO used internally. When using grouped response, sessions are returned as `PersonSessionItemRead` (minimal version) to avoid redundancy.

---

### TrackingAnalyticsFilter

**File:** `Shared/Shared.Contracts/TrackingAnalyticsFilter.cs`

```csharp
public class TrackingAnalyticsFilter : BaseFilter
{
    // Time Filters
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? TimeRange { get; set; }

    // Entity Filters
    public Guid? BuildingId { get; set; }
    public Guid? FloorId { get; set; }
    public Guid? FloorplanId { get; set; }
    public Guid? AreaId { get; set; }
    public Guid? CardId { get; set; }
    public Guid? VisitorId { get; set; }
    public Guid? MemberId { get; set; }
    public Guid? ReaderId { get; set; }

    // String Filters
    public string? IdentityId { get; set; }
    public string? PersonType { get; set; }

    // New Filters
    public string Type { get; set; } = "visitor";
    public bool? HasIncident { get; set; }
    public bool IncludeSummary { get; set; } = false;
    public bool IncludeVisualPaths { get; set; } = false;
    public bool IncludeIncident { get; set; } = true;

    // Export Options
    public string? ReportTitle { get; set; }
    public string? ExportType { get; set; }
    public string? Timezone { get; set; }
}
```

---

## Common Response Format

All endpoints use standard `ApiResponse` format:

```json
{
  "code": 200,
  "message": "Success message",
  "data": { ... }
}
```

**Response Codes:**
- `200` - Success
- `404` - Not Found
- `400` - Bad Request
- `500` - Internal Server Error

---

## Timezone Handling

All datetimes are stored in UTC in the database. The `timezone` parameter converts responses to requested timezone.

**Supported Timezones:**
- `"UTC"` - Coordinated Universal Time (default)
- `"WIB"` - Western Indonesia Time (UTC+7)
- `"UTC+7"` - UTC+7
- `"Asia/Jakarta"` - Java/Indonesia timezone

---

## Performance Considerations

1. **Large Date Ranges:** Querying large date ranges (months) may return thousands of sessions. Use appropriate filters.
2. **Visual Paths:** Enabling `includeVisualPaths` adds significant data payload. Use only when needed.
3. **Incident Data:** Joining with alarm_triggers table adds query overhead. Filter by date range to optimize.
4. **Indexing:** Ensure indexes exist on:
   - `tracking_transaction_YYYYMMDD.trans_time`
   - `alarm_triggers.trigger_time`
   - `alarm_triggers.visitor_id`
   - `alarm_triggers.member_id`

---

## Error Handling

All errors are caught by global exception middleware and returned in standard format:

```json
{
  "code": 500,
  "message": "Internal server error: {error details}",
  "data": null
}
```

---

## Authorization

All Analytics endpoints require:

```csharp
[MinLevel(LevelPriority.PrimaryAdmin)]
```

**Minimum Role:** PrimaryAdmin (Level: 2)

**Role Hierarchy (from highest to lowest):**
1. System (Level: 0)
2. SuperAdmin (Level: 1)
3. PrimaryAdmin (Level: 2)
4. Primary (Level: 3)
5. Secondary (Level: 4)
6. UserCreated (Level: 5)

---

## Export Features

### Export to PDF

**Endpoint:** `GET /api/TrackingSession/export/pdf`

**Query Parameters:** All TrackingAnalyticsFilter parameters

**Response:** PDF file download

---

### Export to Excel

**Endpoint:** `GET /api/TrackingSession/export/excel`

**Query Parameters:** All TrackingAnalyticsFilter parameters

**Response:** Excel file download

---

## Quick Reference

### Common Filter Combinations

```json
// Today's visitor sessions with incidents
{
  "type": "visitor",
  "timeRange": "daily",
  "hasIncident": true
}

// This week's member sessions
{
  "type": "member",
  "timeRange": "weekly"
}

// Custom range with visual paths
{
  "type": "visitor",
  "from": "2025-02-01T00:00:00",
  "to": "2025-02-07T23:59:59",
  "includeVisualPaths": true,
  "includeSummary": true
}

// Specific person's journey
{
  "type": "visitor",
  "visitorId": "xxx-xxx-xxx",
  "includeIncident": true
}
```

---

## Troubleshooting

### Common Issues

**Issue:** Empty sessions array
**Solution:** Check if tracking transaction tables exist for the date range. Tables are named `tracking_transaction_YYYYMMDD`.

**Issue:** No incident data
**Solution:** Verify `alarm_triggers` table has records for the date range and person IDs.

**Issue:** Timezone incorrect
**Solution:** Ensure `timezone` parameter is set correctly (e.g., `"WIB"` for Indonesia).

**Issue:** Response too large
**Solution:** Disable `includeVisualPaths` and use date filters to reduce data volume.

---

## Related Services

- **AlarmTriggers Service** - Manages alarm triggers and actions
- **Card Service** - Manages visitor/member cards
- **Visitor Service** - Manages visitor records
- **MstMember Service** - Manages member records
- **FloorplanMaskedArea Service** - Manages floorplan areas
- **MstFloorplan Service** - Manages floorplan configurations

---

## Changelog

### Version 1.0 (2025-02-08)

- Initial documentation
- Added incident integration to tracking sessions
- Added grouped by person response format
- Added `PersonSessionItemRead` DTO to eliminate data redundancy
- Added support for `type`, `hasIncident`, `includeSummary`, `includeVisualPaths`, `includeIncident` filters
- Removed UserJourney service (redundant with enhanced tracking sessions)

---

## Support

For issues or questions, contact the development team or check the source code at:
- Repository: `Shared/Repositories/Repository/Analytics/TrackingSessionRepository.cs`
- Service: `Shared/BusinessLogic.Services/Implementation/Analytics/TrackingSessionService.cs`
- Controller: `Shared/Web.API.Controllers/Controllers/Analytics/TrackingAnalyticsController.cs`

---

**End of Documentation**
