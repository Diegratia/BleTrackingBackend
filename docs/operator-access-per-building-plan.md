# Operator Access per Building - Implementation Plan

## Problem Statement

**Requirement**: Operator (PrimaryAdmin role) harus bisa melihat data monitoring HANYA untuk building yang mereka akses.

**Current Issue**:
- Semua user dengan role PrimaryAdmin bisa melihat data dari SEMUA building dalam Application mereka
- Tidak ada mechanism untuk membatasi akses per building
- Data monitoring (TrackingTransaction, AlarmRecordTracking, Patrol, dll) sudah punya Building hierarchy tapi tanpa access control

---

## Current State Analysis

### Entity Structure

```
User
├── GroupId → UserGroup (LevelPriority: PrimaryAdmin = "Operator")
├── ApplicationId (multi-tenancy)
└── ❌ Tidak ada relasi ke Building

MstBuilding
├── ApplicationId
├── Name
└── ❌ Tidak ada relasi ke User/UserGroup

Monitoring Data Hierarchy:
MstBuilding
└── MstFloor
    └── MstFloorplan
        └── FloorplanMaskedArea
            ├── TrackingTransaction
            ├── AlarmRecordTracking
            └── PatrolArea
```

**Key Finding**: Semua monitoring data bisa ditelusuri ke Building melalui FloorplanMaskedArea → Floorplan → Floor → Building chain.

### Current Access Control

- **MinLevel Authorization**: Global per Application, tidak ada building-level restriction
- **Multi-tenancy**: ApplicationId filtering saja
- **Analytics Controller**: `[MinLevel(LevelPriority.Primary)]` - semua PrimaryAdmin lihat SEMUA data

**Example Problem**:
```
User A (PrimaryAdmin, ApplicationId = APP1)
├── Bisa lihat data dari Building X, Y, Z (semua di APP1) ❌

Padahal harusnya:
User A (PrimaryAdmin, ApplicationId = APP1)
├── Hanya bisa lihat data dari Building X & Y saja ✅
```

---

## Solution Architecture

### 1. Create Junction Table: UserBuildingAccess

**Entity**: `UserBuildingAccess` (Many-to-Many relationship)

```csharp
[Table("user_building_access")]
public class UserBuildingAccess : BaseModelWithTimeApp, IApplicationEntity
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("building_id")]
    public Guid BuildingId { get; set; }

    [Column("application_id")]
    public Guid ApplicationId { get; set; }

    // Navigation properties
    public User User { get; set; }
    public MstBuilding Building { get; set; }
}
```

**Rationale**:
- ✅ Mendukung multiple building per user (fleksibel)
- ✅ Include audit fields (CreatedAt, UpdatedAt, etc.)
- ✅ Include ApplicationId (multi-tenancy)
- ✅ Soft delete support (Status field)

### 2. Extend BaseRepository with Building Filter

**Method**: `ApplyBuildingFilter()` di `BaseRepository.cs`

```csharp
protected IQueryable<T> ApplyBuildingFilter<T>(
    IQueryable<T> query,
    IEnumerable<Guid> accessibleBuildingIds
) where T : class
{
    // Filter by accessible buildings
    query = query.Where(entity =>
        accessibleBuildingIds.Contains(GetBuildingId(entity))
    );
    return query;
}

private Guid GetBuildingId<T>(T entity)
{
    // Extract BuildingId based on entity type
    if (entity is FloorplanMaskedArea fma)
        return fma.Floorplan.Floor.BuildingId;
    if (entity is MstFloor floor)
        return floor.BuildingId;
    // ... other types
}
```

**Alternative**: Let repository implement interface `IBuildingFilterable` dengan method `GetBuildingId()`.

### 3. Get User's Accessible Buildings from JWT

**Approach**: Extend JWT token dengan `accessibleBuildings` claim

```csharp
// Di AuthService.cs saat generate JWT token
var userBuildingAccesses = await _userBuildingAccessRepository.GetByUserIdAsync(user.Id);
var accessibleBuildingIds = userBuildingAccesses.Select(x => x.BuildingId.ToString()).ToList();

claims.Add(new Claim("accessibleBuildings",
    string.Join(",", accessibleBuildingIds)));
```

### 4. Filter Analytics Queries by Building

**Repository Layer Changes**:

#### A. Extend BaseEntityQuery with Building Filter

