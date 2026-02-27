using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository.Analytics
{

    public class AlarmAnalyticsIncidentRepository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;

        public AlarmAnalyticsIncidentRepository(BleTrackingDbContext context, IHttpContextAccessor accessor)
            : base(context, accessor)
        {
            _context = context;
        }

        // total alarm per area
        // public async Task<List<AlarmAreaSummaryRM>> GetAreaSummaryAsync(AlarmAnalyticsFilter request)
        // {
        //     var range = GetTimeRange(request.TimeRange ?? "weekly");
        //     var (from, to) = (range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7), range?.to ?? request.To ?? DateTime.UtcNow);
        //     var query = _context.AlarmRecordTrackings
        //         .AsNoTracking()
        //         .Include(a => a.FloorplanMaskedArea)
        //         .Where(a => a.Timestamp >= from && a.Timestamp <= to);

        //     query = ApplyFilters(query, request);

        //     var data = await query
        //         .Select(a => new
        //         {
        //             a.AlarmTriggersId,
        //             a.FloorplanMaskedAreaId,
        //             AreaName = a.FloorplanMaskedArea.Name,
        //             AlarmStatus = a.Alarm
        //         })
        //         .Distinct()
        //         .GroupBy(x => new
        //         {
        //             x.FloorplanMaskedAreaId,
        //             x.AreaName,
        //             x.AlarmStatus
        //         })
        //         .Select(g => new AlarmAreaSummaryRM
        //         {
        //             AreaId = g.Key.FloorplanMaskedAreaId,
        //             AreaName = g.Key.AreaName ?? "Unknown",
        //             AlarmStatus = g.Key.AlarmStatus.ToString() ?? "Unknown",
        //             Total = g.Count()
        //         })
        //         .ToListAsync();

        //     return data;
        // }

        public async Task<List<AlarmAreaDailyAggregateRM>> GetAreaDailySummaryAsync(
            AlarmAnalyticsFilter request,
            AlarmGroupByMode groupByMode = AlarmGroupByMode.Area
        )
        {
            var range = GetTimeRange(request.TimeRange);
            var from = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7);
            var to   = range?.to   ?? request.To   ?? DateTime.UtcNow;

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.FloorplanMaskedArea.Floorplan.Floor.Building)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            // Get raw data first, then group in memory based on mode
            var rawData = await query
                .Where(a => a.Timestamp.HasValue)
                .Select(a => new
                {
                    Date = a.Timestamp.Value.Date,
                    a.AlarmTriggersId,
                    a.Alarm,
                    AreaId = a.FloorplanMaskedAreaId,
                    AreaName = a.FloorplanMaskedArea.Name,
                    FloorplanId = a.FloorplanMaskedArea.FloorplanId,
                    FloorplanName = a.FloorplanMaskedArea.Floorplan.Name,
                    FloorId = a.FloorplanMaskedArea.Floorplan.FloorId,
                    FloorName = a.FloorplanMaskedArea.Floorplan.Floor.Name,
                    BuildingId = a.FloorplanMaskedArea.Floorplan.Floor.BuildingId,
                    BuildingName = a.FloorplanMaskedArea.Floorplan.Floor.Building.Name
                })
                .ToListAsync();

            // Group by entity based on mode
            var grouped = groupByMode switch
            {
                AlarmGroupByMode.Building => rawData
                    .GroupBy(x => new { x.Date, x.BuildingId, x.BuildingName, x.Alarm })
                    .Select(g => new AlarmAreaDailyAggregateRM
                    {
                        Date = g.Key.Date,
                        EntityId = g.Key.BuildingId,
                        Name = g.Key.BuildingName ?? "Unknown",
                        AlarmStatus = g.Key.Alarm.ToString(),
                        Total = g.Select(x => x.AlarmTriggersId).Distinct().Count()
                    }),

                AlarmGroupByMode.Floor => rawData
                    .GroupBy(x => new { x.Date, x.FloorId, x.FloorName, x.Alarm })
                    .Select(g => new AlarmAreaDailyAggregateRM
                    {
                        Date = g.Key.Date,
                        EntityId = g.Key.FloorId,
                        Name = g.Key.FloorName ?? "Unknown",
                        AlarmStatus = g.Key.Alarm.ToString(),
                        Total = g.Select(x => x.AlarmTriggersId).Distinct().Count()
                    }),

                AlarmGroupByMode.Floorplan => rawData
                    .GroupBy(x => new { x.Date, x.FloorplanId, x.FloorplanName, x.Alarm })
                    .Select(g => new AlarmAreaDailyAggregateRM
                    {
                        Date = g.Key.Date,
                        EntityId = g.Key.FloorplanId,
                        Name = g.Key.FloorplanName ?? "Unknown",
                        AlarmStatus = g.Key.Alarm.ToString(),
                        Total = g.Select(x => x.AlarmTriggersId).Distinct().Count()
                    }),

                _ => rawData // Area (default)
                    .GroupBy(x => new { x.Date, x.AreaId, x.AreaName, x.Alarm })
                    .Select(g => new AlarmAreaDailyAggregateRM
                    {
                        Date = g.Key.Date,
                        EntityId = g.Key.AreaId,
                        Name = g.Key.AreaName ?? "Unknown",
                        AlarmStatus = g.Key.Alarm.ToString(),
                        Total = g.Select(x => x.AlarmTriggersId).Distinct().Count()
                    })
            };

            return grouped
                .OrderBy(x => x.Date)
                .ToList();
        }


        // alarm per day
        public async Task<List<AlarmDailyRead>> GetDailySummaryAsync(AlarmAnalyticsFilter request)
        {
            // 🕒 Gunakan helper untuk override from-to
            var range = GetTimeRange(request.TimeRange ?? "weekly");
            var from = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7);
            var to = range?.to ?? request.To ?? DateTime.UtcNow;

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            var incidents = await query
                .Select(a => new
                {
                    Date = a.Timestamp.Value.Date,
                    a.AlarmTriggersId
                })
                .Distinct()
                .ToListAsync();

            var grouped = incidents
                .GroupBy(x => x.Date)
                .Select(g => new AlarmDailyRead
                {
                    Date = g.Key,
                    Total = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            return grouped;
        }




        // alarm per status
        public async Task<List<AlarmStatusRead>> GetStatusSummaryAsync(AlarmAnalyticsFilter request)
        {
            var range = GetTimeRange(request.TimeRange);
            var (from, to) = (
                range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7),
                range?.to ?? request.To ?? DateTime.UtcNow
            );

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            // Ambil data unik berdasarkan kombinasi AlarmTrigger + Status
            var incidents = await query
                .Select(a => new
                {
                    a.AlarmTriggersId,
                    a.Alarm
                })
                .Distinct()
                .ToListAsync();

            // GroupBy di memory untuk hasil yang aman dari translation EF
            var grouped = incidents
                .GroupBy(x => x.Alarm)
                .Select(g => new AlarmStatusRead
                {
                    Status = g.Key.HasValue ? g.Key.Value.ToString() : "Unknown",
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(3)
                .ToList();

            if (grouped.Count == 0)
            {
                var random = new Random();
                var statuses = Enum.GetValues(typeof(AlarmRecordStatus)).Cast<AlarmRecordStatus>().ToList();
                var randomStatuses = statuses.OrderBy(x => random.Next()).Take(3).Select(x => x.ToString()).ToList();

                grouped.AddRange(randomStatuses.Select(s => new AlarmStatusRead
                {
                    Status = s,
                    Total = 0
                }));
            }

            return grouped;
        }



        // alarm per visitor
        public async Task<List<AlarmVisitorRead>> GetVisitorSummaryAsync(AlarmAnalyticsFilter request)
        {
            var (from, to) = (
                request.From ?? DateTime.UtcNow.AddDays(-7),
                request.To ?? DateTime.UtcNow
            );

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.Visitor)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            // Ambil kombinasi unik antara AlarmTriggerId dan Visitor
            var incidents = await query
                .Select(a => new
                {
                    a.AlarmTriggersId,
                    a.VisitorId,
                    VisitorName = a.Visitor != null ? a.Visitor.Name : "Unknown"
                })
                .Distinct()
                .ToListAsync();

            // Lakukan grouping di memory
            var grouped = incidents
                .GroupBy(x => new { x.VisitorId, x.VisitorName })
                .Select(g => new AlarmVisitorRead
                {
                    VisitorId = g.Key.VisitorId,
                    VisitorName = g.Key.VisitorName,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            return grouped;
        }



        // alarm per building
        public async Task<List<AlarmBuildingRead>> GetBuildingSummaryAsync(AlarmAnalyticsFilter request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.FloorplanMaskedArea.Floorplan.Floor.Building)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            var incidents = await query
                .Select(a => new
                {
                    a.AlarmTriggersId,
                    BuildingId = a.FloorplanMaskedArea.Floorplan.Floor.Building.Id,
                    BuildingName = a.FloorplanMaskedArea.Floorplan.Floor.Building.Name
                })
                .Distinct()
                .ToListAsync();

            var grouped = incidents
                .GroupBy(x => new { x.BuildingId, x.BuildingName })
                .Select(g => new AlarmBuildingRead
                {
                    BuildingId = g.Key.BuildingId,
                    BuildingName = g.Key.BuildingName,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();
            return grouped;
        }

       public async Task<List<AlarmHourlyStatusRead>> GetHourlyStatusSummaryAsync(AlarmAnalyticsFilter request)
{
    // Range penuh 1 hari
    var date = request.From?.Date ?? DateTime.UtcNow.Date;

    var from = date;                              // 00:00
    var to = date.AddDays(1).AddTicks(-1);        // 23:59:59.9999999

    var query = _context.AlarmRecordTrackings
        .AsNoTracking()
        .Where(a => a.Timestamp >= from && a.Timestamp <= to);

    query = ApplyFilters(query, request);

    // STEP 1: Ambil *alarm pertama* dari setiap AlarmTriggersId dalam 1 hari
    var incidents = await query
        .GroupBy(a => a.AlarmTriggersId)
        .Select(g => new
        {
            AlarmTriggersId = g.Key,
            FirstTimestamp = g.Min(x => x.Timestamp),   // ambil jam pertama
            Status = g.OrderBy(x => x.Timestamp)
                      .First().Alarm.ToString() ?? "Unknown"
        })
        .ToListAsync();

    // STEP 2: Konversi ke hour
    var flattened = incidents
        .Select(x => new
        {
            Hour = x.FirstTimestamp.Value.Hour,
            x.Status
        })
        .ToList();

    // STEP 3: Grouping by hour + status
    var groupedHours = flattened
        .GroupBy(x => x.Hour)
        .ToDictionary(
            g => g.Key,
            g => new AlarmHourlyStatusRead
            {
                Hour = g.Key,
                HourLabel = g.Key.ToString("00") + ".00",
                Status = g.GroupBy(x => x.Status)
                            .ToDictionary(
                                s => s.Key,
                                s => s.Count()
                            )
            }
        );

    // STEP 4: Pastikan output 24 hours lengkap
    var fullDay = Enumerable.Range(0, 24)
        .Select(hour =>
            groupedHours.ContainsKey(hour)
                ? groupedHours[hour]
                : new AlarmHourlyStatusRead
                {
                    Hour = hour,
                    HourLabel = hour.ToString("00") + ".00",
                    Status = new Dictionary<string, int>()
                }
        )
        .OrderBy(x => x.Hour)
        .ToList();

    return fullDay;
}



        //    public async Task<List<(Guid BuildingId, string BuildingName, int Total)>> GetBuildingSummaryAsync(AlarmAnalyticsFilter request)
        //         {
        //             var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);

        //             var query = _context.AlarmRecordTrackings
        //                 .AsNoTracking()
        //                 .Include(a => a.FloorplanMaskedArea.Floorplan.Floor.Building)
        //                 .Where(a => a.Timestamp >= from && a.Timestamp <= to);

        //             query = ApplyFilters(query, request);

        //             var incidents = await query
        //                 .Select(a => new
        //                 {
        //                     a.AlarmTriggersId,
        //                     BuildingId = a.FloorplanMaskedArea.Floorplan.Floor.Building.Id,
        //                     BuildingName = a.FloorplanMaskedArea.Floorplan.Floor.Building.Name
        //                 })
        //                 .Distinct()
        //                 .ToListAsync();

        //             return incidents
        //                 .GroupBy(x => new { x.BuildingId, x.BuildingName })
        //                 .Select(g => (g.Key.BuildingId, g.Key.BuildingName, g.Count()))
        //                 .ToList();
        // }


        // ===================================================================
        // 6️⃣ Helper Filter
        // ===================================================================
        private IQueryable<AlarmRecordTracking> ApplyFilters(IQueryable<AlarmRecordTracking> query, AlarmAnalyticsFilter request)
        {
            if (request.BuildingId.HasValue)
                query = query.Where(a => a.FloorplanMaskedArea.Floorplan.Floor.Building.Id == request.BuildingId);

            if (request.FloorId.HasValue)
                query = query.Where(a => a.FloorplanMaskedArea.Floorplan.Floor.Id == request.FloorId);

            if (request.FloorplanMaskedAreaId.HasValue)
                query = query.Where(a => a.FloorplanMaskedAreaId == request.FloorplanMaskedAreaId);

            if (request.VisitorId.HasValue)
                query = query.Where(a => a.VisitorId == request.VisitorId);

            if (!string.IsNullOrWhiteSpace(request.OperatorName))
                query = query.Where(a => a.DoneBy == request.OperatorName);

            return query;
        }

        public async Task<List<AlarmInvestigatedResultRead>> GetInvestigatedResultSummaryAsync(AlarmAnalyticsFilter request)
        {
            var range = GetTimeRange(request.TimeRange);
            var (from, to) = (
                range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7),
                range?.to ?? request.To ?? DateTime.UtcNow
            );

            var query = _context.AlarmTriggers
                .AsNoTracking()
                .Where(a => a.TriggerTime >= from && a.TriggerTime <= to);

            if (request.BuildingId.HasValue)
            {
                query = query.Where(a => a.Floorplan != null &&
                    a.Floorplan.Floor != null &&
                    a.Floorplan.Floor.BuildingId == request.BuildingId);
            }

            if (request.FloorId.HasValue)
            {
                query = query.Where(a => a.Floorplan != null &&
                    a.Floorplan.FloorId == request.FloorId);
            }

            if (request.FloorplanMaskedAreaId.HasValue)
            {
                var areaFloorplanIds = await _context.FloorplanMaskedAreas
                    .Where(fma => fma.Id == request.FloorplanMaskedAreaId && fma.Status != 0)
                    .Select(fma => fma.FloorplanId)
                    .ToListAsync();

                query = query.Where(a => areaFloorplanIds.Contains(a.FloorplanId.Value));
            }

            if (request.VisitorId.HasValue)
            {
                query = query.Where(a => a.VisitorId == request.VisitorId);
            }

            if (!string.IsNullOrWhiteSpace(request.OperatorName))
            {
                query = query.Where(a => a.DoneBy == request.OperatorName);
            }

            var incidents = await query
                .Where(a => a.InvestigatedResult.HasValue)
                .Select(a => new
                {
                    a.InvestigatedResult
                })
                .ToListAsync();

            // Group by InvestigatedResult and count
            var grouped = incidents
                .Where(x => x.InvestigatedResult.HasValue)
                .GroupBy(x => x.InvestigatedResult!.Value)
                .Select(g => new AlarmInvestigatedResultRead
                {
                    InvestigatedResult = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            // If no results, return all enum values with 0 count
            if (grouped.Count == 0)
            {
                var allResults = Enum.GetValues(typeof(InvestigatedResult))
                    .Cast<InvestigatedResult>()
                    .Select(e => new AlarmInvestigatedResultRead
                    {
                        InvestigatedResult = e,
                        Total = 0
                    })
                    .ToList();

                return allResults;
            }

            return grouped;
        }

        public async Task<List<AlarmDurationAnalyticsRead>> GetAverageDurationSummaryAsync(AlarmAnalyticsFilter request)
        {
            var range = GetTimeRange(request.TimeRange);
            var (from, to) = (
                range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7),
                range?.to ?? request.To ?? DateTime.UtcNow
            );

            var query = _context.AlarmTriggers
                .AsNoTracking()
                .Where(a => a.TriggerTime >= from && a.TriggerTime <= to && a.Alarm.HasValue);

            if (request.BuildingId.HasValue)
            {
                query = query.Where(a => a.Floorplan != null &&
                    a.Floorplan.Floor != null &&
                    a.Floorplan.Floor.BuildingId == request.BuildingId);
            }

            if (request.FloorId.HasValue)
            {
                query = query.Where(a => a.Floorplan != null &&
                    a.Floorplan.FloorId == request.FloorId);
            }

            if (request.FloorplanMaskedAreaId.HasValue)
            {
                var areaFloorplanIds = await _context.FloorplanMaskedAreas
                    .Where(fma => fma.Id == request.FloorplanMaskedAreaId && fma.Status != 0)
                    .Select(fma => fma.FloorplanId)
                    .ToListAsync();

                query = query.Where(a => a.FloorplanId.HasValue && areaFloorplanIds.Contains(a.FloorplanId.Value));
            }

            if (request.VisitorId.HasValue)
            {
                query = query.Where(a => a.VisitorId == request.VisitorId);
            }

            if (!string.IsNullOrWhiteSpace(request.OperatorName))
            {
                query = query.Where(a => a.DoneBy == request.OperatorName);
            }

            var incidents = await query
                .Select(a => new
                {
                    AlarmStatus = a.Alarm.Value.ToString(),
                    TriggerTime = a.TriggerTime,
                    AcknowledgedAt = a.AcknowledgedAt,
                    DispatchedAt = a.DispatchedAt,
                    WaitingTimestamp = a.WaitingTimestamp,
                    AcceptedAt = a.AcceptedAt,
                    ArrivedAt = a.ArrivedAt,
                    InvestigatedDoneAt = a.InvestigatedDoneAt,
                    DoneTimestamp = a.DoneTimestamp,
                    CancelTimestamp = a.CancelTimestamp
                })
                .ToListAsync();

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

