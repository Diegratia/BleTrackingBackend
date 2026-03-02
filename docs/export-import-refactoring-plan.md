# Export/Import Standardization Refactoring Plan

## Context

Export/Import logic is duplicated across **23 services** with export functionality and **12 services** with import functionality. Each service has 80-100+ lines of duplicate QuestPDF/ClosedXML code with:

- Hardcoded column definitions
- Manual table styling
- No pagination support (returns ALL data - performance risk)
- Inconsistent error handling
- Mixed responsibilities (business logic mixed with presentation)

**Current Issues:**
- No pagination in exports (exports all data even if millions of rows)
- Code duplication: same PDF/Excel generation logic in 23+ places
- Import validation scattered across services
- No standardized error messages for import failures
- Maintenance burden: changing column style requires updating 23 files

## Scope

### Initial Implementation (Proof of Concept):
**3 Services Only**: MstBuilding, MstFloor, MstBleReader

These 3 services will serve as the template. Once validated, the pattern can be extended to the other 20+ services.

### Future Services (After Validation):
Remaining 20 services with export: MstFloorplan, Card, CardRecord, Visitor, TrxVisitor, TrackingTransaction, AlarmRecordTracking, FloorplanDevice, FloorplanMaskedArea, MstSecurity, MstOrganization, MstDepartment, MstDistrict, MstAccessCctv, MstAccessControl, MstApplication, MstIntegration, MstBrand, MstMember, + analytics services

## Solution: Create Generic Export/Import Infrastructure

### Phase 1: Create Export Service

#### 1.1 Create Export Configuration DTOs
**File**: `Shared/Shared.Contracts/Reporting/ReportMetadata.cs`

```csharp
namespace Shared.Contracts.Reporting;

public class ReportMetadata
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? FilterInfo { get; set; }
    public int TotalRecords { get; set; }
    public List<ReportColumn> Columns { get; set; } = new();
    public ReportOrientation Orientation { get; set; } = ReportOrientation.Landscape;
    public bool IncludeRowNumbers { get; set; } = true;
    public bool IncludePageNumbers { get; set; } = true;
    public string? TimeZone { get; set; } = "UTC";
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}

public class ReportColumn
{
    public string Header { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public int Width { get; set; } = 2; // Relative width for PDF
    public string? Format { get; set; } // Optional format string (e.g., "yyyy-MM-dd HH:mm:ss")
    public ColumnAlign Align { get; set; } = ColumnAlign.Left;
}

public enum ReportOrientation { Portrait, Landscape }
public enum ColumnAlign { Left, Center, Right }
```

#### 1.2 Create Export Service Interface
**File**: `Shared/BusinessLogic.Services/Interface/IReportExportService.cs`

```csharp
using Shared.Contracts.Reporting;

namespace BusinessLogic.Services.Interface;

public interface IReportExportService
{
    Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, ReportMetadata metadata);
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, ReportMetadata metadata);

    // Helper methods (inspired by TrackingSessionService pattern)
    string GenerateReportTitle(string baseTitle, string? customTitle, string? timeRange);
    string GenerateFilterInfo(ReportFilterInfo filters);
    ReportExportRequest ApplyDefaultPagination(ReportExportRequest? request);
}

// For filter info generation
public class ReportFilterInfo
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? TimeRange { get; set; }
    public Guid? BuildingId { get; set; }
    public Guid? FloorId { get; set; }
    public Guid? AreaId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
    public int? Status { get; set; }
}
```

#### 1.3 Create Export Service Implementation
**File**: `Shared/BusinessLogic.Services/Implementation/ReportExportService.cs`

Dependencies:
- QuestPDF (PDF)
- ClosedXML.Excel (Excel)

Key Features:
- Generic `ExportToPdfAsync<T>()` - Works with any data type via reflection
- Generic `ExportToExcelAsync<T>()` - Works with any data type via reflection
- `ApplyDefaultPagination()` - Ensures page=1, pageSize=10000 if not specified
- Automatic column styling
- Configurable orientation
- Row number support
- Date formatting support

```csharp
public ReportExportRequest ApplyDefaultPagination(ReportExportRequest? request)
{
    return new ReportExportRequest
    {
        Page = request?.Page ?? 1,
        PageSize = request?.PageSize ?? 10000, // Default limit
        SortColumn = request?.SortColumn,
        SortDir = request?.SortDir
    };
}
```

#### 1.4 Add Pagination Support for Export
**File**: `Shared/Shared.Contracts/Reporting/ReportExportRequest.cs`

