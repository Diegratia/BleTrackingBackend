# Analytics Enhancement Plan - Bank Indonesia

**Created:** 2025-02-08
**Context:** BLE Tracking Backend Analytics Enhancement
**Priority:** Security-Focused User Journey Analytics

---

## 📊 Current Analytics Capabilities

### ✅ Alarm Analytics (Implemented)

**Endpoints:**
- `POST /api/AlarmAnalyticsIncident/daily` - Incidents per day
- `POST /api/AlarmAnalyticsIncident/status` - Breakdown by alarm status (Block, Help, WrongZone, etc.)
- `POST /api/AlarmAnalyticsIncident/visitor` - Top visitors involved in alarms
- `POST /api/AlarmAnalyticsIncident/building` - Incidents per building
- `POST /api/AlarmAnalyticsIncident/hourly` - 24-hour distribution with status breakdown
- `POST /api/AlarmAnalyticsIncident/area` - Area chart with time series

### ✅ Tracking Analytics (Implemented)

**Summary Endpoints:**
- `POST /api/TrackingAnalytics/area` - Unique cards per area
- `POST /api/TrackingAnalytics/daily` - Detection count per day
- `POST /api/TrackingAnalytics/reader` - Detection count per BLE reader
- `POST /api/TrackingAnalytics/visitor` - Top visitors by detection count
- `POST /api/TrackingAnalytics/building` - Detection count per building

**Advanced Endpoints:**
- `GET /api/TrackingAnalytics/movement/{cardId}` - Full movement path for specific card
- `POST /api/TrackingAnalytics/heatmap` - X,Y coordinate density
- `POST /api/TrackingAnalytics/latest-position` - Last known location per card
- `POST /api/TrackingAnalytics/area-accessed` - Permission breakdown (restrict vs non-restrict)
- `POST /api/TrackingAnalytics/visitor-session` - Dwell time, enter/exit, duration
- `POST /api/TrackingAnalytics/peak-hours-by-area` - Hourly visitor distribution per area
- `GET /api/TrackingAnalytics/export/pdf` - PDF report generation
- `GET /api/TrackingAnalytics/export/excel` - Excel report generation

### ✅ Visitor Session (Existing)

**What it provides:**
```csharp
VisitorSessionRead {
    PersonId: "John",
    AreaName: "Lobby",
    EnterTime: 10:00,
    ExitTime: 10:15,
    DurationInMinutes: 15
}

// If John moves areas, creates MULTIPLE sessions:
Session 1: John di Lobby (10:00-10:15)
Session 2: John di Banking Hall (10:20-10:45)
Session 3: John di Meeting Room (11:00-12:00)
```

**VisualPaths (included):**
- X,Y coordinates per floorplan
- Used for floorplan visualization
- Time-stamped movement points

**Limitation:**
- ❌ No "Common Paths" aggregation
- ❌ No "Next Area" prediction
- ❌ No journey-level analysis (multi-area sequences)

---

## 🎯 Feature Priorities for Bank Indonesia

### Context: Banking Facility

**Critical Requirements:**
1. **Security** - Detect unusual movement, unauthorized access
2. **Visitor Flow** - Optimize public/employee area transitions
3. **Resource Optimization** - Guard/staff placement, checkpoint efficiency

---

## 🔴 P0: Security-Focused User Journey

### 1. Common Paths Analysis

**Endpoint:** `POST /api/TrackingAnalytics/user-journey/common-paths`

**Purpose:** Discover most popular multi-area journeys

**Implementation:**
```csharp
public class CommonPathRead
{
    public string PathId { get; set; }                  // "lobby-banking-cafe"
    public List<string> AreaSequence { get; set; }       // ["Lobby", "Banking Hall", "Cafeteria"]
    public int JourneyCount { get; set; }                // How many visitors
    public double Percentage { get; set; }               // % of total journeys
    public double AvgDurationMinutes { get; set; }
    public List<string> EntryPoints { get; set; }
    public List<string> ExitPoints { get; set; }
}
```

