using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IAlarmRecordTrackingService
    {
        Task<AlarmRecordTrackingDto> GetByIdAsync(Guid id);
        Task<IEnumerable<AlarmRecordTrackingDto>> GetAllAsync();
        // Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync(); 
    }
}