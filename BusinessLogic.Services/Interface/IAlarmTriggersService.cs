
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IAlarmTriggersService
    {
        Task<IEnumerable<AlarmTriggersDto>> GetAllAsync();
        Task<IEnumerable<AlarmTriggersOpenDto>> OpenGetAllAsync();
        Task UpdateAsync(Guid id, AlarmTriggersUpdateDto dto);
        Task<object> FilterAsync(DataTablesRequest request);
         Task UpdateAlarmStatusAsync(string beaconId, AlarmTriggersUpdateDto dto);
    }
}