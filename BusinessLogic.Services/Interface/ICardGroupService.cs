using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface ICardGroupService
    {
        // Task<CardGroupDto> CreateAsync(CardGroupCreateDto dto);
        // Task UpdateAsync(Guid id, CardGroupUpdateDto dto);
        Task<IEnumerable<CardGroupDto>> GetAllAsync();
        Task<object> FilterAsync(DataTablesRequest request);
        Task DeleteAsync(Guid id);
        Task<CardGroupDto> GetByIdAsync(Guid id);
    }
}