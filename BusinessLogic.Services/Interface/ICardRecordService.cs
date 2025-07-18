using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface ICardRecordService
    {
        Task<CardRecordDto> GetByIdAsync(Guid id);
        Task<IEnumerable<CardRecordDto>> GetAllAsync();
        Task<CardRecordDto> CreateAsync(CardRecordCreateDto createDto);
        Task UpdateAsync(Guid id, CardRecordUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync(); 
    }
}