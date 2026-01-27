using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels;
using Data.ViewModels.AlarmAnalytics;
using Data.ViewModels.ResponseHelper;
using Microsoft.Extensions.Logging;
using Repositories.Repository.Analytics;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class AlarmAnalyticsIncidentService : IAlarmAnalyticsIncidentService
    {
        private readonly AlarmAnalyticsIncidentRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<AlarmAnalyticsIncidentService> _logger;

        public AlarmAnalyticsIncidentService(
            AlarmAnalyticsIncidentRepository repository,
            IMapper mapper,
            ILogger<AlarmAnalyticsIncidentService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        // public async Task<ResponseCollection<AlarmAreaSummaryDto>> GetAreaSummaryAsync(AlarmAnalyticsRequestRM request)
        // {
        //     try
        //     {
        //         var data = await _repository.GetAreaSummaryAsync(request);
        //         var dto = _mapper.Map<List<AlarmAreaSummaryDto>>(data);
        //         return ResponseCollection<AlarmAreaSummaryDto>.Ok(dto, "Incident area summary retrieved successfully");
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error getting incident area summary");
        //         return ResponseCollection<AlarmAreaSummaryDto>.Error($"Internal server error: {ex.Message}");
        //     }
        // }

        public async Task<object> GetAreaSummaryChartAsync(
            AlarmAnalyticsRequestRM request
        )
        {
            var rows = await _repository.GetAreaDailySummaryAsync(request);

            // labels (dates)
            var dates = rows
                .Select(r => r.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var labels = dates
                .Select(d => d.ToString("yyyy-MM-dd"))
                .ToList();

            // group per area
            var areas = rows
                .GroupBy(r => new { r.AreaId, r.AreaName })
                .Select(areaGroup =>
                {
                    var statuses = areaGroup
                        .Select(x => x.AlarmStatus)
                        .Distinct();

                    var series = statuses.Select(status => new ChartSeriesDto
                    {
                        Name = status,
                        Data = dates.Select(date =>
                            areaGroup.FirstOrDefault(r =>
                                r.Date == date &&
                                r.AlarmStatus == status
                            )?.Total ?? 0
                        ).ToList()
                    }).ToList();

                    return new AlarmAreaChartDto
                    {
                        AreaId = areaGroup.Key.AreaId,
                        AreaName = areaGroup.Key.AreaName,
                        Series = series
                    };
                })
                .ToList();

            var result = new AlarmAreaChartResponseDto
            {
                Labels = labels,
                Areas = areas
            };
            return result;
        }


        public async Task<ResponseCollection<AlarmDailySummaryDto>> GetDailySummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetDailySummaryAsync(request);
                var dto = _mapper.Map<List<AlarmDailySummaryDto>>(data);
                return ResponseCollection<AlarmDailySummaryDto>.Ok(dto, "Incident daily summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident daily summary");
                return ResponseCollection<AlarmDailySummaryDto>.Error($"Internal server error: {ex.Message}");
            }
        }

        public async Task<ResponseCollection<AlarmStatusSummaryDto>> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetStatusSummaryAsync(request);
                var dto = _mapper.Map<List<AlarmStatusSummaryDto>>(data);
                return ResponseCollection<AlarmStatusSummaryDto>.Ok(dto, "Incident status summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident status summary");
                return ResponseCollection<AlarmStatusSummaryDto>.Error($"Internal server error: {ex.Message}");
            }
        }

        public async Task<ResponseCollection<AlarmVisitorSummaryDto>> GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetVisitorSummaryAsync(request);
                var dto = _mapper.Map<List<AlarmVisitorSummaryDto>>(data);
                return ResponseCollection<AlarmVisitorSummaryDto>.Ok(dto, "Incident visitor summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident visitor summary");
                return ResponseCollection<AlarmVisitorSummaryDto>.Error($"Internal server error: {ex.Message}");
            }
        }

        public async Task<ResponseCollection<AlarmBuildingSummaryDto>> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetBuildingSummaryAsync(request);
                var dto = _mapper.Map<List<AlarmBuildingSummaryDto>>(data);
                return ResponseCollection<AlarmBuildingSummaryDto>.Ok(dto, "Incident building summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident building summary");
                return ResponseCollection<AlarmBuildingSummaryDto>.Error($"Internal server error: {ex.Message}");
            }
        }
        public async Task<ResponseCollection<AlarmHourlyStatusSummaryDto>> GetHourlyStatusSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetHourlyStatusSummaryAsync(request);
                var dto = _mapper.Map<List<AlarmHourlyStatusSummaryDto>>(data);
                return ResponseCollection<AlarmHourlyStatusSummaryDto>.Ok(dto, "Incident Daily(24 Hours) summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident Daily(24 Hours) summary");
                return ResponseCollection<AlarmHourlyStatusSummaryDto>.Error($"Internal server error: {ex.Message}");
            }
        }
    }
}
