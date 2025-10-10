using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IAlarmCategorySettingsService
    {
        Task<AlarmCategorySettingsDto> CreateAsync(AlarmCategorySettingsCreateDto createDto);
        Task<IEnumerable<AlarmCategorySettingsDto>> GetAllAsync();
        Task<AlarmCategorySettingsDto> GetByIdAsync(Guid id);
        Task UpdateAsync(Guid id, AlarmCategorySettingsUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
    }
}