**SQL Logic:**
```sql
-- Aggregate journeys by area sequence
WITH RankedSessions AS (
    SELECT
        PersonId,
        AreaName,
        EnterTime,
        ROW_NUMBER() OVER (PARTITION BY PersonId ORDER BY EnterTime) as SeqNum
    FROM VisitorSession
    WHERE EnterTime >= @From AND EnterTime <= @To
),
JourneyPaths AS (
    SELECT
        PersonId,
        STRING_AGG(AreaName, '→' ORDER BY EnterTime) as PathSequence,
        COUNT(*) as AreaCount,
        MIN(EnterTime) as JourneyStart,
        MAX(ExitTime) as JourneyEnd,
        DATEDIFF(minute, MIN(EnterTime), MAX(ExitTime)) as JourneyDuration
    FROM RankedSessions
    GROUP BY PersonId
    HAVING COUNT(*) >= 2  -- Multi-area journeys only
)
SELECT
    PathSequence,
    COUNT(*) as JourneyCount,
    COUNT(*) * 100.0 / SUM(COUNT(*)) OVER() as Percentage,
    AVG(JourneyDuration) as AvgDurationMinutes
FROM JourneyPaths
GROUP BY PathSequence
ORDER BY JourneyCount DESC
```

**Example Output:**
```json
{
  "data": [
    {
      "pathId": "lobby-security-banking",
      "areaSequence": ["Lobby", "Security Check", "Banking Hall"],
      "journeyCount": 1234,
      "percentage": 65.2,
      "avgDurationMinutes": 25.5,
      "entryPoints": ["Main Entrance"],
      "exitPoints": ["Banking Hall Exit"]
    }
  ]
}
```

**Use Cases for Bank Indonesia:**
- Identify natural flow - "65% visitors go Lobby → Security → Banking Hall"
- Detect anomalies - "Only 3 people went Lobby → Server Room" (suspicious)
- Optimize signage - "Many people lost at Security → Banking Hall transition"

---

### 2. Security Journey Validation

**Endpoint:** `POST /api/TrackingAnalytics/user-journey/security-check`

**Purpose:** Flag unusual or unauthorized journeys

**Returns:**
```csharp
public class SecurityJourneyCheckRead
{
    public string VisitorId { get; set; }
    public string VisitorName { get; set; }
    public List<string> PathTaken { get; set; }
    public List<SecurityViolation> Violations { get; set; }
    public string RiskLevel { get; set; }              // "Low", "Medium", "High", "Critical"
    public bool RequiresEscort { get; set; }
}

public class SecurityViolation
{
    public string Type { get; set; }                   // "Unauthorized Area", "Restricted Zone"
    public string AreaName { get; set; }
    public DateTime Timestamp { get; set; }
    public string Description { get; set; }
}
```

**Security Rules:**
```csharp
// Define security zones
public enum SecurityZone
{
    Public,          // Lobby, Banking Hall, Cafeteria
    Secure,          // Meeting Rooms, Office Areas
    Restricted,      // Server Room, Vault, Executive Floors
    Critical         // Data Center, Cash Handling Areas
}

// Area to zone mapping
public class AreaZoneMapping
{
    public Guid AreaId { get; set; }
    public string AreaName { get; set; }
    public SecurityZone Zone { get; set; }
    public bool RequiresEscort { get; set; }
    public List<SecurityZone> AllowedFromZones { get; set; }
}
```

**Validation Logic:**
```sql
-- Check for unauthorized zone access
WITH JourneyZones AS (
    SELECT
        v.PersonId,
        v.AreaName,
        z.SecurityZone,
        v.EnterTime,
        LAG(z.SecurityZone) OVER (PARTITION BY v.PersonId ORDER BY v.EnterTime) as PrevZone
    FROM VisitorSession v
    JOIN AreaZoneMapping z ON v.AreaId = z.AreaId
    WHERE v.PersonId = @VisitorId
),
Violations AS (
    SELECT
        PersonId,
        AreaName,
        SecurityZone,
        EnterTime,
        'Unauthorized Zone Access' as Type,
        CASE
            WHEN PrevZone = 'Restricted' AND SecurityZone = 'Critical' THEN 'Valid'
            WHEN PrevZone = 'Secure' AND SecurityZone = 'Restricted' THEN 'Valid'
            WHEN PrevZone = 'Public' AND SecurityZone = 'Secure' THEN 'Valid'
            ELSE 'VIOLATION'
        END as IsValid
    FROM JourneyZones
    WHERE IsValid = 'VIOLATION'
)
SELECT * FROM Violations
```

**Example Alert:**
```json
{
  "visitorId": "John Doe",
  "visitorName": "John Doe",
  "pathTaken": ["Lobby", "Meeting Room", "Server Room"],
  "violations": [
    {
      "type": "Unauthorized Zone Access",
      "areaName": "Server Room",
      "timestamp": "2025-02-08T14:30:00",
      "description": "Server Room requires prior authorization and escort"
    }
  ],
  "riskLevel": "High",
  "requiresEscort": true
}
```

