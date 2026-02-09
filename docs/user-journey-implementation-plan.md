# User Journey & Security Check Implementation Plan

**Created:** 2025-02-08
**Context:** BLE Tracking Backend - Bank Indonesia
**Features:** User Journey Analytics + Security Check

---

## 🎯 Objective

Implement **User Journey Analytics** and **Security Check** features to:

1. Track multi-area journeys across visitors
2. Identify common paths through facility
3. Detect unusual/suspicious journeys (security)
4. Provide next area predictions

---

## 📊 Current State

### ✅ What Exists

- **VisitorSession** entity with PersonId, AreaName, EnterTime, ExitTime, DurationInMinutes
- **TrackingSessionService** - already returns sessions per person
- **VisualPaths** - X,Y coordinates for floorplan visualization
- Can query individual journey: `WHERE PersonId='xxx' ORDER BY EnterTime`

### ❌ What's Missing

- No aggregation across multiple visitors (common paths)
- No security validation for unusual routes
- No zone crossing detection
- No next area prediction

---

## 🎯 Features to Implement

### 1. Common Paths Analysis

**Endpoint:** `POST /api/TrackingAnalytics/user-journey/common-paths`

**Purpose:** Discover most popular multi-area journeys

**Example Response:**

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
    },
    {
      "pathId": "lobby-server-room",
      "areaSequence": ["Lobby", "Security Check", "Server Room"],
      "journeyCount": 3,
      "percentage": 0.16,
      "avgDurationMinutes": 15.0,
      "isAnomaly": true,
      "riskLevel": "High"
    }
  ]
}
```

### 2. Security Journey Check

**Endpoint:** `POST /api/TrackingAnalytics/user-journey/security-check`

**Purpose:** Validate journey for security violations

**Example Response:**

```json
{
  "visitorId": "abc-123",
  "visitorName": "John Doe",
  "pathTaken": ["Lobby", "Security Check", "Server Room"],
  "riskLevel": "High",
  "requiresEscort": true,
  "violations": [
    {
      "type": "Unauthorized Zone Access",
      "areaName": "Server Room",
      "timestamp": "2025-02-08T14:30:00",
      "description": "Server Room requires prior authorization"
    }
  ]
}
```

### 3. Next Area Prediction

**Endpoint:** `POST /api/TrackingAnalytics/user-journey/next-areas`

**Purpose:** Given current area, predict next likely areas

**Example Response:**

```json
{
  "currentArea": "Lobby",
  "nextAreas": [
    { "areaName": "Security Check", "probability": 0.65, "count": 456 },
    { "areaName": "Banking Hall", "probability": 0.25, "count": 178 },
    { "areaName": "Cafeteria", "probability": 0.1, "count": 67 }
  ]
}
```

---

## 📁 Files to Create/Modify

### Phase 1: DTOs & Contracts

**File 1: Create UserJourneyRead.cs**

```
Shared/Shared.Contracts/Analytics/UserJourneyRead.cs
```

**Classes to create:**

```csharp
namespace Shared.Contracts.Analytics
{
    // Common Paths
    public class CommonPathRead
    {
        public string PathId { get; set; }
        public List<string> AreaSequence { get; set; }
        public int JourneyCount { get; set; }
        public double Percentage { get; set; }
        public double AvgDurationMinutes { get; set; }
        public List<string> EntryPoints { get; set; }
        public List<string> ExitPoints { get; set; }
        public bool IsAnomaly { get; set; }
        public string? RiskLevel { get; set; }
    }

    // Security Check
    public class SecurityJourneyCheckRead
    {
        public Guid? VisitorId { get; set; }
        public string? VisitorName { get; set; }
        public List<string> PathTaken { get; set; }
        public string RiskLevel { get; set; }
        public bool RequiresEscort { get; set; }
        public List<SecurityViolation> Violations { get; set; }
    }

    public class SecurityViolation
    {
        public string Type { get; set; }
        public string AreaName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
    }

    // Next Area Prediction
    public class NextAreaPredictionRead
    {
        public string CurrentArea { get; set; }
        public List<AreaProbability> NextAreas { get; set; }
        public int TotalFromArea { get; set; }
    }

    public class AreaProbability
    {
        public string AreaName { get; set; }
        public double Probability { get; set; }
        public int Count { get; set; }
        public double AvgTimeToReach { get; set; }
    }

