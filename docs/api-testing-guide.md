# User Journey Analytics - API Testing Guide

**Updated:** 2025-02-08
**Service:** Analytics (User Journey)
**Base URL:** `http://localhost:5030` (adjust based on your configuration)

---

## Prerequisites

### 1. Authentication
All endpoints require JWT authentication. You must obtain a valid token first:

```bash
# Login to get JWT token
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "superadmin",
    "password": "P@ssw0rd"
  }'

# Save the token from response
TOKEN="your-jwt-token-here"
```

### 2. Required Role
**Minimum Role:** `PrimaryAdmin` (LevelPriority = 2)

Users with lower roles will receive `403 Forbidden`.

---

## Endpoints

### 1. Get Common Paths Analysis

Analyzes journey patterns across all visitors/members to identify most popular routes.

**Endpoint:** `POST /api/UserJourney/common-paths`
**Authorization:** `Bearer <token>`
**Content-Type:** `application/json`

#### Request Body

```json
{
  "from": "2025-02-01T00:00:00Z",
  "to": "2025-02-08T23:59:59Z",
  "minJourneyLength": 2,
  "maxResults": 20,
  "buildingId": null,
  "floorId": null,
  "areaId": null,
  "visitorId": null,
  "memberId": null
}
```

#### Parameters

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `from` | DateTime | No | Start date (UTC). Default: 7 days ago |
| `to` | DateTime | No | End date (UTC). Default: now |
| `minJourneyLength` | int | No | Minimum areas visited to count as journey. Default: 2 |
| `maxResults` | int | No | Maximum number of paths to return. Default: 20 |
| `buildingId` | Guid | No | Filter by specific building |
| `floorId` | Guid | No | Filter by specific floor |
| `areaId` | Guid | No | Filter by specific area |
| `visitorId` | Guid | No | Filter by specific visitor |
| `memberId` | Guid | No | Filter by specific member |

#### Example Request

```bash
curl -X POST http://localhost:5030/api/UserJourney/common-paths \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "from": "2025-02-01T00:00:00Z",
    "to": "2025-02-08T23:59:59Z",
    "minJourneyLength": 2,
    "maxResults": 20
  }'
```

#### Success Response (200 OK)

```json
{
  "data": [
    {
      "pathId": "lobby-banking-office",
      "areaSequence": ["Lobby", "Banking Hall", "Office Area"],
      "journeyCount": 45,
      "percentage": 23.5,
      "avgDurationMinutes": 45.2,
      "isAnomaly": false,
      "riskLevel": "Low"
    },
    {
      "pathId": "lobby-server-room",
      "areaSequence": ["Lobby", "Server Room"],
      "journeyCount": 2,
      "percentage": 1.0,
      "avgDurationMinutes": 15.0,
      "isAnomaly": true,
      "riskLevel": "Medium"
    }
  ],
  "totalJourneys": 191,
  "dateRange": "2025-02-01 to 2025-02-08"
}
```

#### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `data` | array | List of common paths |
| `pathId` | string | Unique identifier (lowercase, hyphenated) |
| `areaSequence` | string[] | Areas visited in order |
| `journeyCount` | int | Number of times this path was taken |
| `percentage` | double | Percentage of total journeys |
| `avgDurationMinutes` | double | Average duration in minutes |
| `isAnomaly` | boolean | True if journey count < 5 |
| `riskLevel` | string | Risk assessment: Low, Medium, High |
| `totalJourneys` | int | Total unique journeys analyzed |
| `dateRange` | string | Analysis date range |

---

### 2. Security Check - Visitor

Performs security zone validation for a specific visitor's journey.

**Endpoint:** `POST /api/UserJourney/security-check/visitor/{visitorId}`
**Authorization:** `Bearer <token>`
**Content-Type:** `application/json`

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `visitorId` | Guid | Yes | Visitor ID to check |

#### Request Body

```json
{
  "from": "2025-02-08T00:00:00Z",
  "to": "2025-02-08T23:59:59Z"
}
```

#### Example Request

```bash
curl -X POST "http://localhost:5030/api/UserJourney/security-check/visitor/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "from": "2025-02-08T00:00:00Z",
    "to": "2025-02-08T23:59:59Z"
  }'
```

