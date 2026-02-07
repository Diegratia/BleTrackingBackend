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

        public async Task<object> GetAreaSummaryChartAsync(
            AlarmAnalyticsRequestRM request
        )
        {
            try
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

                var response = new AlarmAreaChartResponseDto
                {
                    Labels = labels,
                    Areas = areas
                };

                return ApiResponse.Success("Area summary chart retrieved successfully", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting area summary chart");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }


        public async Task<object> GetDailySummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetDailySummaryAsync(request);
                var dto = _mapper.Map<List<AlarmDailySummaryDto>>(data);
                return ApiResponse.Success("Incident daily summary retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident daily summary");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetStatusSummaryAsync(request);
                var dto = _mapper.Map<List<AlarmStatusSummaryDto>>(data);
                return ApiResponse.Success("Incident status summary retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident status summary");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetVisitorSummaryAsync(request);
                var dto = _mapper.Map<List<AlarmVisitorSummaryDto>>(data);
                return ApiResponse.Success("Incident visitor summary retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident visitor summary");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetBuildingSummaryAsync(request);
                var dto = _mapper.Map<List<AlarmBuildingSummaryDto>>(data);
                return ApiResponse.Success("Incident building summary retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident building summary");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }

        public async Task<object> GetHourlyStatusSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetHourlyStatusSummaryAsync(request);
                var dto = _mapper.Map<List<AlarmHourlyStatusSummaryDto>>(data);
                return ApiResponse.Success("Incident Daily(24 Hours) summary retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident Daily(24 Hours) summary");
                return ApiResponse.InternalError($"Internal server error: {ex.Message}");
            }
        }
    }
}
