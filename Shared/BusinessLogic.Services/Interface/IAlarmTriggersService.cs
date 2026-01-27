
using Data.ViewModels;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Interface
{
    public interface IAlarmTriggersService
    {
        Task<IEnumerable<AlarmTriggersDto>> GetAllAsync();
        Task<IEnumerable<AlarmTriggersOpenDto>> OpenGetAllAsync();
        Task UpdateAsync(Guid id, AlarmTriggersUpdateDto dto);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<IEnumerable<AlarmTriggersLookUp>> GetAllLookUpAsync();
         Task UpdateAlarmStatusAsync(string beaconId, AlarmTriggersUpdateDto dto);
    }
}