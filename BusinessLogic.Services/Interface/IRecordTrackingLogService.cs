using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IRecordTrackingLogService
    {
        Task<IEnumerable<RecordTrackingLogDto>> GetRecordTrackingLogsAsync();
        Task<RecordTrackingLogDto> GetRecordTrackingLogByIdAsync(Guid id);
        Task DeleteRecordTrackingLogAsync(Guid id);
    }
}