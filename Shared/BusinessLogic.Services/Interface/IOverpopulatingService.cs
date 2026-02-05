using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IOverpopulatingService
    {
        Task<OverpopulatingRead> GetByIdAsync(Guid id);
        Task<IEnumerable<OverpopulatingRead>> GetAllAsync();
        Task<OverpopulatingRead> CreateAsync(OverpopulatingCreateDto createDto);
        Task UpdateAsync(Guid id, OverpopulatingUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, OverpopulatingFilter filter);
    }
}