```csharp
// Di TrackingAnalyticsRepository.cs
private IQueryable<TrackingTransaction> BaseEntityQuery()
{
    var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
    var accessibleBuildingIds = GetAccessibleBuildingsFromToken();

    var query = _context.TrackingTransactions
        .Include(t => t.FloorplanMaskedArea.Floorplan.Floor.Building)
        .Where(t => t.TransTime >= DateTime.Today);

    // Multi-tenancy filter
    query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

    // NEW: Building access filter
    if (!isSystemAdmin && accessibleBuildingIds.Any())
    {
        query = query.Where(t =>
            accessibleBuildingIds.Contains(t.FloorplanMaskedArea.Floorplan.Floor.BuildingId)
        );
    }

    return query;
}
```

#### B. Extend Monitoring DTOs with Building Info

**Create**: `TrackingTransactionBuildingRead.cs`

```csharp
public class TrackingTransactionBuildingRead
{
    public Guid Id { get; set; }
    public DateTime TransTime { get; set; }
    public string BuildingName { get; set; }  // Include for filtering
    public string FloorName { get; set; }
    public string AreaName { get; set; }
    // ... other fields
}
```

---

## Implementation Plan

### Phase 1: Create Infrastructure (High Priority)

#### 1.1 Create UserBuildingAccess Entity
**File**: `Shared/Entities.Models/UserBuildingAccess.cs`

Steps:
- Create entity following junction table pattern
- Include navigation properties
- Inherit from `BaseModelWithTimeApp`
- Implement `IApplicationEntity`

#### 1.2 Create Migration
```bash
dotnet ef migrations add AddUserBuildingAccess \
  --project Shared/Repositories/Repositories.csproj \
  --startup-project Services.API/Auth/Auth.csproj
```

#### 1.3 Update Database
```bash
dotnet ef database update \
  --project Shared/Repositories/Repositories.csproj \
  --startup-project Services.API/Auth/Auth.csproj
```

---

### Phase 2: Create Repository & Service (High Priority)

#### 2.1 Create UserBuildingAccessRepository
**File**: `Shared/Repositories/Repository/UserBuildingAccessRepository.cs`

Methods:
- `GetByUserIdAsync(Guid userId)` - Get user's accessible buildings
- `GetByBuildingIdAsync(Guid buildingId)` - Get users with access to building
- `AddAccessAsync(Guid userId, Guid buildingId)` - Grant access
- `RemoveAccessAsync(Guid userId, Guid buildingId)` - Revoke access
- `GetAccessibleBuildingIdsAsync(Guid userId)` - Helper for filtering

#### 2.2 Create IUserBuildingAccessService
**File**: `Shared/BusinessLogic.Services/Interface/IUserBuildingAccessService.cs`

Methods:
- `AssignBuildingsToUserAsync(Guid userId, List<Guid> buildingIds)`
- `GetUserAccessibleBuildingsAsync(Guid userId)`
- `RevokeBuildingAccessAsync(Guid userId, Guid buildingId)`

#### 2.3 Create UserBuildingAccessService
**File**: `Shared/BusinessLogic.Services/Implementation/UserBuildingAccessService.cs`

Requirements:
- Inherit from `BaseService`
- Inject `IAuditEmitter`
- Validate user is in same application as building
- Use audit logging

---

### Phase 3: Extend JWT Token (Medium Priority)

#### 3.1 Update AuthService.GenerateJwtToken()
**File**: `Shared/BusinessLogic.Services/Interface/IAuthService.cs` (AuthService class)

Add to JWT claims:
```csharp
var userBuildingAccesses = await _userBuildingAccessRepository
    .GetByUserIdAsync(user.Id);
var accessibleBuildingIds = userBuildingAccesses
    .Select(x => x.BuildingId.ToString())
    .ToList();

claims.Add(new Claim("accessibleBuildings",
    string.Join(",", accessibleBuildingIds)));
```

---

### Phase 4: Update BaseRepository (High Priority)

#### 4.1 Add GetAccessibleBuildingsFromToken() Method
**File**: `Shared/Repositories/Repository/BaseRepository.cs`

