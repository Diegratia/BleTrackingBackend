using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Interface
{
    public interface ICardRecordService
    {
        Task<CardRecordDto> GetByIdAsync(Guid id);
        Task<IEnumerable<CardRecordDto>> GetAllAsync();
        Task<CardRecordDto> CreateAsync(CardRecordCreateDto createDto);
        Task CheckoutCard(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task<IEnumerable<CardUsageSummaryRM>> GetCardUsageSummaryAsync(
        CardRecordRequestRM request); 
        Task<IEnumerable<CardUsageHistoryRM>> GetCardUsageHistoryAsync(
        CardRecordRequestRM request );
        Task<object> ProjectionFilterAsync(DataTablesRequest request);
    }
}