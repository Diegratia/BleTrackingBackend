using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Repositories.Repository.RepoModel;
using Shared.Contracts.Read;
using DataView;

namespace BusinessLogic.Services.Interface
{
    public interface ICardRecordService
    {
        // Use Read DTO for query operations (direct return from repository)
        Task<CardRecordRead> GetByIdAsync(Guid id);
        Task<IEnumerable<CardRecordRead>> GetAllAsync();

        // Use Read DTO for create (returns created entity as Read DTO)
        Task<CardRecordRead> CreateAsync(CardRecordCreateDto createDto);
        Task CheckoutCard(Guid id);

        // Filter with typed filter and DataTablesProjectedRequest
        Task<object> FilterAsync(DataTablesProjectedRequest request, Shared.Contracts.CardRecordFilter filter);

        // Keep RepoModel return types for specialized queries (unchanged)
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task<IEnumerable<CardUsageSummaryRM>> GetCardUsageSummaryAsync();
        Task<IEnumerable<CardUsageHistoryRM>> GetCardUsageHistoryAsync(CardRecordRequestRM request);

        // Keep old ProjectionFilterAsync for backward compatibility
        Task<object> ProjectionFilterAsync(DataTablesRequest request);
    }
}
