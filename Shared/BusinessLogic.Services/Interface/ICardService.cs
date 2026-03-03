using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface ICardService
    {
        Task<CardRead?> GetByIdAsync(Guid id);
        Task AssignToMemberAsync(Guid id, CardAssignDto dto);
        Task<IEnumerable<CardRead>> GetAllAsync();
        Task<IEnumerable<CardRead>> GetAllUnUsedAsync();
        Task<IEnumerable<OpenCardDto>> OpenGetAllUnUsedAsync();
        Task<IEnumerable<OpenCardDto>> OpenGetAllAsync();
        Task<CardRead> CreateAsync(CardCreateDto createDto);
        Task<CardRead> CreateMinimalAsync(CardAddDto dto);
        Task<IEnumerable<CardRead>> BulkAddAsync(List<CardAddDto> dtos);
        Task UpdatesAsync(Guid id, CardEditDto dto);
        Task UpdateAccessAsync(Guid id, CardAccessEdit dto);
        Task UpdateAsync(Guid id, CardUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, CardFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task<IEnumerable<CardRead>> ImportAsync(IFormFile file);
        Task UpdateAccessByVMSAsync(string cardNumber, CardAccessEdit dto);
    }
}