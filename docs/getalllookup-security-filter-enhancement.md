# GetAllLookUp Security Filter Enhancement

## Context

Enhance the `GetAllLookUpAsync` method in `MstSecurityRepository` to support filtering by security heads using the `headsOnly` parameter.

---

## Current Implementation

**File:** `Shared/Repositories/Repository/MstSecurityRepository.cs`

Current implementation only filters when `headsOnly = true`:
```csharp
if (headsOnly.HasValue && headsOnly.Value)
{
    projected = projected.Where(x => x.IsHead == true);
}
```

### Issue

When `headsOnly = false`, no filtering is applied (returns ALL records including heads).

---

## Required Behavior

| headsOnly Parameter | Result |
|---------------------|--------|
| `true` | Show ONLY security heads (`IsHead = true`) |
| `false` | Show ONLY non-heads (`IsHead = false` or `null`) |
| `null` (default) | Show ALL (no filtering) |

---

## Files to Modify

### 1. MstSecurityRepository.cs
**Path:** `Shared/Repositories/Repository/MstSecurityRepository.cs`

**Change the `GetAllLookUpAsync` method (lines 180-219):**

```csharp
public async Task<List<MstSecurityLookUpRead>> GetAllLookUpAsync(bool? headsOnly = null)
{
    var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
    var query = _context.MstSecurities
        .AsNoTracking()
        .Where(fd => fd.Status != 0 && fd.CardNumber != null);

    query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

    var projected = query.GroupJoin(
            _context.Users.Include(u => u.Group),
            s => s.Email,
            u => u.Email,
            (s, users) => new { s, user = users.FirstOrDefault() })
        .Select(x => new MstSecurityLookUpRead
        {
            Id = x.s.Id,
            Name = x.s.Name,
            PersonId = x.s.PersonId,
            CardNumber = x.s.CardNumber,
            OrganizationId = x.s.OrganizationId,
            DepartmentId = x.s.DepartmentId,
            DistrictId = x.s.DistrictId,
            OrganizationName = x.s.Organization.Name,
            DepartmentName = x.s.Department.Name,
            DistrictName = x.s.District.Name,
            Email = x.s.Email,
            IsHead = x.user != null && x.user.Group != null ? x.user.Group.IsHead : null,
            ApplicationId = x.s.ApplicationId,
            Status = x.s.Status
        });

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

### 2. Interface and Service (Already Correct)

- `IMstSecurityService.cs` - Already has `bool? headsOnly` parameter
- `MstSecurityService.cs` - Already passes parameter to repository
- `MstSecurityController.cs` - Already accepts query parameter

---

## Verification

Test the three scenarios:
1. `GET /api/mstsecurity/lookup?headsOnly=true` → Only security heads
2. `GET /api/mstsecurity/lookup?headsOnly=false` → Only non-heads
3. `GET /api/mstsecurity/lookup` → All securities
