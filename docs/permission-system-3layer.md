# Implementasi Permission Flags System - 3 Layer Pattern

## Context

Saat ini sistem menggunakan `LevelPriority` untuk authorization, tapi kurang fleksibel untuk kasus seperti:
- Operator dengan level sama tapi hak akses berbeda
- Head Security yang butuh permission spesifik (approve, disarm, dll)
- Operator biasa yang hanya view saja

**Pattern yang dipilih: 3-Layer Access Control**
- Layer 1: LevelPriority (Base Role)
- Layer 2: IsHead (Role Modifier)
- Layer 3: Permission Flags (Granular Override)

---

## Struktur Access Control

```
┌─────────────────────────────────────────────────────────────┐
│                    ACCESS CONTROL LAYER                      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Layer 1: LevelPriority (Base Role)                         │
│  ─────────────────────────────────────────────────────────   │
│  System (0)        → Full System Access                     │
│  SuperAdmin (1)    → Full Application Access               │
│  PrimaryAdmin (2)  → OPERATOR/HEAD (base access)          │
│  Primary (3)       → SECURITY PERSONNEL (base access)      │
│                                                             │
│  Layer 2: IsHead (Role Modifier)                           │
│  ─────────────────────────────────────────────────────────   │
│  false → Operator Biasa (view only)                         │
│  true  → Head Security (bisa action + approval)            │
│                                                             │
│  Layer 3: Permission Flags (Granular Override)              │
│  ─────────────────────────────────────────────────────────   │
│  CanApprovePatrol → Approve laporan patrol                 │
│  CanAlarmAction   → Alarm action (acknowledge, dispatch)   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Permission Check Logic

```
┌─────────────────────────────────────────────────────────────┐
│                    PERMISSION CHECK LOGIC                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. System (0) & SuperAdmin (1)                            │
│     └── SKIP semua checks, bisa semua                       │
│                                                             │
│  2. PrimaryAdmin (2) - Operator/Head                         │
│     ├── Basic Access (semua PrimaryAdmin):                    │
│     │   ├── Monitoring                                      │
│     │   ├── Config                                         │
│     │   ├── Patrol (VIEW) - assignment tidak butuh approval│
│     │   └── Alarm Trigger (VIEW)                            │
│     │                                                       │
│     ├── IsHead = false → Operator Biasa                    │
│     │   └── View only, tidak bisa action/approve            │
│     │                                                       │
│     └── IsHead = true → Head Security                       │
│         ├── CanApprovePatrol → Approve laporan patrol      │
│         └── CanAlarmAction → Alarm action                  │
│                                                           │
│  3. Primary (3) - Security Personnel                         │
│     └── Alarm actions (Accept, Arrived, DoneInvestigated)  │
│         langsung TANPA permission check                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Entity Structure

### UserGroup (Template)

```csharp
// File: Shared/Entities/Models/UserGroup.cs
public class UserGroup : BaseModel, IApplicationEntity
{
    // ... existing fields ...

    // Layer 2: Role Modifier
    public bool IsHead { get; set; } = false;

    // Layer 3: Permission Template (default untuk user di group)
    public bool CanApprovePatrol { get; set; } = false;
    public bool CanAlarmAction { get; set; } = false;
}
```

### User (Individual)

```csharp
// File: Shared/Entities/Models/User.cs
public class User : BaseModelWithTimeApp, IApplicationEntity
{
    // ... existing fields ...
    public Guid GroupId { get; set; }
    public UserGroup Group { get; set; }

    // Layer 3: Permission Override (nullable = inherit dari Group)
    public bool? CanApprovePatrol { get; set; }
    public bool? CanAlarmAction { get; set; }
}
```

**Catatan:**
- `null` = Inherit dari Group template
- `true` = Override: Enable (meskipun Group false)
- `false` = Override: Disable (meskipun Group true)

---

## Files yang Akan Dimodifikasi

### 1. Entity Changes

**File**: `Shared/Entities/Models/UserGroup.cs`

