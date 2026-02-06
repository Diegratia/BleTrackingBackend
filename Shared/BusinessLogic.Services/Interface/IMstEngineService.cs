using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IMstEngineService
    {
        Task<List<MstEngineRead>> GetAllEnginesAsync();
        Task<List<MstEngineRead>> GetAllOnlineAsync();
        Task<MstEngineRead?> GetEngineByIdAsync(Guid id);
        Task<MstEngine?> GetEngineIdAsync(string engineTrackingId);
        Task<MstEngineRead> CreateEngineAsync(MstEngineCreateDto dto);
        Task UpdateEngineAsync(Guid id, MstEngineUpdateDto dto);
        Task UpdateEngineByIdAsync(string engineTrackingId, MstEngineUpdateDto dto);
        Task DeleteEngineAsync(Guid id);
        Task StopEngineAsync(string engineTrackingId);
        Task StartEngineAsync(string engineTrackingId);
        Task<(List<MstEngineRead> Data, int Total, int Filtered)> FilterAsync(DataTablesProjectedRequest request, MstEngineFilter filter);
        // Task<byte[]> ExportPdfAsync();
        // Task<byte[]> ExportExcelAsync();
    }
}