---

### 3. Zone Crossing Analysis

**Endpoint:** `POST /api/TrackingAnalytics/user-journey/zone-crossing`

**Purpose:** Track transitions between security zones

**Returns:**
```csharp
public class ZoneCrossingRead
{
    public string FromZone { get; set; }              // "Public", "Secure", "Restricted"
    public string ToZone { get; set; }
    public int CrossingCount { get; set; }
    public List<string> Visitors { get; set; }
    public List<string> EntryPoints { get; set; }
    public bool RequiresEscorting { get; set; }
    public double AvgTransitionTime { get; set; }     // Minutes
}
```

**SQL Logic:**
```sql
WITH ZoneTransitions AS (
    SELECT
        v1.PersonId,
        z1.SecurityZone as FromZone,
        z2.SecurityZone as ToZone,
        v1.AreaName as FromArea,
        v2.AreaName as ToArea,
        v2.EnterTime - v1.ExitTime as TransitionMinutes
    FROM VisitorSession v1
    JOIN VisitorSession v2 ON v1.PersonId = v2.PersonId
        AND v2.EnterTime > v1.ExitTime
    JOIN AreaZoneMapping z1 ON v1.AreaId = z1.AreaId
    JOIN AreaZoneMapping z2 ON v2.AreaId = z2.AreaId
    WHERE z1.SecurityZone != z2.SecurityZone
)
SELECT
    FromZone,
    ToZone,
    COUNT(*) as CrossingCount,
    AVG(TransitionMinutes) as AvgTransitionTime
FROM ZoneTransitions
GROUP BY FromZone, ToZone
ORDER BY CrossingCount DESC
```

**Use Cases:**
- Monitor Public → Secure crossings (normal visitor flow)
- Alert on Secure → Restricted (unusual, requires review)
- Track Secure → Public (normal exit flow)

---

### 4. Next Area Prediction

**Endpoint:** `POST /api/TrackingAnalytics/user-journey/next-areas`

**Purpose:** Given current area, predict next likely areas

**Returns:**
```csharp
public class NextAreaPredictionRead
{
    public string CurrentArea { get; set; }
    public List<AreaProbability> NextAreas { get; set; }
}

public class AreaProbability
{
    public string AreaName { get; set; }
    public double Probability { get; set; }            // 0.0 to 1.0
    public int Count { get; set; }
    public double AvgTimeToReach { get; set; }        // Minutes
}
```

**SQL Logic:**
```sql
WITH AreaTransitions AS (
    SELECT
        v1.AreaName as CurrentArea,
        v2.AreaName as NextArea,
        v2.EnterTime - v1.ExitTime as TransitionMinutes
    FROM VisitorSession v1
    JOIN VisitorSession v2 ON v1.PersonId = v2.PersonId
        AND v2.EnterTime > v1.ExitTime
        AND DATEDIFF(minute, v1.ExitTime, v2.EnterTime) < 30  -- Within 30 min
)
SELECT
    CurrentArea,
    NextArea,
    COUNT(*) as Count,
    COUNT(*) * 1.0 / SUM(COUNT(*)) OVER (PARTITION BY CurrentArea) as Probability,
    AVG(TransitionMinutes) as AvgTimeToReach
FROM AreaTransitions
WHERE CurrentArea = @CurrentArea
GROUP BY CurrentArea, NextArea
ORDER BY Probability DESC
```

**Example Output:**
```json
{
  "currentArea": "Lobby",
  "nextAreas": [
    { "areaName": "Security Check", "probability": 0.65, "count": 456, "avgTimeToReach": 2.3 },
    { "areaName": "Banking Hall", "probability": 0.25, "count": 178, "avgTimeToReach": 5.1 },
    { "areaName": "Cafeteria", "probability": 0.10, "count": 67, "avgTimeToReach": 3.5 }
  ]
}
```

---

## 🟡 P1: Visitor Flow Optimization

### 5. Security Checkpoint Bottleneck Analysis

**Endpoint:** `POST /api/TrackingAnalytics/bottlenecks`

**Purpose:** Identify where queues form during peak times

**Returns:**
```csharp
public class BottleneckAnalysisRead
{
    public string AreaName { get; set; }             // "Main Security Checkpoint"
    public string PeakHour { get; set; }              // "08:00-10:00"
    public int AvgWaitTime { get; set; }             // 15 minutes
    public int MaxQueueLength { get; set; }           // 45 people
    public List<string> ContributingAreas { get; set; }
    public string Suggestion { get; set; }            // "Add 1 security guard"
}
```

