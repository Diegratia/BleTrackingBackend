using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface ICardGroupService
    {
        Task<CardGroupRead> GetByIdAsync(Guid id);
        Task<IEnumerable<CardGroupRead>> GetAllAsync();
        Task<CardGroupRead> CreateAsync(CardGroupCreateDto dto);
        Task UpdateAsync(Guid id, CardGroupUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, CardGroupFilter filter);
    }
}
