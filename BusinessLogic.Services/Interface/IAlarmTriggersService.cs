using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IAlarmTriggersService
    {
        Task<IEnumerable<AlarmTriggersDto>> GetAllAsync();
        Task UpdateAsync(Guid id, AlarmTriggersUpdateDto dto);
        Task<object> FilterAsync(DataTablesRequest request);
         Task UpdateAlarmStatusAsync(string beaconId, AlarmTriggersUpdateDto dto);
    }
}