```csharp
// Tambahkan field berikut:
public bool IsHead { get; set; } = false;
public bool CanApprovePatrol { get; set; } = false;
public bool CanAlarmAction { get; set; } = false;
```

**File**: `Shared/Entities/Models/User.cs`

```csharp
// Tambahkan field berikut (nullable):
public bool? CanApprovePatrol { get; set; }
public bool? CanAlarmAction { get; set; }
```

### 2. DTO Changes

**File**: `Shared/Shared.Contracts/Read/UserGroupRead.cs`

```csharp
public class UserGroupRead
{
    // ... existing fields ...
    public bool IsHead { get; set; }
    public bool CanApprovePatrol { get; set; }
    public bool CanAlarmAction { get; set; }
}
```

**File**: `Shared/DataViewModels/CreateUserGroupDTO.cs` (atau UpdateUserGroupDTO)

```csharp
public class CreateUserGroupDTO
{
    // ... existing fields ...
    public bool IsHead { get; set; } = false;
    public bool CanApprovePatrol { get; set; } = false;
    public bool CanAlarmAction { get; set; } = false;
}
```

**File**: `Shared/Shared.Contracts/Read/UserRead.cs`

```csharp
public class UserRead
{
    // ... existing fields ...

    // Dari Group (template)
    public bool GroupIsHead { get; set; }
    public bool GroupCanApprovePatrol { get; set; }
    public bool GroupCanAlarmAction { get; set; }

    // Override individual (nullable)
    public bool? CanApprovePatrol { get; set; }
    public bool? CanAlarmAction { get; set; }

    // Effective value (resolved) - optional untuk frontend
    public bool EffectiveCanApprovePatrol { get; set; }
    public bool EffectiveCanAlarmAction { get; set; }
}
```

**File**: `Shared/DataViewModels/CreateUserDTO.cs` (atau UpdateUserDTO)

```csharp
public class CreateUserDTO
{
    // ... existing fields ...

    // Permission override (nullable = inherit)
    public bool? CanApprovePatrol { get; set; }
    public bool? CanAlarmAction { get; set; }
}
```

### 3. Repository Changes

**File**: `Shared/Repositories/Repository/UserGroupRepository.cs`

Update `ProjectToRead()`:
```csharp
private IQueryable<UserGroupRead> ProjectToRead(IQueryable<UserGroup> query)
{
    return query.Select(x => new UserGroupRead
    {
        Id = x.Id,
        Name = x.Name,
        LevelPriority = x.LevelPriority.ToString(),
        // ... existing fields ...
        IsHead = x.IsHead,
        CanApprovePatrol = x.CanApprovePatrol,
        CanAlarmAction = x.CanAlarmAction,
    });
}
```

**File**: `Shared/Repositories/Repository/UserRepository.cs`

Update `ProjectToRead()`:
```csharp
private IQueryable<UserRead> ProjectToRead(IQueryable<User> query)
{
    return query.Select(x => new UserRead
    {
        Id = x.Id,
        Name = x.Name,
        Level = x.Group.LevelPriority,
        // ... existing fields ...

        // Dari Group (template)
        GroupIsHead = x.Group.IsHead,
        GroupCanApprovePatrol = x.Group.CanApprovePatrol,
        GroupCanAlarmAction = x.Group.CanAlarmAction,

        // Override individual
        CanApprovePatrol = x.CanApprovePatrol,
        CanAlarmAction = x.CanAlarmAction,

        // Effective value (resolved)
        EffectiveCanApprovePatrol = x.CanApprovePatrol ?? x.Group.CanApprovePatrol,
        EffectiveCanAlarmAction = x.CanAlarmAction ?? x.Group.CanAlarmAction,
    });
}
```

### 4. Helper Extension

**File**: `Shared/BusinessLogic.Services/Extension/PermissionExtension.cs` (BARU)