```csharp
namespace Shared.Contracts.Reporting;

public class ReportExportRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10000; // Default max rows per export
    public string? SortColumn { get; set; }
    public string? SortDir { get; set; }
}
```

### Phase 2: Create Import Service

#### 2.1 Create Import Configuration DTOs
**File**: `Shared/Shared.Contracts/Reporting/ImportMetadata.cs`

```csharp
namespace Shared.Contracts.Reporting;

public class ImportColumnMapping
{
    public int ColumnIndex { get; set; } // 0-based Excel column index
    public string PropertyName { get; set; } = string.Empty;
    public bool Required { get; set; } = true;
    public Type? PropertyType { get; set; }
    public string? Format { get; set; } // For date parsing
}

public class ImportResult<T>
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<T> ImportedData { get; set; } = new();
    public List<ImportError> Errors { get; set; } = new();
}

public class ImportError
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
```

#### 2.2 Create Import Service Interface
**File**: `Shared/BusinessLogic.Services/Interface/IReportImportService.cs`

```csharp
using Shared.Contracts.Reporting;

namespace BusinessLogic.Services.Interface;

public interface IReportImportService
{
    Task<ImportResult<T>> ImportFromExcelAsync<T>(
        IFormFile file,
        List<ImportColumnMapping> mappings,
        int headerRow = 0,
        int dataStartRow = 1);
}
```

#### 2.3 Create Import Service Implementation
**File**: `Shared/BusinessLogic.Services/Implementation/ReportImportService.cs`

Key Features:
- Generic Excel import via reflection
- Column mapping configuration
- Type conversion (Guid, string, int, DateTime)
- Required field validation
- Row-level error tracking
- Returns detailed import results

### Phase 3: Refactor Services (Example Pattern)

**Inspired by TrackingSessionService** (better pattern with dynamic title, filter info, page numbers):

#### 3.1 Refactor MstBuildingService (Export)

**Before (lines 304-422):**
```csharp
public async Task<byte[]> ExportPdfAsync()
{
    QuestPDF.Settings.License = LicenseType.Community;
    var buildings = await _repository.GetAllExportAsync(); // No pagination!

    var document = Document.Create(container => {
        // 80+ lines of QuestPDF code...
        // Static title: "Master Building Report"
        // No filter info
        // No total count
        // No page numbers
    });
    return document.GeneratePdf();
}

public async Task<byte[]> ExportExcelAsync()
{
    var buildings = await _repository.GetAllExportAsync(); // No pagination!
    using var workbook = new XLWorkbook();
    // 40+ lines of ClosedXML code...
    // Basic formatting only
    return stream.ToArray();
}
```

**After:**
```csharp
// Inject IReportExportService in constructor
private readonly IReportExportService _reportExportService;

public async Task<byte[]> ExportPdfAsync(ReportExportRequest request)
{
    // Apply default pagination (page=1, pageSize=10000)
    request = _reportExportService.ApplyDefaultPagination(request);

    var buildings = await _repository.GetPaginatedExportAsync(
        request.Page,
        request.PageSize);

    var readDtos = _mapper.Map<IEnumerable<MstBuildingRead>>(buildings);

    var metadata = new ReportMetadata
    {
        Title = "Master Building Report",
        FilterInfo = _reportExportService.GenerateFilterInfo(new ReportFilterInfo
        {
            From = request.DateFrom,
            To = request.DateTo,
            Search = request.Search,
            Status = request.Status
        }),
        TotalRecords = readDtos.Count(),
        Columns = new List<ReportColumn>
        {
            new() { Header = "Name", PropertyName = "Name", Width = 2 },
            new() { Header = "Image", PropertyName = "Image", Width = 2 },
            new() { Header = "CreatedBy", PropertyName = "CreatedBy", Width = 2 },
            new() { Header = "CreatedAt", PropertyName = "CreatedAt", Format = "yyyy-MM-dd HH:mm:ss", Width = 2 },
            new() { Header = "UpdatedBy", PropertyName = "UpdatedBy", Width = 2 },
            new() { Header = "UpdatedAt", PropertyName = "UpdatedAt", Format = "yyyy-MM-dd HH:mm:ss", Width = 2 },
            new() { Header = "Status", PropertyName = "Status", Width = 1 }
        },
        Orientation = ReportOrientation.Landscape,
        IncludePageNumbers = true
    };

    return await _reportExportService.ExportToPdfAsync(readDtos, metadata);
}

public async Task<byte[]> ExportExcelAsync(ReportExportRequest request)
{
    request = _reportExportService.ApplyDefaultPagination(request);

    var buildings = await _repository.GetPaginatedExportAsync(
        request.Page,
        request.PageSize);

    var readDtos = _mapper.Map<IEnumerable<MstBuildingRead>>(buildings);

    var metadata = new ReportMetadata
    {
        Title = "Master Building Report",
        Columns = GetBuildingReportColumns() // Reuse column definitions
    };

    return await _reportExportService.ExportToExcelAsync(readDtos, metadata);
}

private List<ReportColumn> GetBuildingReportColumns()
{
    return new()
    {
        new() { Header = "Name", PropertyName = "Name" },
        new() { Header = "Image", PropertyName = "Image" },
        new() { Header = "CreatedBy", PropertyName = "CreatedBy" },
        new() { Header = "CreatedAt", PropertyName = "CreatedAt", Format = "yyyy-MM-dd HH:mm:ss" },
        new() { Header = "UpdatedBy", PropertyName = "UpdatedBy" },
        new() { Header = "UpdatedAt", PropertyName = "UpdatedAt", Format = "yyyy-MM-dd HH:mm:ss" },
        new() { Header = "Status", PropertyName = "Status" }
    };
}
```

