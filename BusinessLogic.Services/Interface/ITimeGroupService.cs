using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface ITimeGroupService
    {
        Task<IEnumerable<TimeGroupDto>> GetAllsAsync();
        Task<TimeGroupDto?> GetByIdAsync(Guid id);
        Task<TimeGroupDto> CreateAsync(TimeGroupCreateDto dto);
        Task<TimeGroupDto> UpdateAsync(Guid id, TimeGroupUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
    }

}