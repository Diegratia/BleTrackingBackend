using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.Repository;
using Repositories.Repository.Analytics;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Analytics;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class AlarmAnalyticsIncidentService : BaseService, IAlarmAnalyticsIncidentService
    {
        private readonly AlarmAnalyticsIncidentRepository _repository;
        private readonly AlarmCategorySettingsRepository _alarmCategorySettingsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AlarmAnalyticsIncidentService> _logger;

        public AlarmAnalyticsIncidentService(
            AlarmAnalyticsIncidentRepository repository,
            AlarmCategorySettingsRepository alarmCategorySettingsRepository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor http,
            ILogger<AlarmAnalyticsIncidentService> logger) : base(http)
        {
            _repository = repository;
            _alarmCategorySettingsRepository = alarmCategorySettingsRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AlarmAreaChartResponseRead> GetAreaSummaryChartAsync(
            AlarmAnalyticsFilter request,
            AlarmGroupByMode groupByMode = AlarmGroupByMode.Area)
        {
            var rows = await _repository.GetAreaDailySummaryAsync(request, groupByMode);

            // Filter out rows with null dates or null alarm status (data integrity issues)
            rows = rows.Where(r => r.Date != default && !string.IsNullOrWhiteSpace(r.AlarmStatus)).ToList();

            // Get alarm category settings for colors
            var alarmCategorySettings = await _alarmCategorySettingsRepository.GetAllAsync();
            var colorMap = alarmCategorySettings
                .Where(x => !string.IsNullOrWhiteSpace(x.AlarmCategory) && !string.IsNullOrWhiteSpace(x.AlarmColor))
                .ToDictionary(x => x.AlarmCategory!, x => x.AlarmColor!);

            // labels (dates)
            var dates = rows
                .Select(r => r.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var labels = dates
                .Select(d => d.ToString("yyyy-MM-dd"))
                .ToList();

            // group per entity (area/building/floor/floorplan based on mode)
            var entities = rows
                .GroupBy(r => new { r.EntityId, r.Name })
                .Select(entityGroup =>
                {
                    var statuses = entityGroup
                        .Select(x => x.AlarmStatus)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct();

                    var series = statuses.Select(status => new global::Shared.Contracts.ChartSeriesDto
                    {
                        Name = status,
                        Color = colorMap.TryGetValue(status, out var color) ? color : null,
                        Data = dates.Select(date =>
                            entityGroup.FirstOrDefault(r =>
                                r.Date == date &&
                                r.AlarmStatus == status
                            )?.Total ?? 0
                        ).ToList()
                    }).ToList();

                    return new AlarmAreaSeriesRead
                    {
                        EntityId = entityGroup.Key.EntityId,
                        Name = entityGroup.Key.Name ?? "Unknown",
                        Series = series
                    };
                })
                .ToList();

            return new AlarmAreaChartResponseRead
            {
                Labels = labels,
                Areas = entities
            };
        }

        public async Task<List<AlarmDailyRead>> GetDailySummaryAsync(AlarmAnalyticsFilter request)
        {
            return await _repository.GetDailySummaryAsync(request);
        }

        public async Task<List<AlarmStatusRead>> GetStatusSummaryAsync(AlarmAnalyticsFilter request)
        {
            return await _repository.GetStatusSummaryAsync(request);
        }

        public async Task<List<AlarmVisitorRead>> GetVisitorSummaryAsync(AlarmAnalyticsFilter request)
        {
            return await _repository.GetVisitorSummaryAsync(request);
        }

        public async Task<List<AlarmBuildingRead>> GetBuildingSummaryAsync(AlarmAnalyticsFilter request)
        {
            return await _repository.GetBuildingSummaryAsync(request);
        }

        public async Task<List<AlarmHourlyStatusRead>> GetHourlyStatusSummaryAsync(AlarmAnalyticsFilter request)
        {
            return await _repository.GetHourlyStatusSummaryAsync(request);
        }

        public async Task<List<AlarmInvestigatedResultRead>> GetInvestigatedResultSummaryAsync(AlarmAnalyticsFilter request)
        {
            return await _repository.GetInvestigatedResultSummaryAsync(request);
        }

        public async Task<List<AlarmDurationAnalyticsRead>> GetAverageDurationSummaryAsync(AlarmAnalyticsFilter request)
        {
            var incidents = await _repository.GetRawIncidentDurationsAsync(request);

            if (!incidents.Any())
                return new List<AlarmDurationAnalyticsRead>();

            var groupedIncidents = incidents.GroupBy(x => x.AlarmStatus);
            var resultList = new List<AlarmDurationAnalyticsRead>();

            foreach (var group in groupedIncidents)
            {
                var totalDurations = new List<double>();
                var responseDurations = new List<double>();
                var resolutionDurations = new List<double>();

                foreach (var incident in group)
                {
                    if (!incident.TriggerTime.HasValue) continue;

                    var triggerTime = incident.TriggerTime.Value;

                    // Gather all potential subsequent event timestamps
                    var allEventTimes = new List<DateTime?> {
                        incident.AcknowledgedAt, incident.DispatchedAt, incident.WaitingTimestamp,
                        incident.AcceptedAt, incident.ArrivedAt, incident.InvestigatedDoneAt,
                        incident.DoneTimestamp, incident.CancelTimestamp
                    };
                    
                    var validAllEventTimes = allEventTimes.Where(t => t.HasValue).Select(t => t.Value).ToList();
                    
                    DateTime? firstActionTime = null;
                    DateTime? lastEventTime = null;

                    if (validAllEventTimes.Any())
                    {
                        var sortedEvents = validAllEventTimes.OrderBy(t => t).ToList();
                        firstActionTime = sortedEvents.First(); // This acts as timeline.Skip(1).First() since TriggerTime is excluded
                        lastEventTime = sortedEvents.Last();    // This acts as timeline.OrderByDescending(t => t.Timestamp).First()
                    }

                    if (lastEventTime.HasValue)
                    {
                        totalDurations.Add((lastEventTime.Value - triggerTime).TotalSeconds);
                    }

                    if (firstActionTime.HasValue)
                    {
                        responseDurations.Add((firstActionTime.Value - triggerTime).TotalSeconds);
                        
                        if (lastEventTime.HasValue && lastEventTime > firstActionTime)
                        {
                            resolutionDurations.Add((lastEventTime.Value - firstActionTime.Value).TotalSeconds);
                        }
                    }
                }

                var avgTotalSeconds = totalDurations.Any() ? totalDurations.Average() : 0;
                var avgResponseSeconds = responseDurations.Any() ? responseDurations.Average() : 0;
                var avgResolutionSeconds = resolutionDurations.Any() ? resolutionDurations.Average() : 0;

                resultList.Add(new AlarmDurationAnalyticsRead
                {
                    AlarmStatus = group.Key,
                    TotalSeconds = avgTotalSeconds,
                    TotalFormatted = FormatDuration(avgTotalSeconds),
                    ResponseTimeSeconds = avgResponseSeconds,
                    ResponseTimeFormatted = FormatDuration(avgResponseSeconds),
                    ResolutionTimeSeconds = avgResolutionSeconds,
                    ResolutionTimeFormatted = FormatDuration(avgResolutionSeconds)
                });
            }

            return resultList;
        }

        private string FormatDuration(double seconds)
        {
            if (seconds < 60)
            {
                return $"{(int)seconds} seconds";
            }
            else if (seconds < 3600)
            {
                var minutes = (int)(seconds / 60);
                var remainingSeconds = (int)(seconds % 60);
                return remainingSeconds > 0 ? $"{minutes} minutes {remainingSeconds} seconds" : $"{minutes} minutes";
            }
            else
            {
                var hours = (int)(seconds / 3600);
                var remainingMinutes = (int)((seconds % 3600) / 60);
                return remainingMinutes > 0 ? $"{hours} hours {remainingMinutes} minutes" : $"{hours} hours";
            }
        }
    }
}
