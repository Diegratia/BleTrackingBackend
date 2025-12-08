using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IVisitorService
    {
        // Task<VisitorDto> CreateVisitorAsync(VisitorCreateDto createDto);
        // Task<VisitorDto> CreateVisitorAsync(OpenVisitorCreateDto createDto);
        Task<VisitorDto> CreateVisitorAsync(VMSOpenVisitorCreateDto createDto);
        Task<OpenVisitorDto> CreateVisitorVMSAsync(VMSOpenVisitorCreateDto createDto);
        // Task<VisitorDto> CreateVisitorWithTrxAsync(VisitorWithTrxCreateDto createDto);
        Task SendInvitationVisitorAsync(Guid id, CreateInvitationDto CreateInvitationDto);
        Task<VisitorDto> GetVisitorByIdAsync(Guid id);
        Task<VisitorDto> GetVisitorByIdPublicAsync(Guid id);
        Task<IEnumerable<VisitorDto>> GetAllVisitorsAsync();
        Task<IEnumerable<OpenVisitorDto>> OpenGetAllVisitorsAsync();
        Task<VisitorDto> UpdateVisitorAsync(Guid id, VisitorUpdateDto updateDto);
        Task DeleteVisitorAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task ConfirmVisitorEmailAsync(ConfirmEmailDto confirmDto);
        Task<VisitorDto> FillInvitationFormAsync(VisitorInvitationDto dto);
        Task<VisitorDto> AcceptInvitationFormAsync(MemberInvitationDto dto);
        Task SendInvitationByEmailAsync(SendEmailInvitationDto dto);
        Task SendBatchInvitationByEmailAsync(List<SendEmailInvitationDto> dto);
        Task DeclineInvitationAsync(Guid id);
        Task AcceptInvitationAsync(Guid id);
        // Task<VisitorDto> CreateVisitorVMSAsync(VMSOpenVisitorCreateDto createDto);
        Task<VisitorDto> BlacklistVisitorAsync(Guid id, BlacklistReasonDto dto);
        Task UnBlacklistVisitorAsync(Guid id);
        Task<IEnumerable<VisitorLookUpDto>> GetAllLookUpAsync();
    }
}