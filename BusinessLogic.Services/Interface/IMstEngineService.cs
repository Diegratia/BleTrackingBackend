using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstEngineService
    {
        Task<IEnumerable<MstEngineDto>> GetAllEnginesAsync();
        Task<IEnumerable<MstEngineDto>> GetAllOnlineAsync();
        Task StopEngineAsync(string engineId);
        Task StartEngineAsync(string engineId);
        Task<MstEngineDto> GetEngineIdAsync(string engineId);
        Task<MstEngineDto> GetEngineByIdAsync(Guid id);
        Task<MstEngineDto> CreateEngineAsync(MstEngineCreateDto dto);
        Task UpdateEngineAsync(Guid id, MstEngineUpdateDto dto);
        Task UpdateEngineByIdAsync(string engineId, MstEngineUpdateDto dto);
        Task DeleteEngineAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}