    // Journey Request Filter
    public class UserJourneyFilter
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? TimeRange { get; set; }
        public Guid? BuildingId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? CurrentAreaId { get; set; }
        public int? MinJourneyLength { get; set; } // Min areas to visit
        public int? MaxResults { get; set; } // Limit results
    }
}
```

**File 2: Create SecurityZoneMapping (for database)**

```
Shared/Entities.Models/SecurityZoneMapping.cs
```

```csharp
namespace Entities.Models
{
    public class SecurityZoneMapping : BaseModelWithTimeApp
    {
        public Guid AreaId { get; set; }
        public string AreaName { get; set; }
        public SecurityZone SecurityZone { get; set; }
        public bool RequiresEscort { get; set; }
        public string AllowedFromZones { get; set; } // JSON array
    }

    public enum SecurityZone
    {
        Public = 1,       // Lobby, Banking Hall, Cafeteria
        Secure = 2,       // Meeting Rooms, Office Areas
        Restricted = 3,   // Server Room, Vault
        Critical = 4      // Data Center, Cash Handling
    }
}
```

**File 3: Update DbContext**

```
Shared/Repositories/DbContexts/BleTrackingDbContext.cs
```

Add:

```csharp
public DbSet<SecurityZoneMapping> SecurityZoneMappings { get; set; }
```

---

### Phase 2: Repository Layer

**File 4: Create UserJourneyRepository.cs**

```
Shared/Repositories/Repository/Analytics/UserJourneyRepository.cs
```

**Structure:**

```csharp
using Shared.Contracts.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository.Base;

namespace Repositories.Repository.Analytics
{
    public class UserJourneyRepository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;

        public UserJourneyRepository(BleTrackingDbContext context, IHttpContextAccessor accessor)
            : base(context, accessor)
        {
            _context = context;
        }

        /// <summary>
        /// Get common paths - aggregate journeys across all visitors
        /// </summary>
        public async Task<List<CommonPathRead>> GetCommonPathsAsync(UserJourneyFilter filter)
        {
            // SQL Logic:
            // 1. Get all sessions in date range
            // 2. Group by PersonId, order by EnterTime
            // 3. Create path sequence (area1→area2→area3)
            // 4. Group by path sequence, count occurrences
            // 5. Return top N paths

            var from = filter.From ?? DateTime.UtcNow.AddDays(-7);
            var to = filter.To ?? DateTime.UtcNow;

            // Step 1: Get raw sessions
            var sessions = await _context.Set<VisitorSession>() // atau dari table tracking transaction
                .Where(s => s.EnterTime >= from && s.EnterTime <= to)
                .Where(s => !filter.BuildingId.HasValue || s.BuildingId == filter.BuildingId.Value)
                .OrderBy(s => s.EnterTime)
                .ToListAsync();

            // Step 2: Group by person and create path sequences
            var journeys = sessions
                .GroupBy(s => s.PersonId)
                .Select(g => new
                {
                    PersonId = g.Key,
                    AreaSequence = g.OrderBy(s => s.EnterTime)
                        .Select(s => s.AreaName)
                        .ToList(),
                    Duration = g.Sum(s => s.DurationInMinutes ?? 0),
                    FirstArea = g.OrderBy(s => s.EnterTime).First().AreaName,
                    LastArea = g.OrderByDescending(s => s.EnterTime).First().AreaName
                })
                .Where(j => j.AreaSequence.Count >= (filter.MinJourneyLength ?? 2))
                .ToList();

            // Step 3: Group by path and aggregate
            var totalJourneys = journeys.Count;
            var commonPaths = journeys
                .GroupBy(j => string.Join("→", j.AreaSequence))
                .Select(g => new CommonPathRead
                {
                    PathId = g.Key.ToLower().Replace(" ", "-"),
                    AreaSequence = g.First().AreaSequence,
                    JourneyCount = g.Count(),
                    Percentage = (g.Count() * 100.0) / totalJourneys,
                    AvgDurationMinutes = g.Average(j => j.Duration),
                    EntryPoints = g.Select(j => j.FirstArea).Distinct().ToList(),
                    ExitPoints = g.Select(j => j.LastArea).Distinct().ToList(),
                    IsAnomaly = g.Count() < 5, // Threshold: < 5 occurrences = anomaly
                    RiskLevel = g.Count() < 3 ? "High" : g.Count() < 10 ? "Medium" : "Low"
                })
                .OrderByDescending(p => p.JourneyCount)
                .Take(filter.MaxResults ?? 20)
                .ToList();

            return commonPaths;
        }

