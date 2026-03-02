using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IPatrolCaseService
    {
        Task<object> FilterAsync(
            DataTablesProjectedRequest request,
            PatrolCaseFilter filter
        );
        Task<PatrolCaseRead> CreateAsync(PatrolCaseCreateDto createDto);
        Task<PatrolCaseRead> UpdateAsync(Guid id, PatrolCaseUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<PatrolCaseRead?> GetByIdAsync(Guid id);
        Task<IEnumerable<PatrolCaseRead>> GetAllAsync();

        // Approval workflow methods (Case is auto-submitted on create)
        Task<PatrolCaseRead> ApproveAsync(Guid id, PatrolCaseApprovalDto dto);
        Task<PatrolCaseRead> RejectAsync(Guid id, PatrolCaseApprovalDto dto);
        Task<PatrolCaseRead> CloseAsync(Guid id, PatrolCaseCloseDto dto);

        // Attachment management
        Task DeleteAttachmentAsync(Guid caseId, Guid attachmentId);
    }
}