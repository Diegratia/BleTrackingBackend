using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IBoundaryService
    {
        Task<BoundaryRead> GetByIdAsync(Guid id);
        Task<IEnumerable<BoundaryRead>> GetAllAsync();
        Task<BoundaryRead> CreateAsync(BoundaryCreateDto createDto);
        Task UpdateAsync(Guid id, BoundaryUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, BoundaryFilter filter);
    }
}