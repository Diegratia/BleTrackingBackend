using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Helpers.Consumer;
using Microsoft.Extensions.Logging;
using Repositories.Repository.Analytics;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Data.ViewModels.AlarmAnalytics;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class TrackingSummaryService : ITrackingSummaryService
    {
        private readonly TrackingSummaryRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TrackingSummaryService> _logger;

        public TrackingSummaryService(
            TrackingSummaryRepository repository,
            IMapper mapper,
            ILogger<TrackingSummaryService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<object> GetAreaSummaryAsync(TrackingAnalyticsFilter request)
        {
            try
            {
                var data = await _repository.GetAreaSummaryAsync(request);
                var dto = _mapper.Map<List<TrackingAreaSummaryDto>>(data);
                return ApiResponse.Success("Area summary retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting area summary");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetDailySummaryAsync(TrackingAnalyticsFilter request)
        {
            try
            {
                var data = await _repository.GetDailySummaryAsync(request);
                var dto = _mapper.Map<List<TrackingDailySummaryDto>>(data);
                return ApiResponse.Success("Daily summary retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily summary");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetReaderSummaryAsync(TrackingAnalyticsFilter request)
        {
            try
            {
                var data = await _repository.GetReaderSummaryAsync(request);
                var dto = _mapper.Map<List<TrackingReaderSummaryDto>>(data);
                return ApiResponse.Success("Reader summary retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reader summary");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetVisitorSummaryAsync(TrackingAnalyticsFilter request)
        {
            try
            {
                var data = await _repository.GetVisitorSummaryAsync(request);
                var dto = _mapper.Map<List<TrackingVisitorSummaryDto>>(data);
                return ApiResponse.Success("Visitor summary retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visitor summary");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetBuildingSummaryAsync(TrackingAnalyticsFilter request)
        {
            try
            {
                var data = await _repository.GetBuildingSummaryAsync(request);
                var dto = _mapper.Map<List<TrackingBuildingSummaryDto>>(data);
                return ApiResponse.Success("Building summary retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting building summary");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetTrackingMovementByCardIdAsync(Guid cardId)
        {
            try
            {
                var data = await _repository.GetTrackingMovementByCardIdAsync(cardId);
                return ApiResponse.Success("Tracking movement retrieved successfully", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting tracking movement by card ID {CardId}", cardId);
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        // ===========================================================
        // 🔹 Get Heatmap
        // ===========================================================
        public async Task<object> GetHeatmapDataAsync(TrackingAnalyticsFilter request)
        {
            try
            {
                var data = await _repository.GetHeatmapDataAsync(request);
                return ApiResponse.Success("Tracking heatmap retrieved successfully", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting heatmap data");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetCardSummaryAsync(TrackingAnalyticsFilter request)
        {
            try
            {
                var data = await _repository.GetCardSummaryAsync(request);

                var tz = TimezoneHelper.Resolve(request.Timezone);

                if (tz.Id != TimeZoneInfo.Utc.Id)
                {
                    foreach (var item in data)
                    {
                        item.LastDetectedAt =
                            TimezoneHelper.ConvertFromUtc(item.LastDetectedAt, tz);
                    }
                }

                return ApiResponse.Success("Tracking summary retrieved successfully", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting card summary");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetAreaAccessedSummaryAsyncV3(
            TrackingAnalyticsFilter request
        )
        {
            try
            {
                var rows = await _repository.GetAreaAccessDailyAsync(request);

                var dates = rows
                    .Select(x => x.Date.Date)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                var labels = dates
                    .Select(d => d.ToString("MMM d"))
                    .ToList();

                List<int> BuildSeries(string status) =>
                    dates.Select(d =>
                        rows.FirstOrDefault(r =>
                            r.Date.Date == d &&
                            r.RestrictedStatus == status
                        )?.Total ?? 0
                    ).ToList();

                var withPermission = BuildSeries("non-restrict");
                var withoutPermission = BuildSeries("restrict");

                var accessedArea = withPermission
                    .Zip(withoutPermission, (a, b) => a + b)
                    .ToList();

                var response = new AreaAccessResponseDto
                {
                    Summary = new AreaAccessSummaryDto
                    {
                        AccessedAreaTotal = accessedArea.Sum(),
                        WithPermission = withPermission.Sum(),
                        WithoutPermission = withoutPermission.Sum()
                    },
                    Chart = new AreaAccessChartDto
                    {
                        Labels = labels,
                        Series = new()
                    {
                        new Shared.Contracts.ChartSeriesDto
                        {
                            Name = "Accessed Area",
                            Data = accessedArea
                        },
                        new Shared.Contracts.ChartSeriesDto
                        {
                            Name = "With Permission",
                            Data = withPermission
                        },
                        new Shared.Contracts.ChartSeriesDto
                        {
                            Name = "Without Permission",
                            Data = withoutPermission
                        }
                    }
                    }
                };

                return ApiResponse.Success("Success", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting area accessed summary");
                return ApiResponse.InternalError("Internal server error");
            }
        }
    }
}
