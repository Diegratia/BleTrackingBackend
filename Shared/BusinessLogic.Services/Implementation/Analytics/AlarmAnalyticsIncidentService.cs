using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.Repository.Analytics;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Analytics;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class AlarmAnalyticsIncidentService : BaseService, IAlarmAnalyticsIncidentService
    {
        private readonly AlarmAnalyticsIncidentRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<AlarmAnalyticsIncidentService> _logger;

        public AlarmAnalyticsIncidentService(
            AlarmAnalyticsIncidentRepository repository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor http,
            ILogger<AlarmAnalyticsIncidentService> logger) : base(http)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AlarmAreaChartResponseRead> GetAreaSummaryChartAsync(
            AlarmAnalyticsFilter request,
            AlarmGroupByMode groupByMode = AlarmGroupByMode.Area)
        {
            var rows = await _repository.GetAreaDailySummaryAsync(request, groupByMode);

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
                        .Distinct();

                    var series = statuses.Select(status => new global::Shared.Contracts.ChartSeriesDto
                    {
                        Name = status,
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
                        Name = entityGroup.Key.Name,
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
    }
}