```csharp
using Shared.Entities.Models;
using Shared.Shared.Contracts.Read;
using Shared.Shared.Contracts;

namespace Shared.BusinessLogic.Services.Extension
{
    public static class PermissionExtension
    {
        /// <summary>
        /// Mendapatkan effective permission (template + override)
        /// </summary>
        public static bool GetEffectivePermission(
            this UserRead user,
            Func<UserGroupRead, bool> groupTemplate,
            bool? userOverride)
        {
            // Jika user ada override, pakai override
            if (userOverride.HasValue)
                return userOverride.Value;

            // Jika tidak ada Group, default false
            if (user.Group == null)
                return false;

            // Inherit dari group
            return groupTemplate(user.Group);
        }

        /// <summary>
        /// Mendapatkan effective permission untuk entity langsung
        /// </summary>
        public static bool GetEffectivePermission(
            this User user,
            Func<UserGroup, bool> groupTemplate,
            bool? userOverride)
        {
            if (userOverride.HasValue)
                return userOverride.Value;

            return user.Group != null && groupTemplate(user.Group);
        }

        /// <summary>
        /// Cek apakah user bisa approve patrol (Layer 1 + 2 + 3)
        /// </summary>
        public static bool CanApprovePatrol(this UserRead user)
        {
            // Layer 1: System/SuperAdmin bypass
            if (user.Level <= (int)LevelPriority.SuperAdmin)
                return true;

            // Layer 2: Harus PrimaryAdmin
            if (user.Level != (int)LevelPriority.PrimaryAdmin)
                return false;

            // Layer 2: Harus IsHead
            if (!user.GroupIsHead)
                return false;

            // Layer 3: Cek permission
            return user.GetEffectivePermission(
                g => g.CanApprovePatrol,
                user.CanApprovePatrol
            );
        }

        /// <summary>
        /// Cek apakah user bisa alarm action (Layer 1 + 3)
        /// </summary>
        public static bool CanAlarmAction(this UserRead user)
        {
            // Layer 1: System/SuperAdmin bypass
            if (user.Level <= (int)LevelPriority.SuperAdmin)
                return true;

            // Layer 2: Harus PrimaryAdmin
            if (user.Level != (int)LevelPriority.PrimaryAdmin)
                return false;

            // Layer 3: Cek permission
            return user.GetEffectivePermission(
                g => g.CanAlarmAction,
                user.CanAlarmAction
            );
        }
    }
}
```

### 5. Service Changes

**File**: `Shared/BusinessLogic.Services/Implementation/PatrolCaseService.cs`

Update `ApproveAsync`:
```csharp
public async Task<PatrolCaseRead> ApproveAsync(Guid caseId)
{
    var currentUser = await _userService.GetFromTokenAsync();

    // Cek permission menggunakan extension method
    if (!currentUser.CanApprovePatrol())
        throw new UnauthorizedException("Anda tidak memiliki akses approve patrol");

    // ... rest of logic
}
```

**File**: `Shared/BusinessLogic.Services/Implementation/AlarmTriggersService.cs`

Update action methods (AcknowledgeAsync, DispatchAsync, dll):
```csharp
public async Task<AlarmTriggerRead> AcknowledgeAsync(Guid triggerId)
{
    var currentUser = await _userService.GetFromTokenAsync();

    // Cek permission menggunakan extension method
    if (!currentUser.CanAlarmAction())
        throw new UnauthorizedException("Anda tidak memiliki akses action alarm");

    // ... rest of logic
}
```

**File**: `Shared/BusinessLogic.Services/Implementation/UserGroupService.cs`

Update `CreateAsync` dan `UpdateAsync` untuk handle field baru.

**File**: `Shared/BusinessLogic.Services/Implementation/UserService.cs`

Update `CreateAsync` dan `UpdateAsync` untuk handle permission override.

### 6. Migration

**Buat migration baru** (via CLI):
```bash
dotnet ef migrations add AddPermissionFlags --project Shared/Repositories/Repositories.csproj --startup-project Services.API/Auth/Auth.csproj
```

