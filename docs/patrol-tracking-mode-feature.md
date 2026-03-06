# Patrol Tracking Mode Feature - Implementation Plan

## Overview

This document outlines the implementation of a **Patrol Tracking Mode** feature that allows applications to switch between **Manual** checkpoint submission (current behavior) and **Auto** tracking by BLE engine (new behavior).

---

## Problem Statement

Currently, the patrol system requires security guards to **manually submit** each checkpoint via `POST /api/PatrolSession/checkpoint-action`. The system enforces:
- Sequence validation (no skipping checkpoints)
- Dwell time validation (minimum stay requirement)

User wants to restore the **Auto Tracking** mode where:
- Node.js BLE engine directly updates `arrived_at` and `left_at` in `patrol_checkpoint_log` table
- No manual submission required
- No sequence validation
- Checkpoint timeline is the primary data (not status)

---

## Solution

Add a **PatrolTrackingMode** setting to `MstApplication` entity:

| Mode | Value | Behavior |
|------|-------|----------|
| **Manual** | 0 | Security submits checkpoints via API (default, current behavior) |
| **Auto** | 1 | BLE engine tracks automatically, manual submission blocked |

---

## Database Changes

### New Column

**Table:** `mst_application`

**Column:**
```sql
ALTER TABLE mst_application
ADD COLUMN patrol_tracking_mode INT NOT NULL DEFAULT 0;
-- 0 = Manual, 1 = Auto
```

### Migration Command

```bash
dotnet ef migrations add AddPatrolTrackingModeToMstApplication \
  --project Shared/Repositories/Repositories.csproj \
  --startup-project Services.API/MstApplication/MstApplication.csproj

dotnet ef database update \
  --project Shared/Repositories/Repositories.csproj \
  --startup-project Services.API/MstApplication/MstApplication.csproj
```

---

## Code Changes

### 1. New Enum: `PatrolTrackingMode`

**File:** `Shared/Shared.Contracts/Enum.cs`

**Location:** After `PatrolCycleType` enum (around line 423)

```csharp
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PatrolTrackingMode
{
    Manual = 0,  // Default - requires manual checkpoint submission
    Auto = 1     // BLE engine tracks automatically
}
```

---

### 2. Entity: `MstApplication`

**File:** `Shared/Entities.Models/MstApplication.cs`

**Location:** After `ApplicationStatus` property (around line 86)

```csharp
[Column("patrol_tracking_mode")]
public PatrolTrackingMode PatrolTrackingMode { get; set; } = PatrolTrackingMode.Manual;
```

---

### 3. DTOs: `MstApplicationDto`

**File:** `Shared/Data.ViewModels/MstApplicationDto.cs`

Add to all three DTOs:

```csharp
// MstApplicationDto (after ApplicationStatus)
public PatrolTrackingMode PatrolTrackingMode { get; set; } = PatrolTrackingMode.Manual;

// MstApplicationCreateDto (after LicenseType)
public PatrolTrackingMode PatrolTrackingMode { get; set; } = PatrolTrackingMode.Manual;

// MstApplicationUpdateDto (after LicenseType)
public PatrolTrackingMode PatrolTrackingMode { get; set; } = PatrolTrackingMode.Manual;
```

---

### 4. Read DTO: `MstApplicationRead`

**File:** `Shared/Shared.Contracts/Read/MstApplicationRead.cs`

**Location:** After `ApplicationStatus` (around line 43)

```csharp
[JsonPropertyOrder(17)]
public string PatrolTrackingMode { get; set; }
```

---

### 5. Repository: `MstApplicationRepository`

**File:** `Shared/Repositories/Repository/MstApplicationRepository.cs`

**Location:** In `ProjectToRead` method (line 32-55), add to projection:

```csharp
private IQueryable<MstApplicationRead> ProjectToRead(IQueryable<MstApplication> query)
{
    return query.AsNoTracking().Select(x => new MstApplicationRead
    {
        // ... existing fields ...
        ApplicationStatus = x.ApplicationStatus ?? 1,
        PatrolTrackingMode = x.PatrolTrackingMode.ToString()  // Add this
    });
}
```