#### Success Response (200 OK) - Safe Journey

```json
{
  "visitorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "visitorName": "John Doe",
  "memberId": null,
  "memberName": null,
  "pathTaken": ["Lobby", "Banking Hall", "Cafeteria"],
  "riskLevel": "Low",
  "requiresEscort": false,
  "violations": [],
  "totalDurationMinutes": 120
}
```

#### Response - With Violations

```json
{
  "visitorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "visitorName": "John Doe",
  "memberId": null,
  "memberName": null,
  "pathTaken": ["Lobby", "Server Room", "Vault"],
  "riskLevel": "Critical",
  "requiresEscort": true,
  "violations": [
    {
      "type": "Unauthorized Zone Access",
      "areaName": "Server Room",
      "description": "Transition from Public to Restricted is not allowed",
      "zoneType": "Restricted"
    },
    {
      "type": "Escort Required",
      "areaName": "Vault",
      "description": "Vault requires escort for access",
      "zoneType": "Restricted"
    }
  ],
  "totalDurationMinutes": 45
}
```

#### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `visitorId` | Guid? | Visitor ID |
| `visitorName` | string? | Visitor name |
| `pathTaken` | string[] | Areas visited in order |
| `riskLevel` | string | Overall risk: Low, Medium, High, Critical |
| `requiresEscort` | boolean | Whether escort is required |
| `violations` | array | List of security violations |
| `violations[].type` | string | Violation type |
| `violations[].areaName` | string? | Where violation occurred |
| `violations[].description` | string | Violation details |
| `violations[].zoneType` | string? | Security zone type |
| `totalDurationMinutes` | int? | Total journey duration |

---

### 3. Security Check - Member

Performs security zone validation for a specific member's journey.

**Endpoint:** `POST /api/UserJourney/security-check/member/{memberId}`
**Authorization:** `Bearer <token>`
**Content-Type:** `application/json`

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `memberId` | Guid | Yes | Member ID to check |

#### Request Body

```json
{
  "from": "2025-02-08T00:00:00Z",
  "to": "2025-02-08T23:59:59Z"
}
```

#### Example Request

```bash
curl -X POST "http://localhost:5030/api/UserJourney/security-check/member/7fa85f64-5717-4562-b3fc-2c963f66afa7" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "from": "2025-02-08T00:00:00Z",
    "to": "2025-02-08T23:59:59Z"
  }'
```

#### Success Response (200 OK)

Same format as visitor security check, but with `memberId` and `memberName` fields populated.

---

## Error Responses

### 401 Unauthorized

```json
{
  "message": "Authorization required",
  "statusCode": 401
}
```

**Solution:** Check your JWT token is valid and not expired.

### 403 Forbidden

```json
{
  "message": "Insufficient permissions",
  "statusCode": 403
}
```

**Solution:** Ensure user has `PrimaryAdmin` role or higher.

### 400 Bad Request

```json
{
  "message": "Invalid date range",
  "statusCode": 400
}
```

**Solution:** Check request body parameters are valid.

### 404 Not Found

```json
{
  "message": "Visitor not found",
  "statusCode": 404
}
```

**Solution:** Verify the visitor/member ID exists in database.

### 500 Internal Server Error

```json
{
  "message": "An error occurred while processing your request",
  "statusCode": 500
}
```

**Solution:** Check server logs for details.

---

## Testing Scenarios

### Scenario 1: Normal Visitor Journey

**Test:** Check visitor with normal public area access

```bash
# Expected: No violations, Low risk
curl -X POST "http://localhost:5030/api/UserJourney/security-check/visitor/{visitor-id}" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"from": "2025-02-08T00:00:00Z", "to": "2025-02-08T23:59:59Z"}'
```

**Expected Result:**
- `riskLevel`: "Low"
- `violations`: []
- `requiresEscort`: false

---

### Scenario 2: Unauthorized Zone Access

**Test:** Visitor attempts to access restricted areas

```bash
# Expected: Security violations detected
curl -X POST "http://localhost:5030/api/UserJourney/security-check/visitor/{visitor-id}" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"from": "2025-02-08T00:00:00Z", "to": "2025-02-08T23:59:59Z"}'
```