**Migration content** (auto-generated, tapi kurang lebih):

```csharp
// Di UserGroup table
migrationBuilder.AddColumn<bool>(
    name: "IsHead",
    table: "UserGroup",
    type: "bit",
    defaultValue: false);

migrationBuilder.AddColumn<bool>(
    name: "CanApprovePatrol",
    table: "UserGroup",
    type: "bit",
    defaultValue: false);

migrationBuilder.AddColumn<bool>(
    name: "CanAlarmAction",
    table: "UserGroup",
    type: "bit",
    defaultValue: false);

// Di User table
migrationBuilder.AddColumn<bool>(
    name: "CanApprovePatrol",
    table: "User",
    type: "bit",
    nullable: true);

migrationBuilder.AddColumn<bool>(
    name: "CanAlarmAction",
    table: "User",
    type: "bit",
    nullable: true);
```

---

## Implementation Steps

1. **Tambahkan field ke UserGroup entity**
   - `IsHead`, `CanApprovePatrol`, `CanAlarmAction`

2. **Tambahkan field ke User entity**
   - `CanApprovePatrol?`, `CanAlarmAction?` (nullable)

3. **Buat migration**
   ```bash
   dotnet ef migrations add AddPermissionFlags --project Shared/Repositories/Repositories.csproj --startup-project Services.API/Auth/Auth.csproj
   dotnet ef database update --project Shared/Repositories/Repositories.csproj --startup-project Services.API/Auth/Auth.csproj
   ```

4. **Update DTOs**
   - UserGroupRead, CreateUserGroupDTO/UpdateUserGroupDTO
   - UserRead, CreateUserDTO/UpdateUserDTO

5. **Update Repositories**
   - UserGroupRepository.ProjectToRead()
   - UserRepository.ProjectToRead()

6. **Buat PermissionExtension**
   - Helper method untuk permission check

7. **Update Services**
   - PatrolCaseService.ApproveAsync
   - AlarmTriggersService action methods
   - UserGroupService.CreateAsync/UpdateAsync
   - UserService.CreateAsync/UpdateAsync

8. **Update Controllers** (kalau perlu)
   - Kalau ada endpoint untuk update permission

---

## Verification

1. **Database Check**:
   - Pastikan column baru berhasil dibuat
   - Default value = 0 untuk UserGroup
   - Nullable untuk User

2. **Create UserGroup**:
   - Bisa set IsHead, CanApprovePatrol, CanAlarmAction
   - Value tersimpan benar

3. **Create User**:
   - Bisa set permission override (nullable)
   - null = inherit dari Group

4. **Patrol Approve Test**:
   - System/SuperAdmin → bisa approve
   - PrimaryAdmin, IsHead=false, CanApprovePatrol=false → tidak bisa
   - PrimaryAdmin, IsHead=true, CanApprovePatrol=true → bisa
   - PrimaryAdmin, IsHead=true, CanApprovePatrol=false → tidak bisa
   - PrimaryAdmin, IsHead=true, CanApprovePatrol=null → inherit dari Group
   - Primary (3) → tidak bisa (bukan PrimaryAdmin)

5. **Alarm Action Test**:
   - System/SuperAdmin → bisa action
   - PrimaryAdmin, CanAlarmAction=true → bisa
   - PrimaryAdmin, CanAlarmAction=false → tidak bisa
   - Primary (3) → bisa action (Accept, Arrived, dll) tanpa flag

---

## Summary

| Layer | Component | Purpose |
|-------|-----------|---------|
| 1 | LevelPriority | Base role access |
| 2 | IsHead | Role modifier (Operator vs Head) |
| 3 | Permission Flags | Granular override (Template + Override) |

**Key Points:**
- System/SuperAdmin bypass semua checks
- Primary (3) untuk alarm actions langsung bisa tanpa flag
- Nullable bool di User: `null` = inherit, `true/false` = override
- Gunakan extension method `CanApprovePatrol()`, `CanAlarmAction()` untuk clean code
