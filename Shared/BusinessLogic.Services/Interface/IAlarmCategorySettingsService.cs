using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interface
{
    public interface IAlarmCategorySettingsService
    {
        Task<IEnumerable<AlarmCategorySettingsRead>> GetAllAsync();
        Task<AlarmCategorySettingsRead?> GetByIdAsync(Guid id);
        Task UpdateAsync(Guid id, AlarmCategorySettingsUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, AlarmCategorySettingsFilter filter);
    }
}
