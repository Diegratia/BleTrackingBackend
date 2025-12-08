using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Interface
{
    public interface IMstMemberService
    {
        Task<IEnumerable<MstMemberDto>> GetAllMembersAsync();
        Task<IEnumerable<OpenMstMemberDto>> OpenGetAllMembersAsync();
        Task<IEnumerable<MstMemberLookUpDto>> GetAllLookUpAsync();
        Task<MstMemberDto> MemberBlacklistAsync(Guid id, BlacklistReasonDto dto);
         Task UnBlacklistMemberAsync(Guid id) ;
        Task<MstMemberDto> GetMemberByIdAsync(Guid id);
        Task<MstMemberDto> CreateMemberAsync(MstMemberCreateDto createDto);
        Task<MstMemberDto> UpdateMemberAsync(Guid id, MstMemberUpdateDto updateDto);
        Task DeleteMemberAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<IEnumerable<MstMemberDto>> ImportAsync(IFormFile file);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}