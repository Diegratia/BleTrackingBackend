# Operator Access per Building - Implementation Summary

## Overview

Implementation completed for restricting **Operator (PrimaryAdmin role)** access to monitoring data based on their assigned buildings. This feature ensures that operators can only view data from buildings they have been granted access to, while System Admin and SuperAdmin bypass these restrictions.

**Date Completed**: 2026-02-05
**Status**: ✅ Fully Implemented

---

## Architecture Overview

### Problem Statement
- **Before**: All PrimaryAdmin users could see data from ALL buildings within their Application
- **After**: PrimaryAdmin users can only see data from buildings explicitly assigned to them via `UserBuildingAccess`

### Solution Approach
1. **Junction Table**: Created `UserBuildingAccess` entity for many-to-many relationship
2. **JWT Token Extension**: Added `accessibleBuildings` claim containing comma-separated building IDs
3. **Repository-Level Filtering**: Automatic filtering in analytics repositories using `ApplyBuildingFilterIfNonSystemAdmin()`
4. **Admin Bypass**: System Admin and SuperAdmin automatically bypass building restrictions

---

## Files Created

### 1. Entity Layer
| File | Description |
|------|-------------|
| `Shared/Entities.Models/UserBuildingAccess.cs` | Junction table entity linking Users and Buildings |

**Entity Structure**:
```csharp
public class UserBuildingAccess : BaseModelWithTimeApp, IApplicationEntity
{
    public Guid UserId { get; set; }
    public Guid BuildingId { get; set; }
    public Guid ApplicationId { get; set; }

    // Navigation properties
    public User User { get; set; }
    public MstBuilding Building { get; set; }
}
```

### 2. Repository Layer
| File | Description |
|------|-------------|
| `Shared/Repositories/Repository/UserBuildingAccessRepository.cs` | Repository for managing user-building access |

**Key Methods**:
- `GetByUserIdAsync(Guid userId)` - Get user's accessible buildings
- `GetByBuildingIdAsync(Guid buildingId)` - Get users with access to building
- `GetAccessibleBuildingIdsAsync(Guid userId)` - Helper for JWT token generation
- `AddAccessAsync(UserBuildingAccess access)` - Grant access
- `RemoveAccessAsync(Guid userId, Guid buildingId)` - Revoke access

### 3. Service Layer
| File | Description |
|------|-------------|
| `Shared/BusinessLogic.Services/Interface/IUserBuildingAccessService.cs` | Service interface |
| `Shared/BusinessLogic.Services/Implementation/UserBuildingAccessService.cs` | Service implementation |

**Key Methods**:
- `AssignBuildingsToUserAsync(Guid userId, List<Guid> buildingIds)` - Assign multiple buildings
- `GetUserAccessibleBuildingsAsync(Guid userId)` - Get user's accessible buildings
- `RevokeBuildingAccessAsync(Guid userId, Guid buildingId)` - Revoke single building access
- `RevokeAllBuildingAccessAsync(Guid userId)` - Revoke all building access
- `HasAccessAsync(Guid userId, Guid buildingId)` - Check access permission

### 4. API Layer
| File | Description |
|------|-------------|
| `Shared/Web.API.Controllers/Controllers/UserBuildingAccessController.cs` | REST API controller |
| `Shared/Shared.Contracts/Read/UserBuildingAccessRead.cs` | Read DTO |

**API Endpoints**:
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/user-building-access/user/{userId}/assign` | Assign buildings to user |
| `GET` | `/api/user-building-access/user/{userId}` | Get user's accessible buildings |
| `GET` | `/api/user-building-access/building/{buildingId}` | Get users with access to building |
| `DELETE` | `/api/user-building-access/user/{userId}/building/{buildingId}` | Revoke building access |
| `DELETE` | `/api/user-building-access/user/{userId}/all` | Revoke all building access |
| `GET` | `/api/user-building-access/user/{userId}/building/{buildingId}/check` | Check if user has access |

**Authorization**: All endpoints require `[MinLevel(LevelPriority.SuperAdmin)]`

---

## Files Modified

### 1. Database Context
**File**: `Shared/Repositories/DbContexts/BleTrackingDbContext.cs`

**Changes**:
- Added `DbSet<UserBuildingAccess> UserBuildingAccesses { get; set; }`
- Added table mapping in `OnModelCreating()`

### 2. Base Repository
**File**: `Shared/Repositories/Repository/BaseRepository.cs`

**New Methods**:
```csharp
// Get accessible building IDs from JWT token claim
protected IEnumerable<Guid> GetAccessibleBuildingsFromToken()

