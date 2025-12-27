using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IBlacklistAreaService
    {
        Task<BlacklistAreaDto> CreateBlacklistAreaAsync(BlacklistAreaCreateDto createDto);
        Task<IEnumerable<BlacklistAreaDto>> CreatesBlacklistAreaAsync(BlacklistAreaRequestDto request) ;
        Task<List<BlacklistAreaDto>> CreateBatchBlacklistAreaAsync(List<BlacklistAreaCreateDto> dtos);
        Task<BlacklistAreaDto> GetBlacklistAreaByIdAsync(Guid id);
        Task<IEnumerable<OpenBlacklistAreaDto>> OpenGetAllBlacklistAreasAsync();
        Task<IEnumerable<BlacklistAreaDto>> GetAllBlacklistAreasAsync();
        Task UpdateBlacklistAreaAsync(Guid id, BlacklistAreaUpdateDto updatedto);
        Task DeleteBlacklistAreaAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 
        // Task<object> MinimalFilterAsync(DataTablesRequest request); 
    }
}