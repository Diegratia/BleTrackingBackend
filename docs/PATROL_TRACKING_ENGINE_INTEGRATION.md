# Patrol End-to-End Documentation for Tracking Engine (Node.js)

## Overview

This document explains the complete patrol mechanism in BLE Tracking Backend for integration with the Node.js tracking engine. The patrol system enables security personnel to perform scheduled patrols through predefined checkpoints, with real-time event tracking via MQTT.

---

## MQTT Events - Tracking Engine Should Subscribe To

### 1. Session Started Event

**Topic:** `patrol/session/started`

**Payload:**
```json
{
  "sessionId": "guid",
  "securityId": "guid"
}
```

**When Emitted:** When a security guard successfully starts a patrol session

**Tracking Engine Actions:**
- Track active patrol sessions
- Monitor security guard location via BLE readers
- Optional: Trigger real-time notifications to monitoring dashboard

### 2. Session Ended Event

**Topic:** `patrol/session/ended`

**Payload:** Empty string `""`

**When Emitted:** When a security guard stops/completes a patrol session

**Tracking Engine Actions:**
- Mark session as complete
- Calculate final patrol statistics
- Generate patrol completion report

---

## Patrol End-to-End Flow

### Phase 1: Patrol Assignment Configuration

**Entity:** `PatrolAssignment`

| Property | Type | Description |
|----------|------|-------------|
| `Name` | string | Assignment name |
| `PatrolRouteId` | Guid | Route with checkpoints to patrol |
| `TimeGroupId` | Guid? | Schedule (day/time blocks) |
| `StartDate` | DateTime? | Assignment valid from |
| `EndDate` | DateTime? | Assignment valid until |
| `CycleCount` | int | Number of cycles (default: 1) |
| `CycleType` | enum | HalfCycle (1 trip/cycle) or FullCycle (2 trips/cycle) |
| `DurationType` | enum | NoDuration or WithDuration (enforces dwell time) |
| `ApprovalType` | enum | ByThreatLevel, WithoutApproval, Or, And, Sequential |

**TimeBlock Structure:**
```javascript
{
  dayOfWeek: 0-6,  // 0=Sunday, 1=Monday, etc.
  startTime: "HH:mm:ss",
  endTime: "HH:mm:ss"
}
```

**PatrolRouteArea Structure:**
```javascript
{
  patrolAreaId: "guid",
  orderIndex: 1,
  estimatedDistance: 100,  // meters from previous checkpoint
  minDwellTime: 30,  // minimum seconds to stay (optional)
  maxDwellTime: 300  // maximum seconds (analytics only)
}
```

---

### Phase 2: Starting a Patrol Session

**API Endpoint:** `POST /api/PatrolSession/start`

**Request Body:**
```json
{
  "patrolAssignmentId": "guid"
}
```

**Validations Performed (in order):**

1. **Security Assignment Check**
   - Must be original assignee OR active substitute
   - Original assignee blocked if substituted today

2. **Date Range Validation**
   - Current time must be within StartDate-EndDate

3. **TimeGroup Validation**
   - Current day must match TimeBlock.DayOfWeek
   - Current time must be within StartTime-EndTime
   - No existing session for this TimeBlock today

4. **Active Session Check**
   - No active session exists for this assignment

5. **Route Validation**
   - Route must have active checkpoints

**On Success:**
- Creates `PatrolSession` entity
- Creates `PatrolCheckpointLog` for each checkpoint
- Emits MQTT: `patrol/session/started`

**Checkpoint Log Creation Logic:**
```javascript
// Cycle & Trip Calculation
if (cycleType === "FullCycle") {
  totalTrips = cycleCount * 2;  // Round trip: A-B-C then C-B-A
} else {  // HalfCycle
  totalTrips = cycleCount;       // One-way: A-B-C
}

// Direction alternates
for (trip = 1; trip <= totalTrips; trip++) {
  if (trip % 2 === 0) {
    // Even trip: Reverse direction (C-B-A)
  } else {
    // Odd trip: Forward direction (A-B-C)
  }
  // Create checkpoint logs for each area
}
```

