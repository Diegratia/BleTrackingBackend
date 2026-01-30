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
    public interface IPatrolCaseService
    {
        Task<object> FilterAsync(
            DataTablesProjectedRequest request,
            PatrolCaseFilter filter
        );
        Task<PatrolCaseDto> CreateAsync(PatrolCaseCreateDto createDto);
        Task<PatrolCaseDto> UpdateAsync(Guid id, PatrolCaseUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<PatrolCaseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<PatrolCaseRead>> GetAllAsync();
    }
}