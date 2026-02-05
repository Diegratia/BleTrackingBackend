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
    public interface IStayOnAreaService
    {
        Task<StayOnAreaRead> GetByIdAsync(Guid id);
        Task<IEnumerable<StayOnAreaRead>> GetAllAsync();
        Task<StayOnAreaRead> CreateAsync(StayOnAreaCreateDto createDto);
        Task UpdateAsync(Guid id, StayOnAreaUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, StayOnAreaFilter filter);
    }
}