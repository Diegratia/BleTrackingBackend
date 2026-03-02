using Data.ViewModels;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Interface
{
    public interface IAlarmRecordTrackingService
    {
        Task<AlarmRecordTrackingDto> GetByIdAsync(Guid id);
        Task<IEnumerable<AlarmRecordTrackingDto>> GetAllAsync();
        Task<ResponseCollection<AlarmRecordLog>> GetAlarmLogsAsync(TrackingAnalyticsRequestRM request);
        // Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task<List<AlarmTriggerLogFlatRM>> GetAlarmTriggerLogsAsync(
                AlarmAnalyticsRequestRM request);
    }
}