---

### 6. Mapper: `MstApplicationProfile`

**File:** `Shared/BusinessLogic.Services/Extension/MstApplicationProfile.cs`

Update all three mappings:

```csharp
public class MstApplicationProfile : Profile
{
    public MstApplicationProfile()
    {
        CreateMap<MstApplication, MstApplicationDto>()
            .ForMember(dest => dest.ApplicationStatus, opt => opt.MapFrom(src => src.ApplicationStatus))
            .ForMember(dest => dest.PatrolTrackingMode, opt => opt.MapFrom(src => src.PatrolTrackingMode));

        CreateMap<MstApplicationCreateDto, MstApplication>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Generate, opt => opt.Ignore())
            .ForMember(dest => dest.ApplicationRegistered, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.PatrolTrackingMode, opt => opt.MapFrom(src => src.PatrolTrackingMode));

        CreateMap<MstApplicationUpdateDto, MstApplication>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Generate, opt => opt.Ignore())
            .ForMember(dest => dest.ApplicationRegistered, opt => opt.Ignore())
            .ForSourceMember(src => src.PatrolTrackingMode, opt => opt.DoNotValidation());
    }
}
```

---

### 7. Service: `PatrolSessionService`

**File:** `Shared/BusinessLogic.Services/Implementation/PatrolSessionService.cs`

#### 7.1 Add Dependency

Inject `IMstApplicationService` in constructor:

```csharp
private readonly IMstApplicationService _applicationService;

public PatrolSessionService(
    PatrolSessionRepository repo,
    IPatrolCaseService caseService,
    IAuditEmitter audit,
    IMqttPubQueue mqttQueue,
    IMstApplicationService applicationService,  // Add this
    IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
{
    _repo = repo;
    _caseService = caseService;
    _audit = audit;
    _mqttQueue = mqttQueue;
    _applicationService = applicationService;  // Add this
}
```

#### 7.2 Add Helper Method

Add after `StopAsync` method (around line 329):

```csharp
private async Task<PatrolTrackingMode> GetPatrolTrackingModeAsync()
{
    var application = await _applicationService.GetApplicationByIdAsync(AppId);
    if (application == null)
        return PatrolTrackingMode.Manual;

    if (Enum.TryParse<PatrolTrackingMode>(application.PatrolTrackingMode, out var mode))
        return mode;

    return PatrolTrackingMode.Manual;
}
```

#### 7.3 Modify `SubmitCheckpointActionAsync`

Add Auto mode check at the beginning:

```csharp
public async Task<object> SubmitCheckpointActionAsync(PatrolCheckpointActionDto dto)
{
    // 0. Check if application is in Auto mode - manual submission not allowed
    var trackingMode = await GetPatrolTrackingModeAsync();
    if (trackingMode == PatrolTrackingMode.Auto)
    {
        throw new BusinessException(
            "Manual checkpoint submission is not allowed in Auto tracking mode. " +
            "The BLE tracking engine handles checkpoint detection automatically.");
    }

    // 1. Validate active checkpoint presence
    var log = await _repo.GetActiveCheckpointLogAsync(dto.PatrolCheckpointLogId, dto.PatrolAreaId);
    // ... rest of method unchanged
}
```

---

## Files Summary

| # | File | Change |
|---|------|--------|
| 1 | `Shared/Shared.Contracts/Enum.cs` | Add `PatrolTrackingMode` enum |
| 2 | `Shared/Entities.Models/MstApplication.cs` | Add `PatrolTrackingMode` property |
| 3 | `Shared/Data.ViewModels/MstApplicationDto.cs` | Add to 3 DTOs |
| 4 | `Shared/Shared.Contracts/Read/MstApplicationRead.cs` | Add to Read DTO |
| 5 | `Shared/Repositories/Repository/MstApplicationRepository.cs` | Update projection |
| 6 | `Shared/BusinessLogic.Services/Extension/MstApplicationProfile.cs` | Update mapper |
| 7 | `Shared/BusinessLogic.Services/Implementation/PatrolSessionService.cs` | Add mode check |

