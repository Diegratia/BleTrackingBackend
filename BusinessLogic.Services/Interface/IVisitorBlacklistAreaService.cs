using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IVisitorBlacklistAreaService
    {
        Task<VisitorBlacklistAreaDto> CreateVisitorBlacklistAreaAsync(VisitorBlacklistAreaCreateDto createDto);
        Task<VisitorBlacklistAreaDto> GetVisitorBlacklistAreaByIdAsync(Guid id);
        Task<IEnumerable<VisitorBlacklistAreaDto>> GetAllVisitorBlacklistAreasAsync();
        Task UpdateVisitorBlacklistAreaAsync(Guid id, VisitorBlacklistAreaUpdateDto updatedto);
        Task DeleteVisitorBlacklistAreaAsync(Guid id);
    }
}