**PatrolCheckpointLog Initial State:**
```javascript
{
  patrolSessionId: "guid",
  patrolAreaId: "guid",
  areaNameSnap: "Area Name",
  orderIndex: 1,
  checkpointStatus: 0,  // AutoDetected
  arrivedAt: null,
  leftAt: null,
  clearedAt: null,
  minDwellTime: 30,
  maxDwellTime: 300
}
```

---

### Phase 3: Checkpoint Actions

#### 3.1 Arriving at Checkpoint

**Current System:** MANUAL arrival (no auto-detection)

**Tracking Engine Opportunity:**
- BLE Reader detects security tag → Call API to set ArrivedAt
- Geofence detection → Call API to set ArrivedAt

**API Endpoint:** `PUT /api/PatrolCheckpoint/{id}/arrive` (if implemented)

#### 3.2 Submitting Checkpoint Action

**API Endpoint:** `POST /api/PatrolSession/checkpoint-action`

**Request Body:**
```json
{
  "patrolCheckpointLogId": "guid",
  "patrolAreaId": "guid",
  "patrolCheckpointStatus": "Cleared" | "HasCase",
  "securityNote": "string (optional)",
  "caseDetails": {  // Required if status=HasCase
    "title": "string",
    "description": "string",
    "caseType": "Incident" | "Hazard" | "Damage" | "Theft" | "Report" | "PatrolSummary",
    "threatLevel": "None" | "Low" | "Medium" | "High" | "Critical"
  }
}
```

**Validations Performed:**

1. **Active Checkpoint Check**
   - Checkpoint must have ArrivedAt set
   - LeftAt must be null (not already left)

2. **Dwell Time Validation** (if DurationType=WithDuration)
   - Must stay minimum MinDwellTime seconds (default 30)
   - MaxDwellTime is for analytics only (no exception)

3. **Sequence Validation**
   - Previous checkpoints must be cleared first
   - Cannot skip checkpoints in route

4. **HasCase Validation**
   - caseDetails required when status=HasCase

**Results:**

| Status | Action |
|--------|--------|
| `Cleared` | Checkpoint marked as cleared, no case created |
| `HasCase` | Checkpoint marked + PatrolCase created |

---

### Phase 4: Stopping a Patrol Session

**API Endpoint:** `POST /api/PatrolSession/{id}/stop`

**Validations Performed:**

1. **Ownership Check**
   - Session must belong to current security

2. **Session State Check**
   - Session must not already be stopped

**On Success:**
- All `AutoDetected` checkpoints → `Missed`
- Session `EndedAt` set to current time
- Emits MQTT: `patrol/session/ended`

---

## Patrol Case Approval Workflow

### Case Creation (Automatic on HasCase)

**Created with initial state:** `Submitted`

### Approval Logic by ThreatLevel

| ThreatLevel | Approval Mode | Required Approvals |
|-------------|----------------|-------------------|
| `None` | WithoutApproval | Auto-Approved |
| `Low` | Or | Any one head approves |
| `Medium` | Or | Any one head approves |
| `High` | And | Both heads must approve |
| `Critical` | Sequential | Head1 → then Head2 |

### Approval Modes

| Mode | Description |
|------|-------------|
| `ByThreatLevel` | Determined by ThreatLevel (above) |
| `WithoutApproval` | Auto-Approved |
| `Or` | Any one head |
| `And` | All heads required |
| `Sequential` | Heads in order (1 then 2) |

---

## Data Models

