using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IMonitoringConfigService
    {
        Task<MonitoringConfigRead> GetByIdAsync(Guid id);
        Task<IEnumerable<MonitoringConfigRead>> GetAllAsync();
        Task<MonitoringConfigDto> CreateAsync(MonitoringConfigCreateDto createDto);
        Task UpdateAsync(Guid id, MonitoringConfigUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MonitoringConfigFilter filter);
    }
}