---

## Behavior Comparison

### Manual Mode (Current - Default)

| Aspect | Behavior |
|--------|----------|
| Checkpoint Submission | Required via `POST /api/PatrolSession/checkpoint-action` |
| Sequence Validation | Enforced (no skipping) |
| Dwell Time Validation | Enforced if `DurationType = WithDuration` |
| Status Changes | AutoDetected → Cleared/HasCase |
| MQTT Events | `patrol/checkpoint/cleared` on submit |

### Auto Mode (New)

| Aspect | Behavior |
|--------|----------|
| Checkpoint Submission | Blocked - returns error |
| Sequence Validation | None (free order) |
| Dwell Time Validation | Not enforced |
| Status | Not important (AutoDetected stays) |
| Timeline | `arrived_at`, `left_at` updated by Node.js engine directly |
| MQTT Events | Frontend consumes engine events directly |

---

## API Changes

### Update Application Tracking Mode

**Endpoint:** `PUT /api/MstApplication/{id}`

**Request Body:**
```json
{
  "applicationName": "Building A",
  "organizationType": "Single",
  // ... other fields ...
  "patrolTrackingMode": 1  // 0 = Manual, 1 = Auto
}
```

### Get Application (Returns Mode)

**Endpoint:** `GET /api/MstApplication/{id}`

**Response:**
```json
{
  "id": "guid",
  "applicationName": "Building A",
  // ... other fields ...
  "patrolTrackingMode": "Auto"
}
```

### Checkpoint Submission (Auto Mode Blocked)

**Endpoint:** `POST /api/PatrolSession/checkpoint-action`

**Error Response (in Auto mode):**
```json
{
  "message": "Manual checkpoint submission is not allowed in Auto tracking mode. The BLE tracking engine handles checkpoint detection automatically."
}
```

---

## Testing Checklist

- [ ] Migration applies successfully to database
- [ ] Creating a new application defaults to Manual mode (0)
- [ ] Updating application's PatrolTrackingMode works
- [ ] **Manual Mode**: Checkpoint submission works normally
- [ ] **Manual Mode**: Sequence validation enforced
- [ ] **Manual Mode**: Dwell time validation enforced
- [ ] **Auto Mode**: Checkpoint submission blocked with error message
- [ ] **Auto Mode**: Node.js engine can update `arrived_at`/`left_at`
- [ ] Mode switch during active session handled correctly
- [ ] Read DTO returns PatrolTrackingMode as string
- [ ] Filter and pagination still work for MstApplication

---

## Edge Cases

### Existing Applications
- **Issue**: Existing applications have `NULL` value
- **Solution**: Migration sets default to `0` (Manual), backward compatible

### Mode Switch During Active Session
- **Issue**: What if mode changes during active patrol?
- **Solution**: Checked on each submission. Manual submissions blocked after switch to Auto.

### Auto Mode + Case Reporting
- **Issue**: How to report cases in Auto mode?
- **Solution**: Cases created via separate `PatrolCase` API, not tied to checkpoint submission.

### Session Stop in Auto Mode
- **Issue**: How to stop session in Auto mode?
- **Solution**: Still via `POST /api/PatrolSession/{id}/stop`. AutoDetected checkpoints marked as Missed.

---

## Related Documents

- `PATROL_TRACKING_ENGINE_INTEGRATION.md` - Node.js engine integration
- `CLAUDE.md` - Coding standards and patterns
- `REFACTORING_GUIDE.md` - Repository patterns

---

## Service Registration (No Changes Needed)

The `IMstApplicationService` is already registered in the DI container. No additional configuration needed.

**Note:** Ensure `PatrolSessionService` is registered after adding the new dependency.
