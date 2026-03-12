# BLE Tracking Backend - Analisis Potensi Error & Bug

**Tanggal**: 2026-03-12
**Versi**: 1.0
**Scope**: Seluruh codebase (30+ microservices)

---

## Daftar Isi

1. [Ringkasan Eksekutif](#ringkasan-eksekutif)
2. [Hierarki Analisis](#hierarki-analisis)
3. [Critical Issues (P1)](#critical-issues-p1)
4. [High Severity Issues (P2)](#high-severity-issues-p2)
5. [Medium Severity Issues (P3)](#medium-severity-issues-p3)
6. [Low Severity Issues (P4)](#low-severity-issues-p4)
7. [Pattern Violations](#pattern-violations)
8. [Rekomendasi Perbaikan](#rekomendasi-perbaikan)

---

## Ringkasan Eksekutif

### Statistik Masalah

| Severity | Jumlah | Status |
|----------|--------|--------|
| **Critical (P1)** | 8 | Membutuhkan perbaikan segera |
| **High (P2)** | 12 | Prioritas tinggi |
| **Medium (P3)** | 15 | Perlu diperbaiki |
| **Low (P4)** | 8 | Improvement |
| **TOTAL** | **43** | |

### Distribusi per Layer

| Layer | Critical | High | Medium | Low | Total |
|-------|----------|------|--------|-----|-------|
| **Core/Shared** | 2 | 3 | 3 | 3 | 11 |
| **Master Services** | 3 | 4 | 3 | 2 | 12 |
| **Transaction Services** | 3 | 5 | 9 | 3 | 20 |

---

## Hierarki Analisis

```
BLE Tracking Backend
│
├── LAYER 1: Core/Shared (Foundation)  [11 Issues]
│   ├── BaseRepository.cs              [2 Critical, 2 High, 2 Medium]
│   ├── BaseService.cs                 [1 Critical, 1 Medium]
│   ├── Extensions                     [1 Medium]
│   └── Shared.Contracts               [3 Low]
│
├── LAYER 2: Master Data Services      [12 Issues]
│   ├── MstApplication                 [1 Critical, 1 High]
│   ├── MstBuilding                    [1 High]
│   ├── MstFloor                       [1 Critical, 2 High]
│   └── MstFloorplan                   [1 Critical, 2 High, 1 Medium]
│
└── LAYER 3: Transaction Services      [20 Issues]
    ├── Card                           [1 Critical, 2 High, 3 Medium]
    ├── Patrol                         [1 High, 2 Medium]
    ├── Visitor                        [1 Critical, 2 High, 4 Medium]
    ├── TrackingTransaction            [1 High, 2 Medium]
    └── AlarmRecordTracking            [1 Critical, 1 High]
```

---

## Critical Issues (P1)

### C-01: Multi-tenancy Bypass Vulnerability
**Layer**: Core/Shared - BaseRepository
**File**: `Shared/Repositories/Repository/BaseRepository.cs:291-296`
**Severity**: 🚨 CRITICAL
**Category**: Security

**Deskripsi**:
SystemAdmin check dalam `GetApplicationIdAndRole()` mem-bypass semua filter multi-tenancy tanpa validasi tambahan.

```csharp
if (isSystemAdmin)
    return (null, true); // system ga perlu filter applicationId
```

**Dampak**:
- Privilege escalation jika token compromised
- Cross-tenant data access vulnerability
- Tidak ada validasi bahwa SystemAdmin exists di database

**Rekomendasi**:
```csharp
// Tambah validasi SystemAdmin di database
if (isSystemAdmin)
{
    var userId = Guid.Parse(Http.HttpContext?.User.FindFirst("sub")?.Value);
    var user = await _context.Users.FindAsync(userId);
    if (user?.Group?.Name != "System")
        throw new UnauthorizedException("Invalid system admin token");
    return (null, true);
}
```

---

### C-02: AppId Null Reference Exception
**Layer**: Core/Shared - BaseService
**File**: `Shared/BusinessLogic.Services/Implementation/BaseService.cs:25-30`
**Severity**: 🚨 CRITICAL
**Category**: Crash

**Deskripsi**:
Property `AppId` throws generic Exception ketika ApplicationId missing, tapi tidak handle HttpContext null.

```csharp
var appId = Http.HttpContext?.User.FindFirst("ApplicationId")?.Value;
if (string.IsNullOrEmpty(appId))
    throw new Exception("ApplicationId missing in token"); // NullReference if HttpContext is null
```

**Dampak**:
- Application crash jika HttpContext null
- Error message tidak helpful untuk debugging

**Rekomendasi**:
```csharp
protected Guid AppId
{
    get
    {
        if (Http.HttpContext?.User == null)
            throw new UnauthorizedException("No HTTP context available");

        var appId = Http.HttpContext.User.FindFirst("ApplicationId")?.Value;
        if (string.IsNullOrEmpty(appId))
            throw new UnauthorizedException("ApplicationId claim missing in token");

        if (!Guid.TryParse(appId, out var result))
            throw new UnauthorizedException("Invalid ApplicationId format");

        return result;
    }
}
```

---

### C-03: Missing Ownership Validation - MstFloor
**Layer**: Master Data - MstFloor Service
**File**: `Shared/BusinessLogic.Services/Implementation/MstFloorService.cs`
**Severity**: 🚨 CRITICAL
**Category**: Security

**Deskripsi**:
Repository memiliki `CheckInvalidBuildingOwnershipAsync` tapi Service tidak memanggilnya di Create/Update.

**Dampak**:
- User bisa create Floor yang meng-reference Building dari application lain
- Cross-tenant data contamination

**Rekomendasi**:
```csharp
// Di MstFloorService.CreateAsync
var invalidBuildingIds = await _repository.CheckInvalidBuildingOwnershipAsync(
    dto.BuildingId, AppId);
if (invalidBuildingIds.Any())
    throw new UnauthorizedException(
        $"BuildingId does not belong to this Application: {dto.BuildingId}");
```

---

### C-04: Missing Ownership Validation - MstFloorplan
**Layer**: Master Data - MstFloorplan Service
**File**: `Shared/BusinessLogic.Services/Implementation/MstFloorplanService.cs`
**Severity**: 🚨 CRITICAL
**Category**: Security

**Deskripsi**:
Repository memiliki `CheckInvalidFloorOwnershipAsync` tapi Service tidak memanggilnya di Create/Update.

**Dampak**:
- User bisa create Floorplan yang meng-reference Floor dari application lain
- Cross-tenant data contamination

**Rekomendasi**:
```csharp
// Di MstFloorplanService.CreateAsync
var invalidFloorIds = await _repository.CheckInvalidFloorOwnershipAsync(
    dto.FloorId, AppId);
if (invalidFloorIds.Any())
    throw new UnauthorizedException(
        $"FloorId does not belong to this Application: {dto.FloorId}");
```

---

### C-05: Missing FilterAsync - CardRepository
**Layer**: Transaction - Card Service
**File**: `Shared/Repositories/Repository/CardRepository.cs`
**Severity**: 🚨 CRITICAL
**Category**: Functionality

**Deskripsi**:
CardRepository tidak memiliki method `FilterAsync` sama sekali, hanya `GetAllExportAsync`.

**Dampak**:
- Filtering functionality mungkin tidak bekerja dengan benar
- Tidak mengikuti standard pattern

**Rekomendasi**:
```csharp
public async Task<(List<CardRead> Data, int Total, int Filtered)> FilterAsync(
    CardFilter filter)
{
    var query = BaseEntityQuery();

    // Extract ID filters
    var memberIds = ExtractIds(filter.MemberId);
    if (memberIds.Count > 0)
        query = query.Where(x => x.MemberId.HasValue && memberIds.Contains(x.MemberId.Value));

    // ... rest of filters

    var total = await query.CountAsync();
    var filtered = await query.CountAsync();

    query = query.ApplySorting(filter.SortColumn, filter.SortDir);
    query = query.ApplyPaging(filter.Page, filter.PageSize);

    var data = await ProjectToRead(query).ToListAsync();
    return (data, total, filtered);
}
```

---

### C-06: No ProjectToRead - VisitorRepository
**Layer**: Transaction - Visitor Service
**File**: `Shared/Repositories/Repository/VisitorRepository.cs`
**Severity**: 🚨 CRITICAL
**Category**: Architecture

**Deskripsi**:
VisitorRepository tidak memiliki method `ProjectToRead` dan menggunakan `GenericDataTableService`.

**Dampak**:
- Tidak mengikuti single source of truth principle
- Potential N+1 query problems
- Inconsistent data responses

**Rekomendasi**:
- Implement `ProjectToRead` method dengan manual projection
- Implement `FilterAsync` method
- Remove usage of `GenericDataTableService`

---

### C-07: Missing Ownership Validation - Card Service
**Layer**: Transaction - Card Service
**File**: `Shared/BusinessLogic.Services/Implementation/CardService.cs:377, 411`
**Severity**: 🚨 CRITICAL
**Category**: Security

**Deskripsi**:
Methods `UpdateAccessByVMSAsync` dan `SwapCard` tidak melakukan ownership validation.

**Dampak**:
- Bisa allow assign access ke external entities
- Cross-tenant security vulnerability

**Rekomendasi**:
```csharp
// Tambah ownership check sebelum update
if (card.MemberId.HasValue)
{
    var invalidIds = await _repository.CheckInvalidMemberOwnershipAsync(
        card.MemberId.Value, AppId);
    if (invalidIds.Any())
        throw new UnauthorizedException("MemberId validation failed");
}
```

---

### C-08: Incomplete Implementation - AlarmRecordTracking
**Layer**: Transaction - Alarm Service
**File**: `Shared/Repositories/Repository/AlarmRecordTrackingRepository.cs:277-280`
**Severity**: 🚨 CRITICAL
**Category**: Functionality

**Deskripsi**:
TODO comments untuk building access filter, base methods commented out.

**Dampak**:
- Filtering tidak bekerja sebagaimana mestinya
- Security hole di building-level access control

**Rekomendasi**:
- Un-comment dan fix base methods
- Implement building access filter
- Tambah ownership validation

---

## High Severity Issues (P2)

### H-01: ExtractIds Security Issue
**Layer**: Core/Shared - BaseRepository
**File**: `Shared/Repositories/Repository/BaseRepository.cs:255-280`
**Severity**: HIGH
**Category**: Robustness

**Deskripsi**:
ExtractIds method tidak validate GUID format sebelum parsing, bisa throw FormatException.

**Rekomendasi**:
```csharp
if (Guid.TryParse(raw, out var singleId) && singleId != Guid.Empty)
    ids.Add(singleId);
```

---

### H-02: Race Condition in Transaction
**Layer**: Core/Shared - BaseRepository
**File**: `Shared/Repositories/Repository/BaseRepository.cs:86-121`
**Severity**: HIGH
**Category**: Data Integrity

**Deskripsi**:
`ExecuteInTransactionAsync` creates new transaction even if one exists, tidak handle nested transactions.

**Rekomendasi**:
- Use `IDbContextTransaction` for proper transaction handling
- Implement transaction scope for nested transactions

---

### H-03: Performance Issue - Exception Middleware
**Layer**: Core/Shared - CustomExceptionMiddleware
**File**: `Shared/DataViewModels/Shared/ExceptionHelper/CustomExceptionMiddleware.cs:150-153`
**Severity**: HIGH
**Category**: Performance

**Deskripsi**:
Selalu uses Json camelCase regardless of environment, increases serialization overhead.

---

### H-04: [AllowAnonymous] on Export Endpoints
**Layer**: Master Data - MstApplication
**File**: `Shared/Web.API.Controllers/Controllers/MstApplicationController.cs`
**Severity**: HIGH
**Category**: Security

**Deskripsi**:
Export endpoints memiliki `[AllowAnonymous]` yang dangerous.

---

### H-05: Inconsistent Exception Handling
**Layer**: Master Data - Multiple Services
**Severity**: HIGH
**Category**: Consistency

**Deskripsi**:
Mix of `NotFoundException`, `BusinessException`, dan `KeyNotFoundException`.

---

### H-06: Missing BaseEntityQuery - MstFloor
**Layer**: Master Data - MstFloor Repository
**File**: `Shared/Repositories/Repository/MstFloorRepository.cs`
**Severity**: HIGH
**Category**: Architecture

**Deskripsi**:
Tidak memiliki proper multi-tenancy di BaseEntityQuery.

---

### H-07: Duplicate Projection - MstFloor
**Layer**: Master Data - MstFloor Service
**Severity**: HIGH
**Category**: Architecture

**Deskripsi**:
Service melakukan manual projection saat repository sudah return Read DTO.

---

### H-08: SoftDeleteAsync Not Working - MstFloor
**Layer**: Master Data - MstFloor Repository
**Severity**: HIGH
**Category**: Functionality

**Deskripsi**:
SoftDeleteAsync tidak benar-benar delete (hanya save tanpa changes).

---

### H-09: Missing Ownership Validation - PatrolCase
**Layer**: Transaction - Patrol Service
**File**: `Shared/Repositories/Repository/PatrolCaseRepository.cs`
**Severity**: HIGH
**Category**: Security

**Deskripsi**:
Tidak ada `CheckInvalid[Related]OwnershipAsync` methods untuk Security entity.

---

### H-10: No FilterAsync - VisitorRepository
**Layer**: Transaction - Visitor Service
**Severity**: HIGH
**Category**: Functionality

**Deskripsi**:
Tidak implement FilterAsync method, menggunakan generic service.

---

### H-11: Missing Ownership Validation - TrackingTransaction
**Layer**: Transaction - Tracking Service
**Severity**: HIGH
**Category**: Security

**Deskripsi**:
Relasi ke Member, Visitor, Card tapi tidak ada ownership validation.

---

### H-12: No FilterAsync - AlarmRecordTracking
**Layer**: Transaction - Alarm Service
**Severity**: HIGH
**Category**: Functionality

**Deskripsi**:
Repository tidak implement standard FilterAsync pattern.

---

## Medium Severity Issues (P3)

### M-01: Inconsistent Authentication Check
**Layer**: Core/Shared - BaseRepository
**File**: `Shared/Repositories/Repository/BaseRepository.cs:34-60`
**Severity**: MEDIUM
**Category**: Consistency

**Deskripsi**:
GetApplicationIdAndRole returns different values tergantung authentication type (token vs integration).

---

### M-02: Duplicate Audit Field Updates
**Layer**: Core/Shared - BaseService
**File**: `Shared/BusinessLogic.Services/Implementation/BaseService.cs:83-93`
**Severity**: MEDIUM
**Category**: Audit

**Deskripsi**:
SetCreateAudit sets both CreatedBy/UpdatedBy - violates audit principle.

---

### M-03: Hardcoded Error Messages
**Layer**: Core/Shared - AuthExtensions
**File**: `Shared/BusinessLogic.Services/Extension/RootExtension/AuthExtensions.cs:65-66`
**Severity**: MEDIUM
**Category**: Maintainability

**Deskripsi**:
Error messages hardcoded, should be configurable.

---

### M-04: Missing ExtractIds Usage - MstFloor
**Layer**: Master Data - MstFloor Repository
**Severity**: MEDIUM
**Category**: Pattern

**Deskripsi**:
Manual JsonElement parsing instead of using ExtractIds helper.

---

### M-05: Manual JsonElement Parsing - MstFloorplan
**Layer**: Master Data - MstFloorplan Repository
**Severity**: MEDIUM
**Category**: Pattern

**Deskripsi**:
Manual parsing instead of using ExtractIds helper.

---

### M-06: Duplicate Code - Filter Endpoint
**Layer**: Master Data - Multiple Controllers
**Severity**: MEDIUM
**Category**: DRY

**Deskripsi**:
Filter endpoint deserialization duplicated in multiple places.

---

### M-07: Missing GetByIdEntityAsync Usage
**Layer**: Master Data - Multiple Repositories
**Severity**: MEDIUM
**Category**: Pattern

**Deskripsi**:
DeleteAsync tidak menggunakan GetByIdEntityAsync pattern.

---

### M-08: Inconsistent Audit Naming
**Layer**: Transaction - Card Service
**File**: `Shared/BusinessLogic.Services/Implementation/CardService.cs:305`
**Severity**: MEDIUM
**Category**: Audit

**Deskripsi**:
Mix of "Card" dan "Patrol Area" dalam audit calls.

---

### M-09: Complex Approval Logic Without Validation
**Layer**: Transaction - Patrol Service
**Severity**: MEDIUM
**Category**: Security

**Deskripsi**:
Approval flow lacks ownership validation untuk assigned heads.

---

### M-10: No BaseService Features
**Layer**: Transaction - Visitor Service
**Severity**: MEDIUM
**Category**: Architecture

**Deskripsi**:
Service tidak fully utilize BaseService capabilities.

---

### M-11: GenericDataTableService Usage
**Layer**: Transaction - Visitor Service
**Severity**: MEDIUM
**Category**: Architecture

**Deskripsi**:
Using GenericDataTableService instead of manual projection pattern.

---

### M-12: Missing Validation - Tracking
**Layer**: Transaction - Tracking Service
**Severity**: MEDIUM
**Category**: Security

**Deskripsi**:
No ValidateRelatedEntitiesAsync untuk cross-application checks.

---

### M-13: Commented Code - Alarm
**Layer**: Transaction - Alarm Service
**Severity**: MEDIUM
**Category**: Maintainability

**Deskripsi**:
Base methods commented out suggests incomplete implementation.

---

### M-14: Missing Multi-Tenancy - MstFloor
**Layer**: Master Data - MstFloor
**Severity**: MEDIUM
**Category**: Security

**Deskripsi**:
BaseEntityQuery tidak properly handle multi-tenancy di semua cases.

---

### M-15: Missing Ownership Validation Service Call
**Layer**: Master Data - MstFloorplan Service
**Severity**: MEDIUM
**Category**: Security

**Deskripsi**:
Service tidak memanggil ownership validation helpers di Create/Update.

---

## Low Severity Issues (P4)

### L-01: Missing Property Validation - BaseFilter
**Layer**: Core/Shared - BaseFilter
**File**: `Shared/Shared.Contracts/Read/BaseFilter.cs:11-14`
**Severity**: LOW
**Category**: Validation

**Deskripsi**:
No validation untuk PageSize limit, bisa lead ke excessive memory usage.

---

### L-02: Inconsistent JsonIgnore Usage - BaseRead
**Layer**: Core/Shared - BaseRead
**File**: `Shared/Shared.Contracts/Read/BaseRead.cs:13-24`
**Severity**: LOW
**Category**: API Consistency

**Deskripsi**:
All audit fields are JsonIgnore, tapi Status dan ApplicationId mungkin perlu exposed di beberapa APIs.

---

### L-03: Inconsistent Environment Checks - Exception Middleware
**Layer**: Core/Shared - CustomExceptionMiddleware
**File**: `Shared/DataViewModels/Shared/ExceptionHelper/CustomExceptionMiddleware.cs:111-117`
**Severity**: LOW
**Category**: Logging

**Deskripsi**:
Environment checks commented out tapi masih gunakan environment-specific logic inconsistently.

---

### L-04: Missing Max PageSize
**Layer**: Core/Shared - BaseFilter
**Severity**: LOW
**Category**: Performance

**Deskripsi**:
PageSize default 10 tapi no max limit.

---

### L-05: Duplicate Filter Endpoint Code
**Layer**: Master Data - Multiple Controllers
**Severity**: LOW
**Category**: Code Quality

**Deskripsi**:
Filter endpoint deserialization logic duplicated.

---

### L-06: SoftDelete Pattern Inconsistency
**Layer**: Master Data - Multiple Repositories
**Severity**: LOW
**Category**: Pattern

**Deskripsi**:
SoftDelete implementation berbeda antar repositories.

---

### L-07: Missing Null Checks in Projection
**Layer**: Master Data - Multiple Repositories
**Severity**: LOW
**Category**: Robustness

**Deskripsi**:
ProjectToRead methods kurang comprehensive null checking.

---

### L-08: Inconsistent Service Registration
**Layer**: Multiple Services - Program.cs
**Severity**: LOW
**Category**: Configuration

**Deskripsi**:
Beberapa services kurang complete dependency registration.

---

## Pattern Violations

### Anti-Pattern: Duplicate Select Projection

**Violation**: Menggunakan manual `.Select()` di FilterAsync instead of `ProjectToRead()`

**Found in**:
- MstFloor (Service layer)
- MstFloorplan (Service layer)

**Impact**:
- Two sources of truth untuk same projection
- Maintenance nightmare
- Bug-prone

---

### Anti-Pattern: Missing ExtractIds Usage

**Violation**: Manual JsonElement parsing instead of using `ExtractIds()`

**Found in**:
- MstFloorRepository
- MstFloorplanRepository

**Correct pattern**:
```csharp
// ❌ WRONG
var buildingIds = filter.BuildingId.GetRawText();
if (Guid.TryParse(buildingIds, out var bid))
    query = query.Where(x => x.BuildingId == bid);

// ✅ CORRECT
var buildingIds = ExtractIds(filter.BuildingId);
if (buildingIds.Count > 0)
    query = query.Where(x => buildingIds.Contains(x.BuildingId));
```

---

### Anti-Pattern: Missing Ownership Validation

**Violation**: Tidak memanggil `CheckInvalid[Related]OwnershipAsync` di Create/Update

**Found in**:
- MstFloorService
- MstFloorplanService
- CardService (partial - missing in some methods)
- PatrolCaseService
- VisitorService
- TrackingTransactionService

**Correct pattern**:
```csharp
// Di Service CreateAsync
var invalidIds = await _repository.CheckInvalidXxxOwnershipAsync(
    dto.XxxId, AppId);
if (invalidIds.Any())
    throw new UnauthorizedException($"XxxId does not belong to this Application");
```

---

### Anti-Pattern: Mapper Usage When Repository Returns Read DTO

**Violation**: Menggunakan `_mapper.Map()` saat repository sudah return Read DTO

**Found in**:
- MstFloorService
- MstFloorplanService

**Correct pattern**:
```csharp
// ❌ WRONG
var entity = await _repository.GetByIdAsync(id);
return _mapper.Map<MstFloorDto>(entity);  // Double mapping!

// ✅ CORRECT
var entity = await _repository.GetByIdAsync(id);
return entity;  // Direct return, no mapper
```

---

## Rekomendasi Perbaikan

### Phase 1: Critical Security Fixes (Week 1)

1. **Fix Multi-tenancy Bypass** (C-01)
   - Tambah validasi SystemAdmin di database
   - File: `BaseRepository.cs:291-296`

2. **Fix AppId Null Reference** (C-02)
   - Tambah proper null checking dan error handling
   - File: `BaseService.cs:25-30`

3. **Implement Ownership Validation** (C-03, C-04, C-07, C-09, C-11)
   - MstFloor: Add validation calls in Create/Update
   - MstFloorplan: Add validation calls in Create/Update
   - Card: Add validation in UpdateAccessByVMSAsync, SwapCard
   - Patrol: Add ownership validation helpers
   - Tracking: Add ownership validation helpers

4. **Secure Export Endpoints** (H-04)
   - Remove `[AllowAnonymous]` from export endpoints

### Phase 2: Architecture Standardization (Week 2-3)

1. **Implement Missing FilterAsync** (C-05, H-10, H-12)
   - CardRepository: Add FilterAsync method
   - VisitorRepository: Add FilterAsync + ProjectToRead
   - AlarmRecordTrackingRepository: Implement standard pattern

2. **Fix Duplicate Projection** (M-04, M-05, M-06)
   - Replace manual JsonElement parsing with ExtractIds
   - Remove duplicate Select projections
   - Use ProjectToRead consistently

3. **Standardize Exception Handling** (H-05)
   - Use `NotFoundException` consistently
   - Remove KeyNotFoundException usage
   - Standardize error messages

### Phase 3: Data Integrity & Performance (Week 4)

1. **Fix Transaction Handling** (H-02)
   - Implement proper nested transaction support
   - Use IDbContextTransaction correctly

2. **Improve ExtractIds Robustness** (H-01)
   - Add null and empty GUID checks
   - Better error handling

3. **Optimize Exception Middleware** (H-03)
   - Cache JsonSerializerOptions
   - Environment-aware serialization

### Phase 4: Code Quality & Maintainability (Week 5+)

1. **Fix Audit Trail** (M-02, M-08)
   - Remove UpdatedBy set on create
   - Standardize audit naming

2. **Remove GenericDataTableService** (M-11)
   - Convert to manual projection pattern

3. **Add Validation** (L-01, L-04)
   - Add max PageSize limit
   - Add property validation

4. **Clean Up Commented Code** (M-13, L-03)
   - Remove or uncomment properly
   - Fix incomplete implementations

---

## Testing Recommendations

Setelah perbaikan, lakukan testing berikut:

### 1. Security Testing
- Cross-tenant data access attempt
- Privilege escalation scenarios
- Token validation edge cases

### 2. Functional Testing
- Filter functionality untuk semua services
- CRUD operations dengan relasi
- Export functionality

### 3. Performance Testing
- Query performance dengan large dataset
- Memory usage dengan large PageSize
- Transaction throughput

### 4. Integration Testing
- End-to-end flow dari Auth → Master → Transaction
- MQTT message handling
- Alarm trigger evaluation

---

## Appendix: File Reference Index

### Core/Shared Layer
- `Shared/Repositories/Repository/BaseRepository.cs`
- `Shared/BusinessLogic.Services/Implementation/BaseService.cs`
- `Shared/Shared.Contracts/Read/BaseRead.cs`
- `Shared/Shared.Contracts/Read/BaseFilter.cs`
- `Shared/DataViewModels/Shared/ExceptionHelper/CustomExceptionMiddleware.cs`
- `Shared/BusinessLogic.Services/Extension/RootExtension/AuthExtensions.cs`

### Master Services
- `Shared/Repositories/Repository/MstApplicationRepository.cs`
- `Shared/Repositories/Repository/MstBuildingRepository.cs`
- `Shared/Repositories/Repository/MstFloorRepository.cs`
- `Shared/Repositories/Repository/MstFloorplanRepository.cs`
- `Shared/BusinessLogic.Services/Implementation/MstApplicationService.cs`
- `Shared/BusinessLogic.Services/Implementation/MstBuildingService.cs`
- `Shared/BusinessLogic.Services/Implementation/MstFloorService.cs`
- `Shared/BusinessLogic.Services/Implementation/MstFloorplanService.cs`
- `Shared/Web.API.Controllers/Controllers/MstApplicationController.cs`
- `Shared/Web.API.Controllers/Controllers/MstBuildingController.cs`
- `Shared/Web.API.Controllers/Controllers/MstFloorController.cs`
- `Shared/Web.API.Controllers/Controllers/MstFloorplanController.cs`

### Transaction Services
- `Shared/Repositories/Repository/CardRepository.cs`
- `Shared/Repositories/Repository/VisitorRepository.cs`
- `Shared/Repositories/Repository/PatrolCaseRepository.cs`
- `Shared/Repositories/Repository/TrackingTransactionRepository.cs`
- `Shared/Repositories/Repository/AlarmRecordTrackingRepository.cs`
- `Shared/BusinessLogic.Services/Implementation/CardService.cs`
- `Shared/BusinessLogic.Services/Implementation/VisitorService.cs`
- `Shared/BusinessLogic.Services/Implementation/PatrolCaseService.cs`
- `Shared/BusinessLogic.Services/Implementation/TrackingTransactionService.cs`
- `Shared/BusinessLogic.Services/Implementation/AlarmRecordTrackingService.cs`

---

**End of Report**
