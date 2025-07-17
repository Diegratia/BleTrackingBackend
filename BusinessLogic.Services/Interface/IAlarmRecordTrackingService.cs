using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IAlarmRecordTrackingService
    {
        Task<AlarmRecordTrackingDto> GetByIdAsync(Guid id);
        Task<IEnumerable<AlarmRecordTrackingDto>> GetAllAsync();
        Task<AlarmRecordTrackingDto> CreateAsync(AlarmRecordTrackingCreateDto createDto);
        Task UpdateAsync(Guid id, AlarmRecordTrackingUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync(); 
    }
}