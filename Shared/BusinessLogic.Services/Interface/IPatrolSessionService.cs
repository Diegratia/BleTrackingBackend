using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IPatrolSessionService
    {
        Task<object> FilterAsync(
            DataTablesProjectedRequest request,
            PatrolSessionFilter filter
        );
        Task<IEnumerable<PatrolSessionLookUpRead>> GetAllLookUpAsync();
        Task<PatrolSessionRead> CreateAsync(PatrolSessionStartDto dto);
        Task<PatrolSessionRead?> GetByIdAsync(Guid id);
        Task<IEnumerable<PatrolSessionRead>> GetAllAsync();
        Task<PatrolSessionRead> StopAsync(Guid sessionId);
        Task<object> SubmitCheckpointActionAsync(PatrolCheckpointActionDto dto);
    }
}