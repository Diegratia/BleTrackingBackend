using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IStayOnAreaService
    {
        Task<StayOnAreaDto> GetByIdAsync(Guid id);
        Task<IEnumerable<StayOnAreaDto>> GetAllAsync();
        Task<StayOnAreaDto> CreateAsync(StayOnAreaCreateDto createDto);
        Task UpdateAsync(Guid id, StayOnAreaUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 

    }
}