        /// <summary>
        /// Security check - validate journey for violations
        /// </summary>
        public async Task<SecurityJourneyCheckRead> GetSecurityCheckAsync(Guid visitorId, DateTime from, DateTime to)
        {
            // Get visitor sessions
            var sessions = await _context.Set<VisitorSession>()
                .Where(s => s.PersonId == visitorId)
                .Where(s => s.EnterTime >= from && s.EnterTime <= to)
                .OrderBy(s => s.EnterTime)
                .ToListAsync();

            // Get security zone mappings
            var zoneMappings = await _context.SecurityZoneMappings
                .Where(z => z.Status != 0)
                .ToDictionaryAsync(z => z.AreaId, z => z);

            // Validate each transition
            var violations = new List<SecurityViolation>();
            var pathTaken = sessions.Select(s => s.AreaName).ToList();

            for (int i = 0; i < sessions.Count; i++)
            {
                var currentSession = sessions[i];

                if (!zoneMappings.ContainsKey(currentSession.AreaId ?? Guid.Empty))
                    continue;

                var currentZone = zoneMappings[currentSession.AreaId.Value];
                var prevZone = i > 0 && zoneMappings.ContainsKey(sessions[i-1].AreaId ?? Guid.Empty)
                    ? zoneMappings[sessions[i-1].AreaId.Value].SecurityZone
                    : SecurityZone.Public;

                // Check if transition is allowed
                if (currentZone.SecurityZone > SecurityZone.Public &&
                    currentZone.RequiresEscort)
                {
                    violations.Add(new SecurityViolation
                    {
                        Type = "Unauthorized Zone Access",
                        AreaName = currentSession.AreaName,
                        Timestamp = currentSession.EnterTime,
                        Description = $"{currentSession.AreaName} requires prior authorization"
                    });
                }
            }

            // Determine risk level
            string riskLevel = violations.Count == 0 ? "Low"
                : violations.Any(v => v.Type.Contains("Critical")) ? "Critical"
                : violations.Any(v => v.Type.Contains("Restricted")) ? "High"
                : "Medium";

            return new SecurityJourneyCheckRead
            {
                VisitorId = visitorId,
                VisitorName = sessions.FirstOrDefault()?.PersonName,
                PathTaken = pathTaken,
                RiskLevel = riskLevel,
                RequiresEscort = violations.Count > 0,
                Violations = violations
            };
        }