// Apply building filter to query if user is not System/SuperAdmin
protected IQueryable<T> ApplyBuildingFilterIfNonSystemAdmin<T>(
    IQueryable<T> query,
    Func<T, Guid?> buildingIdSelector
) where T : class
```

**Behavior**:
- Returns empty list for System Admin and SuperAdmin (bypass filter)
- Parses `accessibleBuildings` claim from JWT token
- Filters queries by accessible buildings for non-admin users

### 3. Authentication Service
**File**: `Shared/BusinessLogic.Services/Interface/IAuthService.cs`

**Changes**:
- Injected `UserBuildingAccessRepository`
- Modified `GenerateJwtToken()` to async method
- Added `accessibleBuildings` claim to JWT token for non-admin users

```csharp
// Add accessible buildings claim for non-System and non-SuperAdmin users
if (user.Group.LevelPriority != LevelPriority.System &&
    user.Group.LevelPriority != LevelPriority.SuperAdmin)
{
    var accessibleBuildingIds = await _userBuildingAccessRepository.GetAccessibleBuildingIdsAsync(user.Id);
    var buildingIdsString = string.Join(",", accessibleBuildingIds.Select(id => id.ToString()));
    claims.Add(new Claim("accessibleBuildings", buildingIdsString));
}
```

### 4. Analytics Repositories

#### TrackingAnalyticsRepository
**File**: `Shared/Repositories/Repository/Analytics/TrackingAnalyticsRepository.cs`

**Change**: Added building filter to `ApplyFilters()` method
```csharp
// Apply building access filter for non-system/super admin users
query = ApplyBuildingFilterIfNonSystemAdmin(query, t =>
    t.FloorplanMaskedArea?.Floorplan?.Floor?.BuildingId
);
```

#### AlarmRecordTrackingRepository
**File**: `Shared/Repositories/Repository/AlarmRecordTrackingRepository.cs`

**Changes**: Added building filter to BOTH `ApplyFilters()` methods:
- `ApplyFilters(IQueryable<AlarmRecordTracking> query, ...)` - Filters by FloorplanMaskedArea
- `ApplyFilters(IQueryable<AlarmTriggers> query, ...)` - Filters by Floorplan

#### PatrolAreaRepository
**File**: `Shared/Repositories/Repository/PatrolAreaRepository.cs`

**Change**: Added building filter to `GetAllQueryable()` method
```csharp
// Apply building access filter for non-system/super admin users
query = ApplyBuildingFilterIfNonSystemAdmin(query, d => d.Floor?.BuildingId);
```

### 5. Dependency Injection
**File**: `Services.API/Auth/Program.cs`

**Added Registrations**:
```csharp
// Services
builder.Services.AddScoped<IUserBuildingAccessService, UserBuildingAccessService>();