#### 3.2 Refactor MstBuildingService (Import)

**Before (lines 232-271):**
```csharp
public async Task<IEnumerable<MstBuildingDto>> ImportAsync(IFormFile file)
{
    var buildings = new List<MstBuilding>();
    var username = UsernameFormToken;

    using var stream = file.OpenReadStream();
    using var workbook = new XLWorkbook(stream);
    // 40+ lines of manual Excel reading...
}
```

**After:**
```csharp
public async Task<ImportResult<MstBuildingDto>> ImportAsync(IFormFile file)
{
    var mappings = new List<ImportColumnMapping>
    {
        new() { ColumnIndex = 0, PropertyName = "Name", Required = true, PropertyType = typeof(string) },
        new() { ColumnIndex = 1, PropertyName = "Image", Required = false, PropertyType = typeof(string) }
    };

    var importResult = await _reportImportService.ImportFromExcelAsync<MstBuildingCreateDto>(
        file, mappings);

    if (importResult.Errors.Any())
    {
        return new ImportResult<MstBuildingDto>
        {
            Errors = importResult.Errors,
            FailureCount = importResult.FailureCount
        };
    }

    var result = new List<MstBuildingDto>();
    foreach (var dto in importResult.ImportedData)
    {
        var created = await CreateAsync(dto);
        result.Add(created);
    }

    return new ImportResult<MstBuildingDto>
    {
        ImportedData = result,
        SuccessCount = result.Count,
        FailureCount = importResult.FailureCount,
        Errors = importResult.Errors
    };
}
```

#### 3.3 Add Paginated Export to Repository
**File**: `Shared/Repositories/Repository/MstBuildingRepository.cs`

```csharp
public async Task<IEnumerable<MstBuilding>> GetPaginatedExportAsync(int page, int pageSize)
{
    var query = GetAllQueryable();
    query = query.OrderBy(x => x.Name);
    query = query.Skip((page - 1) * pageSize).Take(pageSize);
    return await query.ToListAsync();
}
```

### Phase 4: Update Controllers

**File**: `Shared/Web.API.Controllers/Controllers/MstBuildingController.cs`

Add pagination parameters to export endpoints:

```csharp
[HttpGet("export/pdf")]
public async Task<IActionResult> ExportPdf(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10000)
{
    var request = new ReportExportRequest { Page = page, PageSize = pageSize };
    var pdfBytes = await _service.ExportPdfAsync(request);
    return File(pdfBytes, "application/pdf", "MstBuilding_Report.pdf");
}

[HttpGet("export/excel")]
public async Task<IActionResult> ExportExcel(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10000)
{
    var request = new ReportExportRequest { Page = page, PageSize = pageSize };
    var excelBytes = await _service.ExportExcelAsync(request);
    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "MstBuilding_Report.xlsx");
}
```

### Phase 5: Update Service Interfaces

**File**: `Shared/BusinessLogic.Services/Interface/IMstBuildingService.cs`

```csharp
Task<byte[]> ExportPdfAsync(ReportExportRequest request);
Task<byte[]> ExportExcelAsync(ReportExportRequest request);
Task<ImportResult<MstBuildingDto>> ImportAsync(IFormFile file);
```

**Note**: ReportExportRequest is required (non-nullable) to enforce mandatory pagination.

## Files to Create

