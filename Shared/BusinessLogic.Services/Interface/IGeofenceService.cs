using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IGeofenceService
    {
        Task<GeofenceDto> GetByIdAsync(Guid id);
        Task<IEnumerable<GeofenceDto>> GetAllAsync();
        Task<GeofenceDto> CreateAsync(GeofenceCreateDto createDto);
        Task UpdateAsync(Guid id, GeofenceUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 

    }
}