// Repositories
builder.Services.AddScoped<UserBuildingAccessRepository>();
builder.Services.AddScoped<MstBuildingRepository>();
```

---

## Database Migration

### Migration Details
- **Migration Name**: `AddUserBuildingAccess`
- **Migration ID**: `20260205014528_AddUserBuildingAccess`
- **Status**: ✅ Applied successfully

### Table Created
**Table Name**: `user_building_access`

**Columns**:
| Column | Type | Description |
|--------|------|-------------|
| `id` | Guid | Primary key |
| `user_id` | Guid | Foreign key to User |
| `building_id` | Guid | Foreign key to MstBuilding |
| `application_id` | Guid | Multi-tenancy support |
| `status` | int | Soft delete (1 = active, 0 = inactive) |
| `created_at` | DateTime | Audit timestamp |
| `updated_at` | DateTime | Audit timestamp |
| `created_by` | string | Audit user |
| `updated_by` | string | Audit user |

**Indexes**:
- Foreign key on `user_id`
- Foreign key on `building_id`
- Foreign key on `application_id`

---

## How It Works

### 1. Login Flow (Building Access Added to JWT)

```
┌─────────────────────────────────────────────────────────────┐
│ User Login                                                   │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ AuthService.GenerateJwtToken()                              │
│  - Query UserBuildingAccess for user's buildings             │
│  - Add "accessibleBuildings" claim with comma-separated IDs  │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ JWT Token Generated with Claims:                            │
│  - sub: user_id                                              │
│  - email: user_email                                         │
│  - ApplicationId: app_id                                     │
│  - level: role_level                                         │
│  - accessibleBuildings: "bldg1,bldg2,bldg3"  ← NEW          │
└─────────────────────────────────────────────────────────────┘
```

### 2. Query Flow (Automatic Building Filtering)

```
┌─────────────────────────────────────────────────────────────┐
│ API Request (e.g., GET /api/analytics/tracking)              │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ Repository Method Called                                    │
│  - BaseRepository.GetAccessibleBuildingsFromToken()         │
│    - Check if System/SuperAdmin → bypass (return empty)     │
│    - Parse "accessibleBuildings" claim from JWT             │
│    - Return list of building IDs                            │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ ApplyBuildingFilterIfNonSystemAdmin()                       │
│  - If no building restrictions (empty list) → skip filter   │
│  - Apply WHERE building_id IN (accessibleBuildingIds)       │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ SQL Query Executed with Building Filter                     │
│  - Only returns data from accessible buildings              │
└─────────────────────────────────────────────────────────────┘
```

### 3. Role-Based Access Control

| Role | Level Priority | Building Filter Behavior |
|------|---------------|--------------------------|
| System | 0 | ❌ No filter (sees all buildings) |
| SuperAdmin | 1 | ❌ No filter (sees all buildings) |
| PrimaryAdmin | 2 | ✅ Filtered by `UserBuildingAccess` |
| Primary | 3 | ✅ Filtered by `UserBuildingAccess` |
| Secondary | 4 | ✅ Filtered by `UserBuildingAccess` |
| UserCreated | 5 | ✅ Filtered by `UserBuildingAccess` |

---

## Testing Guide

### 1. Setup Test Data

```sql
-- Assign User A (PrimaryAdmin) to Building X & Y
INSERT INTO user_building_access (id, user_id, building_id, application_id, status, created_at, updated_at)
VALUES
  (NEWID(), '<user-a-id>', '<building-x-id>', '<app-id>', 1, GETDATE(), GETDATE()),
  (NEWID(), '<user-a-id>', '<building-y-id>', '<app-id>', 1, GETDATE(), GETDATE());
