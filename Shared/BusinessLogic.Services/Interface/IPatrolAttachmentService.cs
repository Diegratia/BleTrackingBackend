using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IPatrolAttachmentService
    {
        // Use Read DTO for query operations (direct return from repository)
        Task<PatrolAttachmentRead> GetByIdAsync(Guid id);
        Task<IEnumerable<PatrolAttachmentRead>> GetAllAsync();

        // Create/Update return Read DTOs
        Task<PatrolAttachmentRead> CreateAsync(PatrolAttachmentCreateDto createDto);
        Task<PatrolAttachmentRead> UpdateAsync(Guid id, PatrolAttachmentUpdateDto updateDto);
        Task DeleteAsync(Guid id);

        // Filter with typed filter
        Task<object> FilterAsync(DataTablesProjectedRequest request, PatrolAttachmentFilter filter);
    }
}
