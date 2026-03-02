using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IPatrolAttachmentService
    {
        Task<PatrolAttachmentDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<PatrolAttachmentDto>> GetAllAsync();
        Task<PatrolAttachmentDto> CreateAsync(PatrolAttachmentCreateDto createDto);
        Task<PatrolAttachmentDto> UpdateAsync(Guid id, PatrolAreaUpdateDto updateDto);
        Task<object> FilterAsync(DataTablesRequest request); 
    }
}