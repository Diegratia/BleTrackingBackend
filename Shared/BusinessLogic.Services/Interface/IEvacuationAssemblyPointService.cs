using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IEvacuationAssemblyPointService
    {
        Task<EvacuationAssemblyPointRead> GetByIdAsync(Guid id);
        Task<IEnumerable<EvacuationAssemblyPointRead>> GetAllAsync();
        Task<EvacuationAssemblyPointRead> CreateAsync(EvacuationAssemblyPointCreateDto createDto);
        Task UpdateAsync(Guid id, EvacuationAssemblyPointUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, EvacuationAssemblyPointFilter filter);
        Task<List<EvacuationAssemblyPointRead>> GetByFloorplanIdAsync(Guid floorplanId);
    }
}
