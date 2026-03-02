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
        Task<IEnumerable<MstSecurityDto>> GetAllSecuritiesAsync();
        Task<IEnumerable<OpenMstSecurityDto>> OpenGetAllSecuritiesAsync();
        Task<IEnumerable<MstSecurityLookUpRead>> GetAllLookUpAsync();
        // Task<MstSecurityDto> SecurityBlacklistAsync(Guid id, BlacklistReasonDto dto);
        //  Task UnBlacklistSecurityAsync(Guid id) ;
        Task<MstSecurityDto> GetSecurityByIdAsync(Guid id);
        Task<MstSecurityDto> CreateSecurityAsync(MstSecurityCreateDto createDto);
        Task<MstSecurityDto> UpdateSecurityAsync(Guid id, MstSecurityUpdateDto updateDto);
        Task DeleteSecurityAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<object> FilterAsync(DataTablesProjectedRequest request, SecurityFilter filter);
        Task<IEnumerable<MstSecurityDto>> ImportAsync(IFormFile file);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}