**Expected Result:**
- `riskLevel`: "High" or "Critical"
- `violations`: Array with "Unauthorized Zone Access" type
- `requiresEscort`: true

---

### Scenario 3: Common Paths Analysis

**Test:** Get most popular journey patterns

```bash
# Expected: List of common paths with statistics
curl -X POST "http://localhost:5030/api/UserJourney/common-paths" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "from": "2025-02-01T00:00:00Z",
    "to": "2025-02-08T23:59:59Z",
    "minJourneyLength": 2,
    "maxResults": 20
  }'
```

**Expected Result:**
- `totalJourneys`: > 0
- `data`: Array of paths sorted by `journeyCount`
- Some paths marked `isAnomaly: true` if count < 5

---

### Scenario 4: Filtered Analysis by Building

**Test:** Get common paths for specific building

```bash
curl -X POST "http://localhost:5030/api/UserJourney/common-paths" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "from": "2025-02-01T00:00:00Z",
    "to": "2025-02-08T23:59:59Z",
    "buildingId": "your-building-id-guid"
  }'
```

---

### Scenario 5: Date Range Validation

**Test:** Check journey for specific time period

```bash
curl -X POST "http://localhost:5030/api/UserJourney/security-check/visitor/{visitor-id}" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "from": "2025-02-08T08:00:00Z",
    "to": "2025-02-08T17:00:00Z"
  }'
```

**Expected:** Only shows sessions within the specified time range.

---

## Postman Collection

Import this collection into Postman for easier testing:

```json
{
  "info": {
    "name": "User Journey Analytics",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "variable": [
    {
      "key": "baseUrl",
      "value": "http://localhost:5030"
    },
    {
      "key": "token",
      "value": "your-jwt-token-here"
    }
  ]
}
```

---

## Performance Testing

### Expected Response Times

| Endpoint | Expected Time | Max Acceptable |
|----------|--------------|----------------|
| Common Paths | < 3 sec | < 5 sec |
| Security Check | < 500ms | < 1 sec |

### Load Testing

```bash
# Using Apache Bench
ab -n 1000 -c 10 -H "Authorization: Bearer $TOKEN" \
   -p request.json -T application/json \
   http://localhost:5030/api/UserJourney/common-paths
```

---

## Troubleshooting

### Issue: "Areas without security zone mappings"

**Symptom:** Security check returns no violations but should have some.

**Solution:**
1. Run seed script: `Scripts/SeedSecurityZoneMappings.sql`
2. Or trigger seeding via `DatabaseSeeder.cs`

### Issue: "No journeys found"

**Symptom:** API returns empty data array.

**Possible Causes:**
1. No tracking transactions in date range
2. No visitors/members in database
3. Wrong date range (check timezone - use UTC)

**Solution:**
```sql
-- Verify data exists
SELECT COUNT(*) FROM tracking_transaction_20250208 WHERE trans_time >= '2025-02-08';
```

### Issue: "Migration not applied"

**Symptom:** 500 error about missing table.

**Solution:**
```bash
# Apply migration
dotnet ef database update --project Shared/Repositories/Repositories.csproj --startup-project Services.API/Analytics/Analytics.csproj
```

---

## Security Zone Reference

| Zone | Value | Description | Escort Required |
|------|-------|-------------|-----------------|
| Public | 1 | No restrictions | No |
| Secure | 2 | Badge required | No |
| Restricted | 3 | Special authorization | Yes |
| Critical | 4 | Highest security | Yes |

### Transition Rules

- **Public** → Any zone: Allowed
- **Secure** → Public, Secure: Allowed
- **Secure** → Restricted, Critical: Not allowed
- **Restricted** → Public, Secure, Restricted: Allowed
- **Restricted** → Critical: Not allowed
- **Critical** → Any zone: Allowed

---

## Additional Resources

- **Implementation Plan:** `docs/user-journey-implementation-plan.md`
- **Seed Data:** `Scripts/SeedSecurityZoneMappings.sql`
- **Code Location:** `Shared/Repositories/Repository/Analytics/UserJourneyRepository.cs`
