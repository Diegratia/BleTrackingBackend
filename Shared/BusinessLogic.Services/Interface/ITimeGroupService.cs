using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface ITimeGroupService
    {
        Task<IEnumerable<TimeGroupRead>> GetAllsAsync();
        Task<TimeGroupRead?> GetByIdAsync(Guid id);
        Task<TimeGroupRead> CreateAsync(TimeGroupCreateDto dto);
        Task UpdateAsync(Guid id, TimeGroupUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
    }
}