**SQL Logic:**
```sql
WITH HourlyOccupancy AS (
    SELECT
        AreaName,
        DATEPART(hour, EnterTime) as Hour,
        COUNT(*) as SimultaneousVisitors,
        AVG(DurationInMinutes) as AvgDwellTime
    FROM VisitorSession
    WHERE EnterTime >= @From AND EnterTime <= @To
    GROUP BY AreaName, DATEPART(hour, EnterTime)
),
Bottlenecks AS (
    SELECT
        AreaName,
        Hour,
        SimultaneousVisitors as MaxQueueLength,
        AvgDwellTime as AvgWaitTime,
        ROW_NUMBER() OVER (PARTITION BY AreaName ORDER BY SimultaneousVisitors DESC) as Rank
    FROM HourlyOccupancy
)
SELECT
    AreaName,
    CAST(Hour as varchar) + ':00-' + CAST(Hour+1 as varchar) + ':00' as PeakHour,
    AvgWaitTime,
    MaxQueueLength,
    CASE
        WHEN MaxQueueLength > 50 THEN 'Add 2 staff'
        WHEN MaxQueueLength > 30 THEN 'Add 1 staff'
        ELSE 'Adequate staffing'
    END as Suggestion
FROM Bottlenecks
WHERE Rank <= 3
ORDER BY AreaName, MaxQueueLength DESC
```

---

### 6. Staff Scheduling Optimization

**Endpoint:** `POST /api/TrackingAnalytics/staff-optimization`

**Purpose:** Optimize guard/staff placement

**Returns:**
```csharp
public class StaffOptimizationRead
{
    public string AreaName { get; set; }
    public List<TimeSlot> PeakSlots { get; set; }
    public int RecommendedStaff { get; set; }
    public int CurrentStaff { get; set; }
    public string Reason { get; set; }
}

public class TimeSlot
{
    public string TimeRange { get; set; }
    public int AvgVisitors { get; set; }
    public int StaffUtilization { get; set; }
}
```

---

### 7. Journey Funnel Analysis

**Endpoint:** `POST /api/TrackingAnalytics/user-journey/funnel`

**Purpose:** Track drop-off at each step

**Returns:**
```csharp
public class JourneyFunnelRead
{
    public string StepName { get; set; }               // "Enter Lobby", "Security Check", "Banking Hall"
    public int Visitors { get; set; }
    public double DropOffRate { get; set; }
    public double ConversionRate { get; set; }
    public double AvgTimeFromPrevious { get; set; }
}
```

**Use Cases for Bank Indonesia:**
- Conference: Registration → Keynote → Lunch → Breakout
- Banking: Entrance → Security → Teller → Manager
- Security Screening: Entry → Document Check → Bag Scan → Access Granted

---

## 🔵 P2: Additional Analytics

### 8. Alarm Response Time Analytics

**Endpoint:** `POST /api/AlarmAnalyticsIncident/response-time`

**Purpose:** Track operator performance

**Returns:**
```csharp
public class ResponseTimeAnalyticsRead
{
    public double AvgResponseTimeMinutes { get; set; }
    public double MedianResponseTimeMinutes { get; set; }
    public double P95ResponseTimeMinutes { get; set; }
    public int SlaBreaches { get; set; }
    public double SlaComplianceRate { get; set; }
    public List<OperatorPerformance> Operators { get; set; }
}
```

**SQL Logic:**
```sql
SELECT
    InvestigatedBy as OperatorName,
    AVG(DATEDIFF(minute, Timestamp, InvestigatedTimestamp)) as AvgResponseTimeMinutes,
    PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY DATEDIFF(minute, Timestamp, InvestigatedTimestamp)) as MedianResponseTime,
    PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY DATEDIFF(minute, Timestamp, InvestigatedTimestamp)) as P95ResponseTime,
    COUNT(*) as TotalAlarms,
    SUM(CASE WHEN DATEDIFF(minute, Timestamp, InvestigatedTimestamp) > 15 THEN 1 ELSE 0 END) as SlaBreaches
FROM AlarmRecordTracking
WHERE InvestigatedTimestamp IS NOT NULL
GROUP BY InvestigatedBy
```

---

### 9. Period-over-Period Comparison

**Endpoint:** `POST /api/TrackingAnalytics/compare-periods`

**Purpose:** WoW (Week-over-Week), MoM (Month-over-Month) comparison

