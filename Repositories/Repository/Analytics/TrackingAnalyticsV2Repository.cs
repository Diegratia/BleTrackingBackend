using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;
using Helpers.Consumer;

namespace Repositories.Repository.Analytics
{
    public class TrackingAnalyticsV2Repository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;
        private static readonly TimeZoneInfo WibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public TrackingAnalyticsV2Repository(BleTrackingDbContext context, IHttpContextAccessor accessor)
            : base(context, accessor)
        {
            _context = context;
        }

        // === GET TIME RANGE (sama seperti V1) ===
        private (DateTime from, DateTime to)? GetTimeRange(string? timeReport)
        {
            if (string.IsNullOrWhiteSpace(timeReport)) return null;
            var now = DateTime.UtcNow;
            return timeReport.Trim().ToLower() switch
            {
                "daily" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
                "weekly" => (
                    now.Date.AddDays(-(int)now.DayOfWeek + 1),
                    now.Date.AddDays(7 - (int)now.DayOfWeek).AddDays(1).AddTicks(-1)
                ),
                "monthly" => (
                    new DateTime(now.Year, now.Month, 1),
                    new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month))
                        .AddDays(1).AddTicks(-1)
                ),
                _ => null
            };
        }

        // === GET TABLE NAMES IN RANGE (WIB) ===
        private List<string> GetTableNamesInRange(DateTime fromUtc, DateTime toUtc)
        {
            var fromWib = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, WibZone).Date;
            var toWib = TimeZoneInfo.ConvertTimeFromUtc(toUtc, WibZone).Date;

            var tables = new List<string>();
            for (var d = fromWib; d <= toWib; d = d.AddDays(1))
            {
                tables.Add($"tracking_transaction_{d:yyyyMMdd}");
            }
            return tables;
        }

        // Di dalam class TrackingAnalyticsV2Repository

        // === DTO SEMENTARA (internal) ===
        internal class SessionRaw
        {
            public Guid? VisitorId { get; set; }
            public string? VisitorName { get; set; }
            // public Guid? MemberId { get; set; }
            // public string? MemberName { get; set; }
            public Guid? CardId { get; set; }
            public string? CardName { get; set; }
            public Guid? AreaId { get; set; }
            public string? AreaName { get; set; }
            public Guid? BuildingId { get; set; }
            public string? BuildingName { get; set; }
            public Guid? FloorId { get; set; }
            public string? FloorName { get; set; }
            public Guid? FloorplanId { get; set; }
            public string? FloorplanName { get; set; }
            public DateTime TransTimeUtc { get; set; }
            public Guid? PersonId => VisitorId;
            // public string? PersonName => VisitorName ?? MemberName;

            // BARU: Tipe orang
            public string? PersonType => VisitorId.HasValue ? "Visitor" : "Member";
            // public string? AlarmStatus { get; set; }
            // public string? HostName { get; set; }
        }

        // === BUILD SQL UNTUK SESSION ===
        private (string sql, List<object> parameters) BuildSessionSql(
            string tableName, DateTime fromUtc, DateTime toUtc,
            TrackingAnalyticsRequestRM request, int startParamIndex)
        {
            var sql = $@"
        SELECT 
            v.id AS VisitorId,
            v.name AS VisitorName,
            c.id AS CardId,
            c.name AS CardName,
            ma.id AS AreaId,
            ma.name AS AreaName,
            b.id AS BuildingId,
            b.name AS BuildingName,
            fl.id AS FloorId,
            fl.name AS FloorName,
            fp.id AS FloorplanId,
            fp.name AS FloorplanName,
            t.trans_time AS TransTimeUtc
        FROM [dbo].[{tableName}] t
        LEFT JOIN card c ON t.card_id = c.id
        LEFT JOIN visitor v ON c.visitor_id = v.id
        LEFT JOIN floorplan_masked_area ma ON t.floorplan_masked_area_id = ma.id
        LEFT JOIN mst_floorplan fp ON ma.floorplan_id = fp.id
        LEFT JOIN mst_floor fl ON fp.floor_id = fl.id
        LEFT JOIN mst_building b ON fl.building_id = b.id
        WHERE t.trans_time >= @p{startParamIndex}
          AND t.trans_time <= @p{startParamIndex + 1}
    ";

            var parameters = new List<object> { fromUtc, toUtc };
            int paramIndex = startParamIndex + 2;

            if (request.BuildingId.HasValue)
            {
                sql += $" AND b.id = @p{paramIndex++}";
                parameters.Add(request.BuildingId.Value);
            }
            if (request.FloorId.HasValue)
            {
                sql += $" AND fl.id = @p{paramIndex++}";
                parameters.Add(request.FloorId.Value);
            }
            if (request.AreaId.HasValue)
            {
                sql += $" AND ma.id = @p{paramIndex++}";
                parameters.Add(request.AreaId.Value);
            }
            if (request.VisitorId.HasValue)
            {
                sql += $" AND v.id = @p{paramIndex++}";
                parameters.Add(request.VisitorId.Value);
            }
            if (request.MemberId.HasValue)
            {
                sql += $" AND v.id = @p{paramIndex++}";
                parameters.Add(request.MemberId.Value);
            }

            return (sql, parameters);
        }

        // === MAIN METHOD: Get Visitor Session Summary ===
        public async Task<List<VisitorSessionSummaryRM>> GetVisitorSessionSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            var range = GetTimeRange(request.TimeRange);
            var fromUtc = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-1);
            var toUtc = range?.to ?? request.To ?? DateTime.UtcNow;

            var tableNames = GetTableNamesInRange(fromUtc, toUtc);
            if (!tableNames.Any()) return new List<VisitorSessionSummaryRM>();

            var unionParts = new List<string>();
            var allParameters = new List<object>();
            int paramIndex = 0;

            foreach (var table in tableNames)
            {
                var (sql, parameters) = BuildSessionSql(table, fromUtc, toUtc, request, paramIndex);
                unionParts.Add(sql);
                allParameters.AddRange(parameters);
                paramIndex += parameters.Count;
            }

            var fullSql = string.Join("\nUNION ALL\n", unionParts);

            var raw = await _context.Database
                .SqlQueryRaw<SessionRaw>(fullSql, allParameters.ToArray())
                .ToListAsync();

            if (!raw.Any()) return new List<VisitorSessionSummaryRM>();

            // === PROSES SESSION PER VISITOR PER AREA ===
            var sessions = new List<VisitorSessionSummaryRM>();
            var wibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            
        var grouped = raw
            .Where(x => x.PersonId.HasValue && x.AreaId.HasValue)
            .GroupBy(x => new { x.PersonId, x.AreaId })
            .ToList();

            foreach (var group in grouped)
            {
                // DI DALAM foreach (var group in grouped)
                var records = group.OrderBy(x => x.TransTimeUtc).ToList();
                VisitorSessionSummaryRM? current = null;

                foreach (var rec in records)
                {
                    // DI DALAM foreach (var rec in records)
                        var wibTime = TimeZoneInfo.ConvertTimeFromUtc(rec.TransTimeUtc, wibZone);

                        if (current == null)
                        {
                            current = MapToSession(rec, wibTime);
                        }
                        else
                        {
                            // HANYA jika pindah area
                            if (rec.AreaId != current.AreaId)
                            {
                                // Akhiri sesi lama → ExitTime = transaksi terakhir di area lama
                                current.ExitTime = wibTime.AddSeconds(-1);
                                current.DurationInMinutes = (int)(current.ExitTime.Value - current.EnterTime).TotalMinutes;
                                // Hanya simpan sesi jika durasi > 1 menit
                                if (current.DurationInMinutes >= 1)
                                {
                                    sessions.Add(current);
                                }

                                // Mulai sesi baru
                                current = MapToSession(rec, wibTime);
                            }
                        }
                    }

                // Akhiri sesi terakhir
                if (current != null)
                {
                    var lastRecord = group.Last();
                    var lastWib = TimeZoneInfo.ConvertTimeFromUtc(lastRecord.TransTimeUtc, wibZone);
                    var gapFromLast = (DateTime.UtcNow - lastRecord.TransTimeUtc).TotalMinutes;

                    if (gapFromLast > 5)
                    {
                        current.ExitTime = lastWib;
                        current.DurationInMinutes = (int)(lastWib - current.EnterTime).TotalMinutes;
                    }
                    else
                    {
                        current.ExitTime = null;
                        current.DurationInMinutes = null;
                    }
                    sessions.Add(current);
                }
            }

            return sessions
                .OrderByDescending(x => x.EnterTime)
                .ToList();
        }

                    private VisitorSessionSummaryRM MapToSession(SessionRaw rec, DateTime enterWib)
            {
                return new VisitorSessionSummaryRM
                {
                    // PersonId = rec.PersonId,
                    // PersonName = rec.PersonName,
                    PersonType = rec.PersonType,

                    VisitorId = rec.VisitorId,
                    VisitorName = rec.VisitorName,
                    // MemberId = rec.MemberId,
                    // MemberName = rec.MemberName,

                    AreaId = rec.AreaId,
                    AreaName = rec.AreaName,
                    BuildingId = rec.BuildingId,
                    BuildingName = rec.BuildingName,
                    FloorId = rec.FloorId,
                    FloorName = rec.FloorName,
                    FloorplanId = rec.FloorplanId,
                    FloorplanName = rec.FloorplanName,
                    EnterTime = enterWib,
                };
            }
    }
}