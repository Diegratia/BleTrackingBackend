using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstMemberService
    {
        Task<IEnumerable<MstMemberDto>> GetAllMembersAsync();
        Task<IEnumerable<OpenMstMemberDto>> OpenGetAllMembersAsync();
        Task<MstMemberDto> GetMemberByIdAsync(Guid id);
        Task<MstMemberDto> CreateMemberAsync(MstMemberCreateDto createDto);
        Task<MstMemberDto> UpdateMemberAsync(Guid id, MstMemberUpdateDto updateDto);
        Task DeleteMemberAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}