### PatrolSession
```javascript
{
  id: "guid",
  patrolAssignmentId: "guid",
  patrolAssignmentNameSnap: "Assignment Name",
  patrolRouteId: "guid",
  patrolRouteNameSnap: "Route Name",
  timeGroupId: "guid?",
  timeGroupNameSnap: "Time Group Name",
  securityId: "guid",
  securityNameSnap: "Security Name",
  securityIdentityIdSnap: "ID123",
  securityCardNumberSnap: "CARD001",
  startedAt: "datetime",
  endedAt: "datetime?",
  createdAt: "datetime",
  updatedAt: "datetime"
}
```

### PatrolCheckpointLog
```javascript
{
  id: "guid",
  patrolSessionId: "guid",
  patrolAreaId: "guid",
  areaNameSnap: "Area Name",
  orderIndex: 1,
  checkpointStatus: 0|1|2|3,  // AutoDetected|Cleared|HasCase|Missed
  arrivedAt: "datetime?",
  leftAt: "datetime?",
  clearedAt: "datetime?",
  minDwellTime: 30,
  maxDwellTime: 300,
  notes: "string?"
}
```

### PatrolCase
```javascript
{
  id: "guid",
  patrolSessionId: "guid",
  patrolAreaId: "guid?",
  title: "string",
  description: "string",
  caseType: "Incident|Hazard|Damage|Theft|Report|PatrolSummary",
  threatLevel: "None|Low|Medium|High|Critical",
  caseStatus: "Open|Submitted|Approved|Rejected|Closed",
  securityHead1Id: "guid?",
  securityHead2Id: "guid?",
  approvedByHead1: "bool",
  approvedByHead2: "bool",
  approvedByHead1At: "datetime?",
  approvedByHead2At: "datetime?",
  createdAt: "datetime",
  updatedAt: "datetime"
}
```

---

## Enums Reference

### PatrolCheckpointStatus
```javascript
{
  AutoDetected: 0,  // Initial state, not yet visited
  Cleared: 1,       // Visited and cleared
  HasCase: 2,       // Visited, case created
  Missed: 3         // Never visited
}
```

### PatrolCycleType
```javascript
{
  HalfCycle: 0,  // 1 cycle = 1 trip (A-B-C)
  FullCycle: 1   // 1 cycle = 2 trips (A-B-C + C-B-A)
}
```

### PatrolDurationType
```javascript
{
  NoDuration: 0,    // Dwell time not enforced
  WithDuration: 1   // MinDwellTime enforced
}
```

### PatrolStartType
```javascript
{
  Manual: 0,    // Security starts manually
  AutoStart: 1  // System auto-starts (future)
}
```

### PatrolApprovalType
```javascript
{
  ByThreatLevel: 0,  // Approval by threat level
  WithoutApproval: 1,
  Or: 2,
  And: 3,
  Sequential: 4
}
```

### CaseStatus
```javascript
{
  Open: 0,
  Submitted: 1,
  Approved: 2,
  Rejected: 3,
  Closed: 4
}
```

### CaseType
```javascript
{
  Incident: 0,
  Hazard: 1,
  Damage: 2,
  Theft: 3,
  Report: 4,
  PatrolSummary: 5
}
```

### ThreatLevel
```javascript
{
  None: 0,
  Low: 1,
  Medium: 2,
  High: 3,
  Critical: 4
}
```

---

## API Endpoints Summary for Tracking Engine

### Session Management
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/PatrolSession/start` | Start a new patrol session |
| POST | `/api/PatrolSession/{id}/stop` | Stop current session |
| GET | `/api/PatrolSession/{id}` | Get session details |
| GET | `/api/PatrolSession` | Get all sessions |

### Checkpoint Actions
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/PatrolSession/checkpoint-action` | Submit checkpoint action |

### Case Management
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/PatrolCase` | Create new case |
| PUT | `/api/PatrolCase/{id}/approve` | Approve case |
| PUT | `/api/PatrolCase/{id}/reject` | Reject case |
| PUT | `/api/PatrolCase/{id}/close` | Close approved case |

---

## Integration Points for Node.js Tracking Engine

### 1. MQTT Subscription
```javascript
// Subscribe to patrol events
mqttClient.subscribe('patrol/session/started');
mqttClient.subscribe('patrol/session/ended');