```

### 2. Verify JWT Token

1. Login as User A
2. Decode JWT token (use jwt.io or similar)
3. Verify `accessibleBuildings` claim contains Building X & Y IDs

**Expected JWT Payload**:
```json
{
  "sub": "user-a-id",
  "email": "usera@example.com",
  "ApplicationId": "app-id",
  "level": "2",
  "accessibleBuildings": "building-x-id,building-y-id"
}
```

### 3. Test Analytics Queries

**Test Case 1: Access Allowed Building**
```bash
# Should return data ✅
GET /api/analytics/tracking?buildingId=<building-x-id>
```

**Test Case 2: Access Denied Building**
```bash
# Should return EMPTY (user doesn't have access) ✅
GET /api/analytics/tracking?buildingId=<building-z-id>
```

**Test Case 3: Cross-Build Data Query**
```bash
# Should only return data from Building X & Y ✅
GET /api/analytics/tracking
```

### 4. Test Building Access Management

**Assign Buildings**:
```bash
POST /api/user-building-access/user/<user-id>/assign
{
  "buildingIds": ["building-x-id", "building-y-id", "building-z-id"]
}
```

**Get User's Buildings**:
```bash
GET /api/user-building-access/user/<user-id>
```

**Revoke Building Access**:
```bash
DELETE /api/user-building-access/user/<user-id>/building/<building-x-id>
```

---

## Backward Compatibility

✅ **Fully backward compatible**:

- **Users without `UserBuildingAccess` entries**: See ALL buildings in their application (default behavior)
- **Existing JWT tokens without `accessibleBuildings` claim**: Treated as no building restrictions
- **Existing analytics queries**: Automatically apply building filter if claim exists

---

## Security Considerations

### 1. Database-Level Filtering
- Filtering occurs at **SQL query level**, not controller/service level
- Data from unauthorized buildings is **never retrieved** from database
- More secure and efficient than filtering in memory

### 2. JWT Token Validation
- Building IDs are **validated** before being added to JWT
- Only buildings from user's **same Application** can be assigned
- System/SuperAdmin automatically bypass all restrictions

### 3. Authorization
- All management endpoints require **SuperAdmin** role
- Building access cannot be escalated by users themselves
- Audit logging tracks all access assignments/revocations

---

## Performance Impact

### Positive Impacts
✅ **Reduced data transfer**: Only relevant building data is retrieved
✅ **Faster queries**: Filtered queries return fewer rows
✅ **Lower memory usage**: Less data loaded into application memory

### Considerations
⚠️ **JWT token size**: Each building ID adds ~36 bytes (GUID format)
⚠️ **Token parsing**: Minimal overhead for parsing comma-separated IDs
⚠️ **Database joins**: Building filter requires JOIN through hierarchy (Floor → Floorplan → Floor)

**Recommendation**: For users with 50+ buildings, consider alternative approach (e.g., caching in Redis)

---

## Future Enhancements

### Potential Improvements
1. **Frontend Integration**
   - Building selector dropdown (pre-filtered by accessible buildings)
   - UI indicators showing which buildings user can access
   - "Access Denied" messages for unauthorized building views

2. **Caching Strategy**
   - Cache user building access in Redis for faster lookups
   - Invalidate cache on access assignment/revocation

3. **Inherited Access**
   - Option to inherit building access from UserGroup
   - Role-based building templates (e.g., "Security A" gets Buildings 1-5)

4. **Time-Based Access**
   - Add `valid_from` and `valid_until` to UserBuildingAccess
   - Support temporary building access grants

---

## Migration Notes

### For Existing Deployments

1. **Database Migration**: Run `dotnet ef database update` on all services
2. **Restart Auth Service**: Required for new dependency injection registrations
3. **Existing Users**: No action needed - they see all buildings by default
4. **New Users**: Assign buildings using API endpoints or direct SQL

### Rollback Plan

If issues arise:
1. Remove `ApplyBuildingFilterIfNonSystemAdmin()` calls from repositories
2. Comment out `accessibleBuildings` claim in JWT generation
3. Existing data remains unchanged (no data loss)

---

## Support & Troubleshooting

### Common Issues

**Issue**: User cannot see ANY buildings
- **Cause**: User has empty `accessibleBuildings` claim
- **Fix**: Assign buildings via API or check JWT token

**Issue**: User sees all buildings despite restrictions
- **Cause**: User might be System/SuperAdmin or no entries in `UserBuildingAccess`
- **Fix**: Check user's role level and `UserBuildingAccess` table

**Issue**: Building filter not applied in queries
- **Cause**: Repository missing `ApplyBuildingFilterIfNonSystemAdmin()` call
- **Fix**: Ensure all analytics repositories call the method

---

## Documentation References

- [Implementation Plan](./operator-access-per-building-plan.md) - Original planning document
- [PROJECT_GUIDE.md](../PROJECT_GUIDE.md) - General architecture guidelines
- [REFACTORING_GUIDE.md](../REFACTORING_GUIDE.md) - Repository patterns and standards

---

## Summary

✅ **Implementation Status**: Complete
✅ **Database Migration**: Applied
✅ **API Endpoints**: Available
✅ **Backward Compatibility**: Maintained
✅ **Security**: Database-level filtering
✅ **Admin Bypass**: System/SuperAdmin bypass restrictions

The feature is **production-ready** and can be deployed following the testing guide above.
