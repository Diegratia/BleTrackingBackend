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
using Shared.Contracts.Analytics;
using DataView;
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

        public async Task<List<TrackingAreaRead>> GetAreaSummaryAsync(TrackingAnalyticsFilter request)
        {
            return await _repository.GetAreaSummaryAsync(request);
        }

        public async Task<List<TrackingDailyRead>> GetDailySummaryAsync(TrackingAnalyticsFilter request)
        {
            return await _repository.GetDailySummaryAsync(request);
        }

        public async Task<List<TrackingReaderRead>> GetReaderSummaryAsync(TrackingAnalyticsFilter request)
        {
            return await _repository.GetReaderSummaryAsync(request);
        }

        public async Task<List<TrackingVisitorRead>> GetVisitorSummaryAsync(TrackingAnalyticsFilter request)
        {
            return await _repository.GetVisitorSummaryAsync(request);
        }

        public async Task<List<TrackingBuildingRead>> GetBuildingSummaryAsync(TrackingAnalyticsFilter request)
        {
            return await _repository.GetBuildingSummaryAsync(request);
        }

        public async Task<List<TrackingMovementRead>> GetTrackingMovementByCardIdAsync(Guid cardId)
        {
            var data = await _repository.GetTrackingMovementByCardIdAsync(cardId);
            if (data.Count == 0)
                throw new NotFoundException($"No tracking movement found for card ID: {cardId}");
            return data;
        }

        // ===========================================================
        // 🔹 Get Heatmap
        // ===========================================================
        public async Task<List<TrackingHeatmapRead>> GetHeatmapDataAsync(TrackingAnalyticsFilter request)
        {
            return await _repository.GetHeatmapDataAsync(request);
        }

        public async Task<List<TrackingCardRead>> GetCardSummaryAsync(TrackingAnalyticsFilter request)
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

            return data;
        }

        public async Task<AreaAccessResponseDto> GetAreaAccessedSummaryAsyncV3(
            TrackingAnalyticsFilter request
        )
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

            return response;
        }
    }
}