```csharp
protected IEnumerable<Guid> GetAccessibleBuildingsFromToken()
{
    var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

    // System admin bypasses building restrictions
    if (isSystemAdmin)
        return Enumerable.Empty<Guid>();

    var buildingIdsClaim = _httpContextAccessor.HttpContext?
        .User?.FindFirst("accessibleBuildings")?.Value;

    if (string.IsNullOrEmpty(buildingIdsClaim))
        return Enumerable.Empty<Guid>();

    return buildingIdsClaim.Split(',')
        .Select(Guid.Parse)
        .ToList();
}
```

#### 4.2 Add ApplyBuildingFilterIfNonSystemAdmin()
**File**: `Shared/Repositories/Repository/BaseRepository.cs`

```csharp
protected IQueryable<T> ApplyBuildingFilterIfNonSystemAdmin<T>(
    IQueryable<T> query,
    Func<T, Guid?> buildingIdSelector
) where T : class
{
    var accessibleBuildingIds = GetAccessibleBuildingsFromToken();

    // Skip if system admin or no restriction
    if (!accessibleBuildingIds.Any())
        return query;

    // Filter by accessible buildings
    query = query.Where(entity =>
    {
        var buildingId = buildingIdSelector(entity);
        return buildingId.HasValue && accessibleBuildingIds.Contains(buildingId.Value);
    });

    return query;
}
```

---

### Phase 5: Update Analytics Repositories (High Priority)

#### 5.1 Update TrackingAnalyticsRepository
**File**: `Shared/Repositories/Repository/Analytics/TrackingAnalyticsRepository.cs`

Change:
- Add building filter to all queries
- Use `ApplyBuildingFilterIfNonSystemAdmin` with selector:
  ```csharp
  t => t.FloorplanMaskedArea.Floorplan.Floor.BuildingId
  ```

#### 5.2 Update AlarmRecordTrackingRepository
**File**: `Shared/Repositories/Repository/AlarmRecordTrackingRepository.cs`

Change:
- Add building filter to queries
- Use selector for alarm through FloorplanMaskedArea

#### 5.3 Update Patrol Repositories
**Files**:
- `Shared/Repositories/Repository/Patrol/PatrolAreaRepository.cs`
- `Shared/Repositories/Repository/Patrol/PatrolSessionRepository.cs`

Change:
- Add building filter through Floor → Building

---

### Phase 6: Create UserBuildingAccess Controller (Medium Priority)

#### 6.1 Create UserBuildingAccessController
**File**: `Shared/Web.API.Controllers/Controllers/UserBuildingAccessController.cs`

Endpoints:
- `POST /api/user-building-access/assign` - Assign buildings to user
- `GET /api/user-building-access/{userId}` - Get user's accessible buildings
- `DELETE /api/user-building-access/{userId}/{buildingId}` - Revoke access
- `POST /api/user-building-access/filter` - DataTables endpoint

Authorization:
- `[MinLevel(LevelPriority.SuperAdmin)]` - Only SuperAdmin can manage access

---

### Phase 7: Frontend Integration (Low Priority - Out of Scope)