        /// <summary>
        /// Next area prediction - probability based on historical data
        /// </summary>
        public async Task<NextAreaPredictionRead> GetNextAreasAsync(Guid currentAreaId, UserJourneyFilter filter)
        {
            var from = filter.From ?? DateTime.UtcNow.AddDays(-30);
            var to = filter.To ?? DateTime.UtcNow;

            // Get all sessions in the area
            var sessionsInArea = await _context.Set<VisitorSession>()
                .Where(s => s.AreaId == currentAreaId)
                .Where(s => s.EnterTime >= from && s.EnterTime <= to)
                .ToListAsync();

            // Find next area for each person
            var transitions = new List<(string? NextArea, int TransitionMinutes)>();

            foreach (var personId in sessionsInArea.Select(s => s.PersonId).Distinct())
            {
                var personSessions = sessionsInArea
                    .Where(s => s.PersonId == personId)
                    .OrderBy(s => s.EnterTime)
                    .ToList();

                for (int i = 0; i < personSessions.Count; i++)
                {
                    var currentExit = personSessions[i].ExitTime;

                    // Find next session after current (within 30 min)
                    var nextSession = await _context.Set<VisitorSession>()
                        .Where(s => s.PersonId == personId)
                        .Where(s => s.EnterTime > currentExit)
                        .Where(s => s.EnterTime <= currentExit.AddMinutes(30))
                        .OrderBy(s => s.EnterTime)
                        .FirstOrDefaultAsync();

                    if (nextSession != null)
                    {
                        transitions.Add((nextSession.AreaName,
                            (int)(nextSession.EnterTime - currentExit).TotalMinutes));
                    }
                }
            }

            // Aggregate by next area
            var totalTransitions = transitions.Count;
            var nextAreas = transitions
                .GroupBy(t => t.NextArea)
                .Select(g => new AreaProbability
                {
                    AreaName = g.Key ?? "Unknown",
                    Probability = (double)g.Count() / totalTransitions,
                    Count = g.Count(),
                    AvgTimeToReach = g.Average(t => t.TransitionMinutes)
                })
                .OrderByDescending(a => a.Probability)
                .Take(5)
                .ToList();

            return new NextAreaPredictionRead
            {
                CurrentArea = (await _context.Set<VisitorSession>()
                    .Where(s => s.AreaId == currentAreaId)
                    .Select(s => s.AreaName)
                    .FirstOrDefaultAsync()) ?? "Unknown",
                NextAreas = nextAreas,
                TotalFromArea = totalTransitions
            };
        }
    }
}
```

---

### Phase 3: Service Layer

**File 5: Create UserJourneyService.cs**

```
Shared/BusinessLogic.Services/Implementation/Analytics/UserJourneyService.cs
```

```csharp
using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels.ResponseHelper;
using Microsoft.Extensions.Logging;
using Repositories.Repository.Analytics;
using Shared.Contracts.Analytics;
using System;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class UserJourneyService : BaseService, IUserJourneyService
    {
        private readonly UserJourneyRepository _repository;
        private readonly ILogger<UserJourneyService> _logger;

        public UserJourneyService(
            UserJourneyRepository repository,
            IHttpContextAccessor http,
            ILogger<UserJourneyService> logger) : base(http)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<object> GetCommonPathsAsync(UserJourneyFilter filter)
        {
            try
            {
                var data = await _repository.GetCommonPathsAsync(filter);
                return ApiResponse.Success("Common paths retrieved successfully", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting common paths");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetSecurityCheckAsync(Guid visitorId, UserJourneyFilter filter)
        {
            try
            {
                var from = filter.From ?? DateTime.UtcNow.AddDays(-1);
                var to = filter.To ?? DateTime.UtcNow;

                var result = await _repository.GetSecurityCheckAsync(visitorId, from, to);

                return ApiResponse.Success("Security check completed", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in security check for visitor {VisitorId}", visitorId);
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetNextAreasAsync(Guid currentAreaId, UserJourneyFilter filter)
        {
            try
            {
                var result = await _repository.GetNextAreasAsync(currentAreaId, filter);
                return ApiResponse.Success("Next areas prediction retrieved", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next areas for area {AreaId}", currentAreaId);
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }
    }
}
```

**File 6: Create Interface**

```
Shared/BusinessLogic.Services/Interface/Analytics/IUserJourneyService.cs
```

```csharp
using System;
using System.Threading.Tasks;
using Shared.Contracts.Analytics;

namespace BusinessLogic.Services.Interface.Analytics
{
    public interface IUserJourneyService
    {
        Task<object> GetCommonPathsAsync(UserJourneyFilter filter);
        Task<object> GetSecurityCheckAsync(Guid visitorId, UserJourneyFilter filter);
        Task<object> GetNextAreasAsync(Guid currentAreaId, UserJourneyFilter filter);
    }
}
```

---

### Phase 4: Controller Layer

**File 7: Create UserJourneyController.cs**

```
Shared/Web.API.Controllers/Controllers/Analytics/UserJourneyController.cs
```

```csharp
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Interface.Analytics;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Analytics;
using System;
using System.Threading.Tasks;

namespace Web.API.Controllers.Controllers.Analytics
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserJourneyController : ControllerBase
    {
        private readonly IUserJourneyService _service;

        public UserJourneyController(IUserJourneyService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get common paths - most popular multi-area journeys
        /// </summary>
        [HttpPost("common-paths")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<object> GetCommonPaths([FromBody] UserJourneyFilter filter)
        {
            return await _service.GetCommonPathsAsync(filter);
        }

        /// <summary>
        /// Security check - validate journey for violations
        /// </summary>
        [HttpPost("security-check/{visitorId}")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<object> GetSecurityCheck(
            Guid visitorId,
            [FromBody] UserJourneyFilter filter)
        {
            return await _service.GetSecurityCheckAsync(visitorId, filter);
        }

        /// <summary>
        /// Next area prediction - probability of next area from current area
        /// </summary>
        [HttpPost("next-areas/{currentAreaId}")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<object> GetNextAreas(
            Guid currentAreaId,
            [FromBody] UserJourneyFilter filter)
        {
            return await _service.GetNextAreasAsync(currentAreaId, filter);
        }
    }
}
```

---

### Phase 5: Database Migration

**Step 1: Create SecurityZoneMapping entity**

**Step 2: Add migration**

```bash
dotnet ef migrations add AddSecurityZoneMapping \
  --project Shared/Repositories/Repositories.csproj \
  --startup-project Services.API/Auth/Auth.csproj
```

**Step 3: Seed security zone data**

```sql
INSERT INTO SecurityZoneMapping (Id, AreaId, AreaName, SecurityZone, RequiresEscort, CreatedAt, UpdatedAt, Status)
VALUES
(NEWID(), 'lobby-id', 'Lobby', 1, 0, GETDATE(), GETDATE(), 1),
(NEWID(), 'banking-hall-id', 'Banking Hall', 1, 0, GETDATE(), GETDATE(), 1),
(NEWID(), 'cafeteria-id', 'Cafeteria', 1, 0, GETDATE(), GETDATE(), 1),
(NEWID(), 'meeting-room-id', 'Meeting Room', 2, 0, GETDATE(), GETDATE(), 1),
(NEWID(), 'office-area-id', 'Office Area', 2, 0, GETDATE(), GETDATE(), 1),
(NEWID(), 'server-room-id', 'Server Room', 3, 1, GETDATE(), GETDATE(), 1),
(NEWID(), 'vault-id', 'Vault Area', 3, 1, GETDATE(), GETDATE(), 1);
```

---

## 🚀 Implementation Steps

### Week 1: Foundation

**Day 1-2: DTOs & Contracts**

- [ ] Create `UserJourneyRead.cs` with all DTOs
- [ ] Create `SecurityZoneMapping.cs` entity
- [ ] Update `BleTrackingDbContext.cs` with new DbSet

**Day 3: Database**

- [ ] Add migration for SecurityZoneMapping
- [ ] Seed initial security zone data
- [ ] Test zone mapping CRUD

**Day 4-5: Repository Layer**

- [ ] Create `UserJourneyRepository.cs`
- [ ] Implement `GetCommonPathsAsync()`
- [ ] Implement `GetSecurityCheckAsync()`
- [ ] Implement `GetNextAreasAsync()`
- [ ] Write unit tests

### Week 2: Service & Controller

**Day 1-2: Service Layer**

- [ ] Create `IUserJourneyService.cs` interface
- [ ] Create `UserJourneyService.cs` implementation
- [ ] Add error handling and logging

**Day 3-4: Controller Layer**

- [ ] Create `UserJourneyController.cs`
- [ ] Add `[MinLevel]` authorization
- [ ] Add Swagger documentation

**Day 5: Integration Testing**

- [ ] Test common paths endpoint
- [ ] Test security check endpoint
- [ ] Test next areas endpoint
- [ ] Load testing with 10K+ sessions

### Week 3: Polish & Deploy

**Day 1-2: Performance Optimization**

- [ ] Add caching for common paths (Redis)
- [ ] Optimize SQL queries
- [ ] Add database indexes

**Day 3: Documentation**

- [ ] Update API documentation
- [ ] Create usage examples
- [ ] Write troubleshooting guide

**Day 4-5: UAT & Deploy**

- [ ] User acceptance testing
- [ ] Bug fixes
- [ ] Deploy to staging
- [ ] Production deployment

---

## 🧪 Testing Plan

### Unit Tests

**Test 1: Common Paths - Basic**

```
Input: 100 visitors, 2 paths (Lobby→Banking, Lobby→Cafe)
Expected: Returns 2 paths with correct counts and percentages
```

**Test 2: Common Paths - Anomaly Detection**

```
Input: 1 visitor goes to Server Room
Expected: IsAnomaly=true, RiskLevel="High"
```

**Test 3: Security Check - Clean Path**

```
Input: John goes Lobby→Banking Hall→Cafeteria
Expected: RiskLevel="Low", Violations=[]
```

**Test 4: Security Check - Violation**

```
Input: John goes Lobby→Server Room
Expected: RiskLevel="High", RequiresEscort=true
```

**Test 5: Next Area - Prediction**

```
Input: Current="Lobby", historical data shows 65%→Security
Expected: Security Check with probability 0.65
```

### Integration Tests

**Test API Endpoints:**

```bash
# Common Paths
curl -X POST http://localhost:5001/api/UserJourney/common-paths \
  -H "Content-Type: application/json" \
  -d '{"from":"2025-02-01","to":"2025-02-08","minJourneyLength":2}'

# Security Check
curl -X POST http://localhost:5001/api/UserJourney/security-check/abc-123 \
  -H "Content-Type: application/json" \
  -d '{"from":"2025-02-08","to":"2025-02-08"}'

# Next Areas
curl -X POST http://localhost:5001/api/UserJourney/next-areas/lobby-id \
  -H "Content-Type: application/json" \
  -d '{"from":"2025-02-01","to":"2025-02-08"}'
```

---

## 📊 Success Criteria

### Functional Requirements

- ✅ Common paths returns top 20 journeys with counts
- ✅ Security check detects zone violations
- ✅ Next area prediction returns probabilities
- ✅ All endpoints support date range filtering
- ✅ Anomaly detection flags rare paths (< 5 occurrences)

### Performance Requirements

- ✅ Common paths query < 3 seconds (100K sessions)
- ✅ Security check < 500ms per visitor
- ✅ Next area prediction < 2 seconds
- ✅ Support concurrent 100 users

### Security Requirements

- ✅ All endpoints require PrimaryAdmin+ role
- ✅ Audit logging for security checks
- ✅ Zone validation uses database configuration
- ✅ No SQL injection vulnerabilities

---

## 🚨 Risks & Mitigations

### Risk 1: Performance with Large Data

**Impact:** Slow queries with 100K+ sessions
**Mitigation:**

- Add database indexes on (PersonId, EnterTime, AreaId)
- Cache common paths for 1 hour (Redis)
- Use pagination for large results

### Risk 2: Zone Configuration

**Impact:** Wrong zone mapping = false alerts
**Mitigation:**

- Admin UI for zone management
- Zone validation before deployment
- Test with known good paths

### Risk 3: Data Quality

**Impact:** Missing/inaccurate session data
**Mitigation:**

- Validate session data before aggregation
- Log sessions with missing zones
- Provide data quality dashboard

---

## 📈 Future Enhancements

**Phase 2:**

- Journey funnel analysis
- Bottleneck detection
- Staff scheduling optimization

**Phase 3:**

- Real-time journey tracking
- Anomaly detection (ML-based)
- Zone capacity alerts

---

## ✅ Checklist

Before implementation:

- [ ] Review plan with stakeholders
- [ ] Confirm security zone mappings
- [ ] Define anomaly thresholds
- [ ] Prepare test data

During implementation:

- [ ] Follow repository pattern (return Read DTOs)
- [ ] Use direct return in service (no AutoMapper)
- [ ] Add MinLevel authorization
- [ ] Include audit logging

After implementation:

- [ ] Load testing (10K+ concurrent users)
- [ ] Security testing
- [ ] UAT with Bank Indonesia team
- [ ] Documentation handover

---

**Ready to execute? Review and confirm!**

Tiap Field
Field: TotalSeconds
Tipe: double?
Pengertian: Durasi total insiden dari event pertama sampai event terakhir (dalam detik).
────────────────────────────────────────
Field: TotalFormatted
Tipe: string
Pengertian: Durasi total dalam format human-readable: "2 hours 30 minutes", "15 seconds", dll.
────────────────────────────────────────
Field: ResponseTimeSeconds
Tipe: double?
Pengertian: Response Time — waktu dari Triggered sampai first action (Acknowledged/Dispatched/Investigated). Mengukur seberapa cepat sistem merespon alarm.
────────────────────────────────────────
Field: ResponseTimeFormatted
Tipe: string
Pengertian: Response time dalam format bacaan.
────────────────────────────────────────
Field: ResolutionTimeSeconds
Tipe: double?
Pengertian: Resolution Time — waktu dari first action sampai event terakhir (Done/Cancelled). Mengukur seberapa lama proses penyelesaian.
────────────────────────────────────────
Field: ResolutionTimeFormatted
Tipe: string
Pengertian: Resolution time dalam format bacaan.
