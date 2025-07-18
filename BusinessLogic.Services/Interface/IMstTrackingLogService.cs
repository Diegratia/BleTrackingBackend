using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstTrackingLogService
    {
        Task<IEnumerable<MstTrackingLogDto>> GetMstTrackingLogsAsync();
        Task<MstTrackingLogDto> GetMstTrackingLogByIdAsync(Guid id);
        Task DeleteMstTrackingLogAsync(Guid id);
    }
}