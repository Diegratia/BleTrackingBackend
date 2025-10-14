using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Interface
{
    public interface ICardService
    {
        Task<CardMinimalsDto> GetByIdAsyncV2(Guid id);
        Task<CardDto> GetByIdAsync(Guid id);
        Task AssignToMemberAsync(Guid id, CardAssignDto dto);
        Task<IEnumerable<CardDto>> GetAllAsync();
        Task<IEnumerable<CardMinimalsDto>> GetAllAsyncV2();
        Task<IEnumerable<OpenCardDto>> OpenGetAllAsync();
        Task<CardDto> CreateAsync(CardCreateDto createDto);
        Task UpdatesAsync(Guid id, CardEditDto dto);
        Task UpdateAccessAsync(Guid id, CardAccessEdit dto) ;
        Task<CardMinimalsDto> CreatesAsync(CardAddDto dto);
        Task UpdateAsync(Guid id, CardUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
         Task<IEnumerable<CardDto>> ImportAsync(IFormFile file);
    }
}