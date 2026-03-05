# Relationship Design: User ↔ Security/Member/Visitor

## Context

Currently, the relationship between **User/UserGroup** and **Security/Member/Visitor** entities uses **email-based matching** instead of foreign key relationships. This analysis evaluates whether to continue with email matching or implement proper foreign key relationships.

---

## Current Implementation (Email-Based Matching)

### How It Works
```csharp
// Example from PatrolSessionRepository.cs:40-43
query = query.Where(pas =>
    pas.Security != null &&
    pas.Security.Email == userEmail  // String matching
);
```

### Entity Structures

| Entity | Email Field | Nullable | FK to User |
|--------|-------------|----------|------------|
| **User** | `Email` (required) | No | - |
| **MstSecurity** | `Email` | **Yes** | No |
| **MstMember** | `Email` | **Yes** | No |
| **Visitor** | `Email` | **Yes** | No |

### Issues Found

| Issue | Impact | Location |
|-------|--------|----------|
| **Nullable emails** | Queries can fail or miss records | All 3 entities |
| **No unique constraint** | Duplicate emails possible | Database schema |
| **String comparison** | Poor performance (no index usage) | All repositories |
| **No referential integrity** | Orphaned records possible | Database level |
| **Email changes break links** | Manual updates required | Application logic |

### Code Locations Using Email Matching

- `PatrolSessionRepository.cs:40-43` - Security session filtering
- `PatrolCaseRepository.cs:48, 87, 125, 432` - Case ownership
- `MstSecurityRepository.cs:285-290` - GetByEmailAsyncRaw
- `MstMemberRepository.cs:465-472` - GetByEmailAsyncRaw
- `VisitorRepository.cs:234` - GetByEmailAsyncRaw

---

## Final Decision: Nullable UserId FK for All Three Entities

Based on user requirements:
- **Security**: Some without accounts → `UserId` nullable
- **Member**: Some without accounts → `UserId` nullable
- **Visitor**: Don't need accounts → `UserId` nullable

## Implementation Plan

### 1. Add UserId to All Three Entities

```csharp
// MstSecurity.cs
public Guid? UserId { get; set; }
public User? User { get; set; }

// MstMember.cs
public Guid? UserId { get; set; }
public User? User { get; set; }

// Visitor.cs
public Guid? UserId { get; set; }
public User? User { get; set; }
```

### 2. Migration Strategy

**Phase 1: Add Columns (Nullable)**
- Add `UserId` column to `MstSecurities`, `MstMembers`, `Visitors` tables
- Keep existing `Email` column for backup/records without accounts

**Phase 2: Backfill Data**
- Match existing records by email to populate UserId
- Records with matching User.Email → Set UserId
- Records without matching User → UserId remains null

**Phase 3: Add FK Constraints**
- Create foreign key to Users table
- Create indexes on UserId for performance

**Phase 4: Update Code**
- Replace email-based queries with UserId-based queries
- Keep email as secondary identifier where needed

### 3. Query Pattern Changes

**Before (Email-Based):**
```csharp
query = query.Where(s => s.Email == userEmail);
```

**After (UserId-Based):**
```csharp
// Primary: Use UserId (fast, indexed)
query = query.Where(s => s.UserId == currentUserId);

// Fallback: For records without UserId, use email
query = query.Where(s => s.UserId == currentUserId || s.Email == userEmail);
```

---

## Files to Modify

### Entities (3 files)
- `Shared/Entities.Models/MstSecurity.cs` - Add `UserId?`, `User` navigation
- `Shared/Entities.Models/MstMember.cs` - Add `UserId?`, `User` navigation
- `Shared/Entities.Models/Visitor.cs` - Add `UserId?`, `User` navigation

### DbContext (1 file)
- `Shared/Repositories/DbContexts/BleTrackingDbContext.cs` - Add relationships

### Migration (1 new file)
- `Shared/Repositories/Migrations/xxx_AddUserIdToSecurityMemberVisitor.cs`

### Repositories (Multiple files)
- Update queries to use `UserId` instead of `Email`
- Keep email as fallback for records without UserId

Key repositories:
- `PatrolSessionRepository.cs` - Line 40-43
- `PatrolCaseRepository.cs` - Lines 48, 87, 125, 432
- `MstSecurityRepository.cs` - All email-based queries
- `MstMemberRepository.cs` - All email-based queries
- `VisitorRepository.cs` - All email-based queries

### Services (Update logic)
- `PatrolSessionService.cs` - GetSecurityByEmail → GetSecurityByUserId
- `PatrolCaseService.cs` - User context matching
- `MstSecurityService.cs` - Create/Update validation
- `MstMemberService.cs` - Create/Update validation

### Controllers (Optional updates)
- Update to use UserId where appropriate

---

## Backfill SQL Script

```sql
-- Backfill UserId for existing records based on email matching
UPDATE MstSecurities
SET UserId = u.Id
FROM Users u
WHERE LOWER(MstSecurities.Email) = LOWER(u.Email)
  AND MstSecurities.UserId IS NULL;

UPDATE MstMembers
SET UserId = u.Id
FROM Users u
WHERE LOWER(MstMembers.Email) = LOWER(u.Email)
  AND MstMembers.UserId IS NULL;

UPDATE Visitors
SET UserId = u.Id
FROM Users u
WHERE LOWER(Visitors.Email) = LOWER(u.Email)
  AND Visitors.UserId IS NULL;
```

---

## Benefits of This Approach

| Benefit | Description |
|---------|-------------|
| **Performance** | Indexed FK lookups vs string comparison |
| **Data Integrity** | FK constraint prevents invalid UserId values |
| **Flexibility** | Nullable allows records without accounts |
| **Gradual Migration** | Can migrate incrementally |
| **Backup Identifier** | Email still available for fallback |

---

## Additional Task: GetAllLookUp Security Filter Enhancement

**File:** `Shared/Repositories/Repository/MstSecurityRepository.cs`

Update `GetAllLookUpAsync` method:

| headsOnly Parameter | Result |
|---------------------|--------|
| `true` | Show ONLY security heads (`IsHead = true`) |
| `false` | Show ONLY non-heads (`IsHead = false` or `null`) |
| `null` (default) | Show ALL (no filtering) |

**Implementation:**
```csharp
public async Task<List<MstSecurityLookUpRead>> GetAllLookUpAsync(bool? headsOnly = null)
{
    // ... existing query setup ...

    var projected = query.GroupJoin(...).Select(...);

    // Filter by heads
    if (headsOnly.HasValue)
    {
        if (headsOnly.Value)
            projected = projected.Where(x => x.IsHead == true);
        else
            projected = projected.Where(x => x.IsHead != true);  // false or null
    }

    return await projected.ToListAsync();
}
```

**Also Update:**
- `IMstSecurityService.cs` - Interface already has `bool? headsOnly` parameter
- `MstSecurityService.cs` - Pass parameter to repository
- `MstSecurityController.cs` - Already accepts query parameter

---

## Implementation Status

### Completed
- [x] Add UserId to MstSecurity entity
- [x] Add UserId to MstMember entity
- [x] Add UserId to Visitor entity
- [x] Update DbContext relationships for MstSecurity
- [x] Update DbContext relationships for MstMember
- [x] Update DbContext relationships for Visitor

### Pending
- [ ] Create EF Core migration
- [ ] Update PatrolSessionRepository queries
- [ ] Update PatrolCaseRepository queries
- [ ] Update MstSecurityRepository queries and GetAllLookUpAsync
- [ ] Update MstMemberRepository queries
- [ ] Update VisitorRepository queries
- [ ] Update services for UserId-based logic
- [ ] Run migration and backfill data