| File | Purpose |
|------|---------|
| `Shared/Shared.Contracts/Reporting/ReportMetadata.cs` | Export configuration DTOs |
| `Shared/Shared.Contracts/Reporting/ReportExportRequest.cs` | Export pagination request |
| `Shared/Shared.Contracts/Reporting/ReportFilterInfo.cs` | Filter info for export header |
| `Shared/Shared.Contracts/Reporting/ImportMetadata.cs` | Import configuration DTOs |
| `Shared/BusinessLogic.Services/Interface/IReportExportService.cs` | Export service interface |
| `Shared/BusinessLogic.Services/Interface/IReportImportService.cs` | Import service interface |
| `Shared/BusinessLogic.Services/Implementation/ReportExportService.cs` | Export service implementation |
| `Shared/BusinessLogic.Services/Implementation/ReportImportService.cs` | Import service implementation |

## Implementation Order

### Phase 1: Foundation (Priority 1)
1. Create `Shared/Shared.Contracts/Reporting/ReportMetadata.cs` - Export configuration DTOs
2. Create `Shared/Shared.Contracts/Reporting/ReportExportRequest.cs` - Pagination request DTO
3. Create `Shared/Shared.Contracts/Reporting/ImportMetadata.cs` - Import configuration DTOs
4. Create `Shared/BusinessLogic.Services/Interface/IReportExportService.cs` - Export interface
5. Create `Shared/BusinessLogic.Services/Interface/IReportImportService.cs` - Import interface

### Phase 2: Service Implementation (Priority 2)
1. Create `ReportExportService.cs` with `ExportToPdfAsync()` and `ExportToExcelAsync()`
2. Create `ReportImportService.cs` with `ImportFromExcelAsync()`
3. Register both services in `RootServiceExtensions.cs`

### Phase 3: Repository Updates (Priority 3)
1. Add `GetPaginatedExportAsync(int page, int pageSize)` to `MstBuildingRepository`
2. Add `GetPaginatedExportAsync(int page, int pageSize)` to `MstFloorRepository`
3. Add `GetPaginatedExportAsync(int page, int pageSize)` to `MstBleReaderRepository`

### Phase 4: Service Refactoring (Priority 4) - 3 Services Only
1. Refactor `MstBuildingService.ExportPdfAsync()` and `ExportExcelAsync()`
2. Refactor `MstBuildingService.ImportAsync()` - return `ImportResult<MstBuildingDto>`
3. Refactor `MstFloorService.ExportPdfAsync()` and `ExportExcelAsync()`
4. Refactor `MstFloorService.ImportAsync()` - return `ImportResult<MstFloorRead>`
5. Refactor `MstBleReaderService.ExportPdfAsync()` and `ExportExcelAsync()`
6. Refactor `MstBleReaderService.ImportAsync()` - return `ImportResult<MstBleReaderRead>`

### Phase 5: Controller & Interface Updates (Priority 5)
1. Update `IMstBuildingService` interface - non-nullable `ReportExportRequest` parameter
2. Update `IMstFloorService` interface
3. Update `IMstBleReaderService` interface
4. Update `MstBuildingController` export endpoints - add page/pageSize query params
5. Update `MstFloorController` export endpoints
6. Update `MstBleReaderController` export endpoints

### Future Phases (After Proof of Concept Validation)
- Apply same pattern to remaining 20+ services

## Key Benefits

1. **Reduced Code Duplication**: ~80 lines per service → ~10 lines per service
2. **Pagination Support**: Prevent memory issues with large datasets
3. **Consistent Error Handling**: Standardized import error messages
4. **Maintainability**: Column style changes in ONE place
5. **Type Safety**: Generic implementation works with any DTO
6. **Testability**: Export/Import logic can be unit tested independently

## Verification

1. Test export with pagination (page=1, pageSize=100)
2. Test export without pagination (pageSize=0 or null)
3. Test import with valid data
4. Test import with invalid data (verify error messages)
5. Test export PDF/Excel file formats
6. Performance test with 10,000+ rows

## Critical Notes

1. **Breaking Change (Import)**: Return type changes from `IEnumerable<T>` to `ImportResult<T>`
2. **Breaking Change (Export)**: `ReportExportRequest` is now required (non-nullable) - mandatory pagination
3. **Default Pagination**: All exports default to 10,000 rows max per page
4. **API consumers must update**: Clients must handle `ImportResult<T>` instead of `IEnumerable<T>`
5. **Reflection Performance**: Export uses reflection - acceptable for export operations (not hot path)
