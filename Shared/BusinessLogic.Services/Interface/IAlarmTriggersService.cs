using Data.ViewModels;
using Data.ViewModels.AlarmAnalytics;
using Repositories.Repository.RepoModel;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IAlarmTriggersService
    {
        Task<IEnumerable<AlarmTriggersRead>> GetAllAsync();
        Task<IEnumerable<AlarmTriggersOpenDto>> OpenGetAllAsync();
        Task UpdateAsync(Guid id, AlarmTriggersUpdateDto dto);
        Task<object> FilterAsync(DataTablesProjectedRequest request);
        Task<IEnumerable<AlarmTriggersLookUp>> GetAllLookUpAsync();
        Task UpdateAlarmStatusAsync(string beaconId, AlarmTriggersUpdateDto dto);
        Task<object> GetIncidentTimelineAsync(Guid alarmTriggerId);

        // Security action methods
        Task AcknowledgeAsync(Guid id, string username);
        Task EnRouteAsync(Guid id, string username);
        Task ArrivedAsync(Guid id, string username);
    }
}
