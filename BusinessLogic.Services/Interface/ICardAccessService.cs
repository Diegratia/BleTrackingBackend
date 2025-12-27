using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface ICardAccessService
    {
        Task<CardAccessDto> CreateAsync(CardAccessCreateDto dto);
        Task UpdateAsync(Guid id, CardAccessUpdateDto dto);
        Task<IEnumerable<CardAccessDto>> GetAllAsync();
        Task<CardAccessDto?> GetByIdAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task DeleteAsync(Guid id);
        Task AssignCardAccessToCardAsync(CardAssignAccessDto dto);
    }
}