Update Analytics frontend:
- Add building selector (pre-filtered by user's accessible buildings)
- Hide/show data based on building access
- Show "Access Denied" message for unauthorized buildings

---

## Critical Files to Create

1. **Shared/Entities.Models/UserBuildingAccess.cs** - Junction table entity
2. **Shared/Repositories/Repository/UserBuildingAccessRepository.cs** - Repository
3. **Shared/BusinessLogic.Services/Interface/IUserBuildingAccessService.cs** - Service interface
4. **Shared/BusinessLogic.Services/Implementation/UserBuildingAccessService.cs** - Service implementation
5. **Shared/Web.API.Controllers/Controllers/UserBuildingAccessController.cs** - Controller

---

## Critical Files to Modify

1. **Shared/Repositories/Repository/BaseRepository.cs** - Add building filter methods
2. **Shared/BusinessLogic.Services/Interface/IAuthService.cs** - Extend JWT with building claims
3. **Shared/Repositories/Repository/Analytics/TrackingAnalyticsRepository.cs** - Add building filter
4. **Shared/Repositories/Repository/AlarmRecordTrackingRepository.cs** - Add building filter
5. **Shared/Repositories/Repository/Patrol/PatrolAreaRepository.cs** - Add building filter
6. **Shared/Repositories/Repository/Patrol/PatrolSessionRepository.cs** - Add building filter
7. **Services.API/Auth/Program.cs** - Register new services/repositories

---

## Existing Patterns to Reuse

### Junction Table Pattern
Reference: `CardAccessMaskedArea` entity
- Simple foreign key relationships
- Include ApplicationId for multi-tenancy
- Status field for soft delete

### Repository Pattern for Junction Tables
Reference: `CardAccessMaskedAreaRepository`
- Query by foreign key with navigation
- Bulk delete by foreign key

### Hierarchy Query Pattern
Reference: `TrackingAnalyticsRepository.cs`
```csharp
query = query.Include(t => t.FloorplanMaskedArea.Floorplan.Floor.Building)
         .Where(t => t.TransTime >= from && t.TransTime <= to);
```

---

## Verification Steps

### After Implementation:

1. **Create Test Data**:
   ```sql
   -- Assign User A (PrimaryAdmin) to Building X & Y
   INSERT INTO user_building_access (user_id, building_id, application_id, status, created_at)
   VALUES
     ('<user-a-id>', '<building-x-id>', '<app-id>', 1, GETDATE()),
     ('<user-a-id>', '<building-y-id>', '<app-id>', 1, GETDATE());
   ```

2. **Test JWT Token**:
   - Login as User A
   - Decode JWT token
   - Verify `accessibleBuildings` claim contains Building X & Y IDs

3. **Test Analytics Query**:
   - Call GET /api/analytics/tracking with Building X filter
   - Should return data ✅

   - Call GET /api/analytics/tracking with Building Z filter
   - Should return EMPTY (user tidak punya akses) ✅

4. **Test Cross-Build Data**:
   - Query yang melibatkan Building X, Y, Z
   - Hanya data dari X & Y yang muncul ✅

---

## Architecture Decision Records

### Decision 1: Why Junction Table (UserBuildingAccess)?

**Options Considered**:
1. User.BuildingId (single building) - Ditolak, tidak fleksibel
2. UserGroup.Buildings (group-level) - Ditolak, kurang granular
3. UserBuildingAccess (junction table) - ✅ Dipilih

**Rationale**:
- User bisa punya akses ke multiple building
- Flexible untuk per-user assignment
- Mudah ditambah/removed akses
- Follow existing pattern (CardAccessMaskedArea)

### Decision 2: Why Filter at Database Level?

**Options Considered**:
1. Controller level - Ditolak, kurang aman
2. Service level - Ditolak, kurang efisien
3. Database/Repository level - ✅ Dipilih

**Rationale**:
- Lebih aman - data dari building lain tidak pernah diquery
- Lebih efisien - filter di SQL level
- Lebih konsisten dengan pattern ApplicationId filtering

### Decision 3: Why Extend JWT Token?

**Options Considered**:
1. Query UserBuildingAccess di setiap request - Ditolak, slow
2. Cache di Redis - Ditolak, tambah complexity
3. Extend JWT token - ✅ Dipilih

**Rationale**:
- Simple - token sudah passed di setiap request
- Fast - tidak perlu query database
- Stateless - no cache dependency

---

## Notes

- **System Admin bypass**: System admin (LevelPriority.System = 0) tetap bisa lihat semua building
- **SuperAdmin bypass**: SuperAdmin (LevelPriority.SuperAdmin = 1) juga bisa lihat semua building
- **Operator restriction**: Hanya PrimaryAdmin (LevelPriority.PrimaryAdmin = 2) ke atas yang di-filter
- **Backward compatibility**: User tanpa UserBuildingAccess entries akan dianggap punya akses ke SEMUA building dalam application mereka (default behavior)

---

## Open Questions for User

1. **Apakah SuperAdmin juga perlu di-restrict per building?**
   - Saat ini: SuperAdmin bisa lihat semua building
   - Alternatif: SuperAdmin juga perlu UserBuildingAccess

2. **Apakah ada UI untuk manage UserBuildingAccess?**
   - Atau hanya via database/SQL?
   - Perlu CRUD endpoint di controller?

3. **Apakah inherited access needed?**
   - Jika User A punya akses ke Building X, apakah otomatis semua user di UserGroup yang sama juga punya akses?
   - Saat ini: Tidak, per-user assignment

4. **Bagaimana dengan new user registration?**
   - Default: User baru tanpa UserBuildingAccess = akses ke semua building
   - Atau: Wajib assign minimal 1 building saat registration?
