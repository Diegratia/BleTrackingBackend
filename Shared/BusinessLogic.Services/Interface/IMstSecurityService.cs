using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IMstSecurityService
    {
        // Use Read DTO for query operations (direct return from repository)
        Task<IEnumerable<MstSecurityRead>> GetAllSecuritiesAsync();
        Task<IEnumerable<MstSecurityRead>> GetAllSecurityHeadsAsync();
        Task<IEnumerable<MstSecurityLookUpRead>> GetAllLookUpAsync(bool? headsOnly = null);
        Task<MstSecurityRead> GetSecurityByIdAsync(Guid id);

        // Open API endpoints still use DTOs for external integration
        Task<IEnumerable<OpenMstSecurityDto>> OpenGetAllSecuritiesAsync();

        // Create/Update/Import return Read DTOs
        Task<MstSecurityRead> CreateSecurityAsync(MstSecurityCreateDto createDto);
        Task<MstSecurityRead> UpdateSecurityAsync(Guid id, MstSecurityUpdateDto updateDto);
        Task DeleteSecurityAsync(Guid id);

        // Filter with typed filter
        Task<object> FilterAsync(DataTablesProjectedRequest request, SecurityFilter filter);

        // Import/Export still return DTOs for file operations
        Task<IEnumerable<MstSecurityDto>> ImportAsync(IFormFile file);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}