mqttClient.on('message', (topic, message) => {
  if (topic === 'patrol/session/started') {
    const { sessionId, securityId } = JSON.parse(message);
    // Start tracking security location
    startTrackingSession(sessionId, securityId);
  }
  else if (topic === 'patrol/session/ended') {
    // Stop tracking session
    stopTrackingSession(message.toString());
  }
});
```

### 2. BLE Integration (Future Enhancement)
```javascript
// When BLE reader detects security tag at checkpoint
async function onBeaconDetected(readerId, tagId, areaId) {
  const security = await getSecurityByTag(tagId);
  const activeSession = await getActiveSession(security.id);

  if (activeSession) {
    // Find checkpoint log for this area
    const checkpoint = await findCheckpointByArea(activeSession.id, areaId);

    if (checkpoint && !checkpoint.arrivedAt) {
      // Set arrival time
      await setCheckpointArrival(checkpoint.id);
    }
  }
}
```

### 3. Geofence Integration (Alternative)
```javascript
// When security enters geofenced area
async function onGeofenceEntry(securityId, areaId) {
  const activeSession = await getActiveSession(securityId);

  if (activeSession) {
    const checkpoint = await findCheckpointByArea(activeSession.id, areaId);

    if (checkpoint && !checkpoint.arrivedAt) {
      await setCheckpointArrival(checkpoint.id);
    }
  }
}
```

---

## Critical Files Reference

| File | Purpose |
|------|---------|
| `PatrolSessionService.cs` | Session lifecycle, MQTT events |
| `PatrolAssignmentService.cs` | Assignment validation, overlap check |
| `PatrolCaseService.cs` | Case creation and approval workflow |
| `PatrolSessionRepository.cs` | Data access, checkpoint queries |
| `Enum.cs` | All enum definitions |
| `PatrolSessionController.cs` | Session API endpoints |
| `PatrolCaseController.cs` | Case API endpoints |

---

## MQTT Broker Configuration

**Connection Details (from .env):**
```bash
MQTT_HOST=localhost
MQTT_PORT=1883
MQTT_USERNAME=bio_mqtt
MQTT_PASSWORD=P@ssw0rd
```

**Topics Published by Patrol System:**
- `patrol/session/started` - When patrol session starts
- `patrol/session/ended` - When patrol session ends

---

## Sample API Flow

### Starting a Patrol Session

```bash
curl -X POST http://localhost:5020/api/PatrolSession/start \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <jwt_token>" \
  -d '{
    "patrolAssignmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }'
```

**Response (201 Created):**
```json
{
  "message": "Patrol Session created successfully",
  "data": {
    "id": "7fa85f64-5717-4562-b3fc-2c963f66afa7",
    "patrolAssignmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "patrolAssignmentNameSnap": "Night Patrol Building A",
    "patrolRouteId": "2fa85f64-5717-4562-b3fc-2c963f66afa5",
    "patrolRouteNameSnap": "Main Route",
    "securityId": "1fa85f64-5717-4562-b3fc-2c963f66afa4",
    "securityNameSnap": "John Doe",
    "securityIdentityIdSnap": "EMP001",
    "securityCardNumberSnap": "CARD001",
    "startedAt": "2026-03-05T10:30:00Z",
    "endedAt": null,
    "checkpoints": [
      {
        "id": "8fa85f64-5717-4562-b3fc-2c963f66afa8",
        "patrolAreaId": "9fa85f64-5717-4562-b3fc-2c963f66afa9",
        "areaNameSnap": "Lobby",
        "orderIndex": 1,
        "checkpointStatus": 0,
        "arrivedAt": null,
        "leftAt": null,
        "minDwellTime": 30,
        "maxDwellTime": 300
      }
    ]
  }
}
```

### Submitting Checkpoint Action

```bash
curl -X POST http://localhost:5020/api/PatrolSession/checkpoint-action \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <jwt_token>" \
  -d '{
    "patrolCheckpointLogId": "8fa85f64-5717-4562-b3fc-2c963f66afa8",
    "patrolAreaId": "9fa85f64-5717-4562-b3fc-2c963f66afa9",
    "patrolCheckpointStatus": "Cleared",
    "securityNote": "All clear"
  }'
