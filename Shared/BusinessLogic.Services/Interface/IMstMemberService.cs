using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IMstMemberService
    {
        Task<IEnumerable<MstMemberRead>> GetAllMembersAsync();
        Task<IEnumerable<OpenMstMemberDto>> OpenGetAllMembersAsync();
        Task<IEnumerable<MstMemberLookUpRead>> GetAllLookUpAsync();
        Task<MstMemberRead> MemberBlacklistAsync(Guid id, BlacklistReasonDto dto);
        Task UnBlacklistMemberAsync(Guid id);
        Task<MstMemberRead> GetMemberByIdAsync(Guid id);
        Task<MstMemberRead> CreateMemberAsync(MstMemberCreateDto createDto);
        Task<MstMemberRead> UpdateMemberAsync(Guid id, MstMemberUpdateDto updateDto);
        Task DeleteMemberAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request);
        Task<IEnumerable<MstMemberDto>> ImportCsvAsync(IFormFile file);
        Task<IEnumerable<MstMemberDto>> ImportExcelAsync(IFormFile file);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}