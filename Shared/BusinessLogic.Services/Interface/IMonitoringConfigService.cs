using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMonitoringConfigService
    {
        Task<MonitoringConfigDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MonitoringConfigDto>> GetAllAsync();
        Task<MonitoringConfigDto> CreateAsync(MonitoringConfigCreateDto createDto);
        Task UpdateAsync(Guid id, MonitoringConfigUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 
    }
}