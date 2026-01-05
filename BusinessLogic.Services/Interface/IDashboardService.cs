using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Interface
{
    public interface IDashboardService
    {
        Task<ResponseSingle<DashboardSummaryDto>> GetSummaryAsync();
        Task<ResponseSingle<CardUsageCountDto>> GetCardStatsAsync();
        Task<ResponseSingle<List<AreaSummaryDto>>> GetTopAreasAsync(int topCount = 5);
        Task<List<BlacklistLogRM>> GetBlacklistLogsAsync();
    }
}