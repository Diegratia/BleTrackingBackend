using Data.ViewModels;
using Data.ViewModels.AlarmAnalytics;
using Repositories.Repository.RepoModel;
using Shared.Contracts.Read;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interface
{
    public interface IAlarmTriggersService
    {
        Task<IEnumerable<AlarmTriggersRead>> GetAllAsync();
        Task<IEnumerable<AlarmTriggersOpenDto>> OpenGetAllAsync();
        Task UpdateAsync(Guid id, AlarmTriggersUpdateDto dto);
        Task<object> FilterAsync(DataTablesProjectedRequest request);
        Task<IEnumerable<AlarmTriggersLookUp>> GetAllLookUpAsync();
        Task<object> GetIncidentTimelineAsync(Guid alarmTriggerId);

        // New workflow methods based on ActionStatus enum changes

        /// <summary>
        /// Operator acknowledges the alarm - changes ActionStatus to Acknowledged
        /// Flow: Idle → Acknowledged
        /// </summary>
        Task AcknowledgeAsync(Guid id);

        /// <summary>
        /// Operator dispatches alarm to specific security - changes ActionStatus to Dispatched
        /// Flow: Acknowledged → Dispatched
        /// </summary>
        Task DispatchAsync(Guid id, Guid securityId);

        /// <summary>
        /// Operator puts alarm in waiting queue (no security available) - changes ActionStatus to Waiting
        /// Flow: Acknowledged → Waiting
        /// </summary>
        Task WaitingAsync(Guid id);

        /// <summary>
        /// Security accepts the dispatch - changes ActionStatus to Accepted
        /// Flow: Dispatched → Accepted
        /// </summary>
        Task AcceptAsync(Guid id);

        /// <summary>
        /// Security arrives at location - changes ActionStatus to Arrived
        /// Flow: Accepted → Arrived
        /// </summary>
        Task ArrivedAsync(Guid id);

        /// <summary>
        /// Security completes investigation with result - changes ActionStatus to DoneInvestigated
        /// Flow: Arrived → DoneInvestigated
        /// </summary>
        Task DoneInvestigatedAsync(Guid id, string result);

        /// <summary>
        /// Operator marks alarm as resolved - changes ActionStatus to Done
        /// Flow: DoneInvestigated → Done (final state)
        /// </summary>
        Task ResolveAsync(Guid id);
    }
}