```

### Submitting Checkpoint with Case

```bash
curl -X POST http://localhost:5020/api/PatrolSession/checkpoint-action \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <jwt_token>" \
  -d '{
    "patrolCheckpointLogId": "8fa85f64-5717-4562-b3fc-2c963f66afa8",
    "patrolAreaId": "9fa85f64-5717-4562-b3fc-2c963f66afa9",
    "patrolCheckpointStatus": "HasCase",
    "securityNote": "Found suspicious activity",
    "caseDetails": {
      "title": "Suspicious Package",
      "description": "Unattended bag found near entrance",
      "caseType": "Hazard",
      "threatLevel": "Medium"
    }
  }'
```

### Stopping a Patrol Session

```bash
curl -X POST http://localhost:5020/api/PatrolSession/7fa85f64-5717-4562-b3fc-2c963f66afa7/stop \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <jwt_token>"
```

---

## Error Responses

### Validation Errors (400 Bad Request)
```json
{
  "message": "Validation failed",
  "errors": {
    "patrolAssignmentId": ["PatrolAssignmentId is required"]
  }
}
```

### Business Logic Errors (400 Bad Request)
```json
{
  "message": "You are not assigned to this patrol assignment today."
}
```

### Unauthorized (401 Unauthorized)
```json
{
  "message": "You are not allowed to stop this session"
}
```

### Not Found (404 Not Found)
```json
{
  "message": "Patrol Session with id {id} not found"
}
```

---

## Service Configuration

**Patrol Service Port:** 5020 (defined in `.env` as `PATROL_PORT`)

**Docker Service Name:** `patrol`

**Health Check:**
```bash
curl http://localhost:5020/health
```

---

## Notes for Tracking Engine Team

1. **MQTT Events are Reliable**: The `patrol/session/started` and `patrol/session/ended` events are emitted within transaction boundaries, ensuring data consistency.

2. **Checkpoints are Created at Session Start**: All checkpoint logs are pre-created when a session starts, with status `AutoDetected (0)`.

3. **Trip Direction Matters**: For FullCycle routes, checkpoints are created in both forward and reverse directions. Tracking engine should consider this when monitoring progress.

4. **Dwell Time is Optional**: Only enforced if `DurationType = WithDuration (1)`. The `minDwellTime` varies per checkpoint (from route configuration).

5. **Sequence Validation**: Security guards must complete checkpoints in order. They cannot skip ahead.

6. **Case Auto-Submission**: Cases created during checkpoint action are automatically set to `Submitted` status and require approval based on threat level.

7. **Session Ownership**: Only the security personnel who started the session can stop it.

8. **Missed Checkpoints**: When a session is stopped, all unvisited checkpoints (status still `AutoDetected`) are automatically marked as `Missed`.

---

## Future Enhancements

1. **Auto-Arrival Detection**: BLE reader or geofence integration to automatically set `ArrivedAt` timestamp.

2. **Real-time Progress Tracking**: WebSocket or SSE endpoint for real-time session progress updates.

3. **Analytics API**: Additional endpoints for patrol statistics, completion rates, and performance metrics.

4. **Checkpoint Time Alert**: MQTT event when security exceeds `maxDwellTime` at a checkpoint.

---

## Support

For questions or issues related to the patrol system integration:
- Backend Team: [Contact]
- Documentation Repository: [Link]
- Issue Tracker: [Link]
