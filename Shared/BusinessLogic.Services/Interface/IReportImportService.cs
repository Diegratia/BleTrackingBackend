using Microsoft.AspNetCore.Http;
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