**Returns:**
```csharp
public class PeriodComparisonRead
{
    public string MetricName { get; set; }
    public double Period1Value { get; set; }
    public double Period2Value { get; set; }
    public double AbsoluteChange { get; set; }
    public double PercentageChange { get; set; }
    public string Trend { get; set; }
}
```

---

### 10. Area Utilization Dashboard

**Endpoint:** `POST /api/TrackingAnalytics/area-utilization`

**Purpose:** Understand how areas are used

**Returns:**
```csharp
public class AreaUtilizationRead
{
    public string AreaName { get; set; }
    public double Capacity { get; set; }
    public double PeakOccupancy { get; set; }
    public double UtilizationRate { get; set; }
    public string PeakHour { get; set; }
}
```

---

## 📋 Implementation Priority Matrix

| Feature | Security | Flow | Optimization | Effort | Priority |
|---------|----------|------|--------------|--------|----------|
| **Common Paths Analysis** | ✅ | ✅ | | Medium | **P0** ⭐⭐⭐ |
| **Security Journey Validation** | ✅ | | | Low | **P0** ⭐⭐⭐ |
| **Zone Crossing Analysis** | ✅ | | | Low | **P0** ⭐⭐⭐ |
| **Next Area Prediction** | ✅ | ✅ | | Low | **P1** ⭐⭐ |
| **Bottleneck Analysis** | | ✅ | ✅ | Medium | **P1** ⭐⭐ |
| **Staff Optimization** | | | ✅ | Medium | **P1** ⭐⭐ |
| **Journey Funnel** | | ✅ | | Low | **P1** ⭐⭐ |
| **Response Time Analytics** | | | ✅ | Low | **P2** ⭐ |
| **Period-over-Period** | ✅ | ✅ | ✅ | Medium | **P2** ⭐ |
| **Area Utilization** | | | ✅ | Medium | **P2** ⭐ |

---

## 🔧 Technical Implementation

### Files to Create

**1. UserJourneyRead.cs**
```
Shared/Shared.Contracts/Analytics/UserJourneyRead.cs
```
- CommonPathRead
- SecurityJourneyCheckRead
- ZoneCrossingRead
- NextAreaPredictionRead
- JourneyFunnelRead

**2. UserJourneyRepository.cs**
```
Shared/Repositories/Repository/Analytics/UserJourneyRepository.cs
```
- GetCommonPathsAsync()
- GetSecurityCheckAsync()
- GetZoneCrossingAsync()
- GetNextAreasAsync()
- GetJourneyFunnelAsync()

**3. UserJourneyService.cs**
```
Shared/BusinessLogic.Services/Implementation/Analytics/UserJourneyService.cs
```
- Business logic for journey analysis
- Security zone validation
- Aggregation and pattern recognition

**4. UserJourneyController.cs**
```
Shared/Web.API.Controllers/Controllers/Analytics/UserJourneyController.cs
```
- REST API endpoints
- [MinLevel] authorization
- ApiResponse wrapping

### Database Schema

**Security Zones Table:**
```sql
CREATE TABLE SecurityZoneMapping (
    AreaId UNIQUEIDENTIFIER PRIMARY KEY,
    AreaName NVARCHAR(255),
    SecurityZone NVARCHAR(50),      -- Public, Secure, Restricted, Critical
    RequiresEscort BIT,
    AllowedFromZones NVARCHAR(200)  -- JSON array
);
```

**Journey Summary Table (for performance):**
```sql
CREATE TABLE JourneySummary (
    SummaryDate DATE,
    PathHash NVARCHAR(64),           -- MD5 of area sequence
    AreaSequence NVARCHAR(500),      -- "Lobby→Security→Banking"
    JourneyCount INT,
    AvgDurationMinutes DECIMAL(10,2),
    PRIMARY KEY (SummaryDate, PathHash)
);

-- Refresh daily via scheduled job
```

---

## 📊 Example Use Cases for Bank Indonesia

### Use Case 1: Security - Unauthorized Access Detection

**Scenario:** Visitor accessed Server Room without authorization

**User Journey Detection:**
```
Path: Lobby → Meeting Room → Server Room
Alert: Server Room requires prior authorization
Action: Security team notified
Result: Immediate interception
```

### Use Case 2: Visitor Flow - Security Checkpoint Bottleneck

**Problem:** Long queues at main entrance during morning rush (8-9am)

**Analysis:**
```
Bottleneck: Main Security Checkpoint
Peak: 08:00-09:00
Avg Wait: 25 minutes
Max Queue: 67 people
Suggestion: Add 2 security guards during 8-10am
Result: Wait time reduced to 12 minutes
```

### Use Case 3: Resource Optimization - Staff Scheduling

**Problem:** Overstaffed during quiet hours, understaffed during peaks

**Optimization:**
```
Lobby Security:
  07:00-09:00: 4 guards (high traffic)
  09:00-12:00: 2 guards (moderate)
  12:00-14:00: 3 guards (lunch rush)
  14:00-17:00: 2 guards (moderate)
  17:00-19:00: 1 guard (low)
```

---

## 🚀 Implementation Plan

### Phase 1 (2-3 weeks): Security-Focused User Journey

**Week 1:**
1. Create UserJourneyRead DTOs
2. Implement GetCommonPathsAsync()
3. Implement GetSecurityCheckAsync()
4. Create SecurityZoneMapping table

**Week 2:**
5. Implement GetZoneCrossingAsync()
6. Implement GetNextAreasAsync()
7. Create UserJourneyController
8. Unit tests

**Week 3:**
9. Frontend integration (if applicable)
10. Security alert configuration
11. Documentation
12. UAT testing

### Phase 2 (1-2 weeks): Flow Optimization

**Week 4:**
1. Implement GetBottleneckAnalysisAsync()
2. Implement GetStaffOptimizationAsync()
3. Implement GetJourneyFunnelAsync()

**Week 5:**
4. Dashboard creation
5. Report generation
6. Performance optimization

### Phase 3 (Future): Advanced Analytics

- Response time analytics
- Period-over-period comparison
- Real-time monitoring (SSE/WebSocket)
- Anomaly detection (ML-based)

---

## ✅ Success Metrics

**Security Metrics:**
- Detect 95%+ of unusual movement patterns
- Reduce unauthorized access incidents by 80%
- Alert on zone violations within 30 seconds

**Flow Metrics:**
- Reduce security checkpoint wait times by 30%
- Identify and resolve 3+ major bottlenecks
- Improve visitor journey efficiency by 20%

**Resource Metrics:**
- Optimize guard/staff placement based on data
- Reduce overstaffing during quiet hours
- Improve staff utilization rate to 85%+

---

## 📁 Key Files Reference

**Existing Files:**
- `Shared/Shared.Contracts/Analytics/VisitorSessionRead.cs` - Building block for journey
- `Shared/Repositories/Repository/Analytics/TrackingSessionRepository.cs` - Session logic
- `Shared/BusinessLogic.Services/Implementation/Analytics/TrackingSessionService.cs` - Service layer

**New Files to Create:**
- `Shared/Shared.Contracts/Analytics/UserJourneyRead.cs`
- `Shared/Repositories/Repository/Analytics/UserJourneyRepository.cs`
- `Shared/BusinessLogic.Services/Implementation/Analytics/UserJourneyService.cs`
- `Shared/Web.API.Controllers/Controllers/Analytics/UserJourneyController.cs`

**Documentation:**
- This file: `docs/analytics-enhancement-plan.md`
- API documentation (Swagger) - Auto-generated from controllers

---

## 🔍 Notes

**VisitorSession vs User Journey:**

**VisitorSession (existing):**
- Granular: 1 session = 1 area dwell
- Can be ordered by `EnterTime` per person to see sequence
- VisualPaths provides X,Y coordinates for floorplan

**User Journey (new feature):**
- Aggregated: Links sessions by person into multi-area journeys
- "Common Paths" - Which area sequences are most popular
- "Next Area" - Probability of where visitors go next
- "Security Validation" - Flags unusual/authorized paths

**Example:**
```sql
-- Existing: Get sessions for John
SELECT * FROM VisitorSession
WHERE PersonId = 'John'
ORDER BY EnterTime

-- Result:
John - Lobby (10:00-10:15)
John - Banking Hall (10:20-10:45)
John - Meeting Room (11:00-12:00)

-- New: Get common paths (all visitors)
SELECT AreaSequence, COUNT(*) as JourneyCount
FROM (
    SELECT STRING_AGG(AreaName, '→' ORDER BY EnterTime) as AreaSequence
    FROM VisitorSession
    GROUP BY PersonId
) Journeys
GROUP BY AreaSequence
ORDER BY JourneyCount DESC

-- Result:
"Lobby→Banking Hall→Meeting Room": 45 journeys
"Lobby→Security→Office Area": 123 journeys
"Lobby→Cafeteria": 234 journeys
```

---

**Ready for implementation. Let's start with Phase 1: Security-Focused User Journey!**
