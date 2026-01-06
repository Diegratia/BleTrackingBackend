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
    using Microsoft.Extensions.Logging;

    namespace Repositories.Repository.Analytics
    {
        public class TrackingAnalyticsV2Repository : BaseRepository
        {
            private readonly BleTrackingDbContext _context;
            private readonly ILogger<TrackingAnalyticsV2Repository> _logger;
            private static readonly TimeZoneInfo WibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            public TrackingAnalyticsV2Repository(BleTrackingDbContext context,
            IHttpContextAccessor accessor,
            ILogger<TrackingAnalyticsV2Repository> logger)
                : base(context, accessor)
            {
                _context = context;
                _logger = logger;
            }


            private (DateTime from, DateTime to)? GetTimeRange(string? timeReport)
            {
                if (string.IsNullOrWhiteSpace(timeReport)) return null;

                var wibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var wibNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, wibZone);

                return timeReport.Trim().ToLower() switch
                {
                    "daily" =>
                    (
                        TimeZoneInfo.ConvertTimeToUtc(wibNow.Date, wibZone),
                        TimeZoneInfo.ConvertTimeToUtc(
                            wibNow.Date.AddDays(1).AddTicks(-1),
                            wibZone
                        )
                    ),

                    "weekly" =>
                    (
                        TimeZoneInfo.ConvertTimeToUtc(
                            wibNow.Date.AddDays(-(int)wibNow.DayOfWeek + 1),
                            wibZone
                        ),
                        TimeZoneInfo.ConvertTimeToUtc(
                            wibNow.Date.AddDays(7 - (int)wibNow.DayOfWeek)
                                .AddDays(1).AddTicks(-1),
                            wibZone
                        )
                    ),

                    "monthly" =>
                    (
                        TimeZoneInfo.ConvertTimeToUtc(
                            new DateTime(wibNow.Year, wibNow.Month, 1),
                            wibZone
                        ),
                        TimeZoneInfo.ConvertTimeToUtc(
                            new DateTime(wibNow.Year, wibNow.Month,
                                DateTime.DaysInMonth(wibNow.Year, wibNow.Month))
                            .AddDays(1).AddTicks(-1),
                            wibZone
                        )
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
                    // tables.Add($"tracking_transaction_{d:yyyyMMdd}");
                    var table = $"tracking_transaction_{d:yyyyMMdd}";
                    if (TableExists(table))
                        tables.Add(table);
                }
                return tables;
            }

            // Di dalam class TrackingAnalyticsV2Repository

            // === DTO SEMENTARA (internal) ===
            internal class SessionRaw
            {
                public Guid? PersonId => VisitorId ?? MemberId;
                public string? PersonName => VisitorName ?? MemberName;
                public string PersonType =>
                    VisitorId.HasValue ? "Visitor" :
                    MemberId.HasValue ? "Member" :
                    "Unknown";
                public string? IdentityId { get; set; }
                public Guid? CardId { get; set; }
                public string? CardName { get; set; }
                public string? CardNumber { get; set; }
                public Guid? VisitorId { get; set; }
                public string? VisitorName { get; set; }
                public Guid? MemberId { get; set; }
                public string? MemberName { get; set; }
                public Guid? AreaId { get; set; }
                public string? AreaName { get; set; }
                public Guid? BuildingId { get; set; }
                public string? BuildingName { get; set; }
                public Guid? FloorId { get; set; }
                public string? FloorName { get; set; }
                public Guid? FloorplanId { get; set; }
                public string? FloorplanImage { get; set; }
                public string? FloorplanName { get; set; }
                public DateTime TransTimeUtc { get; set; }
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
                m.id AS MemberId,
                m.name AS MemberName,
                COALESCE(v.identity_id, m.identity_id) AS IdentityId,
                c.id AS CardId,
                c.name AS CardName,
                c.card_number AS CardNumber,
                ma.id AS AreaId,
                ma.name AS AreaName,
                b.id AS BuildingId,
                b.name AS BuildingName,
                fl.id AS FloorId,
                fl.name AS FloorName,
                fp.id AS FloorplanId,
                fp.name AS FloorplanName,
                fp.floorplan_image AS FloorplanImage,
                t.trans_time AS TransTimeUtc
            FROM [dbo].[{tableName}] t
            LEFT JOIN card c ON t.card_id = c.id
            LEFT JOIN visitor v ON c.visitor_id = v.id
            LEFT JOIN mst_member m ON c.member_id = m.id
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
                    sql += $" AND m.id = @p{paramIndex++}";
                    parameters.Add(request.MemberId.Value);
                }
                // filter tipe person 
                if (!string.IsNullOrWhiteSpace(request.PersonType))
            {
                var type = request.PersonType.Trim().ToLower();

                if (type == "visitor")
                {
                    sql += $" AND v.id IS NOT NULL";
                }
                else if (type == "member")
                {
                    sql += $" AND m.id IS NOT NULL";
                }
            }

                        if (!string.IsNullOrWhiteSpace(request.IdentityId))
            {
                sql += $" AND (v.identity_id = @p{paramIndex} OR m.identity_id = @p{paramIndex})";
                parameters.Add(request.IdentityId);
                paramIndex++;
            }


                return (sql, parameters);
            }

            // === MAIN METHOD: Get Visitor Session Summary ===
           public async Task<List<VisitorSessionSummaryRM>> GetVisitorSessionSummaryAsync(
            TrackingAnalyticsRequestRM request)
        {
            var range = GetTimeRange(request.TimeRange);
            var fromUtc = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-1);
            var toUtc   = range?.to   ?? request.To   ?? DateTime.UtcNow;

            var tableNames = GetTableNamesInRange(fromUtc, toUtc);
            if (!tableNames.Any())
                return new List<VisitorSessionSummaryRM>();

            var unionParts   = new List<string>();
            var allParams    = new List<object>();
            int paramIndex   = 0;

            foreach (var table in tableNames)
            {
                var (sql, parameters) =
                    BuildSessionSql(table, fromUtc, toUtc, request, paramIndex);

                unionParts.Add(sql);
                allParams.AddRange(parameters);
                paramIndex += parameters.Count;
            }

            var fullSql = string.Join("\nUNION ALL\n", unionParts);

            var raw = await _context.Database
                .SqlQueryRaw<SessionRaw>(fullSql, allParams.ToArray())
                .ToListAsync();

            if (!raw.Any())
                return new List<VisitorSessionSummaryRM>();

            var wibZone  = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var nowWib   = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, wibZone);
            var sessions = new List<VisitorSessionSummaryRM>();

            // === GROUP BY PERSON + AREA (DWELL SUMMARY) ===
            var grouped = raw
                .Where(x => x.PersonId.HasValue && x.AreaId.HasValue)
                .GroupBy(x => new { x.PersonId, x.AreaId });

            foreach (var group in grouped)
            {
                var records = group
                    .OrderBy(x => x.TransTimeUtc)
                    .ToList();

                if (records.Count == 0)
                    continue;

                var first = records.First();
                var last  = records.Last();

                var enterWib = TimeZoneInfo.ConvertTimeFromUtc(
                    first.TransTimeUtc, wibZone);

                var lastWib = TimeZoneInfo.ConvertTimeFromUtc(
                    last.TransTimeUtc, wibZone);

                var session = MapToSession(first, enterWib);

                // === HANYA 1 HIT → SESSION MASIH AKTIF ===
                if (records.Count == 1)
                {
                    session.ExitTime = null;
                    session.DurationInMinutes = null;
                    sessions.Add(session);
                    continue;
                }

                // === SESSION SELESAI ===
                session.ExitTime = lastWib;
                session.DurationInMinutes =
                    (int)(lastWib - session.EnterTime).TotalMinutes;

                // jangan tampilkan session 0 menit
                if (session.DurationInMinutes >= 1)
                    sessions.Add(session);
            }

            return sessions
                .OrderByDescending(x => x.EnterTime)
                .ToList();
        }

        public async Task<List<VisitorSessionSummaryExportRM>> GetVisitorSessionSummaryExportAsync(TrackingAnalyticsRequestRM request)
        {
            var sessions = await GetVisitorSessionSummaryAsync(request);
            
            return sessions.Select(s => new VisitorSessionSummaryExportRM
            {
                // Properti yang sama dengan VisitorSessionSummaryRM
                VisitorId = s.VisitorId,
                VisitorName = s.VisitorName,
                PersonType = s.PersonType,
                AreaName = s.AreaName,
                BuildingName = s.BuildingName,
                FloorName = s.FloorName,
                FloorplanName = s.FloorplanName,
                EnterTime = s.EnterTime,
                ExitTime = s.ExitTime,
                DurationInMinutes = s.DurationInMinutes,
                
                // Format khusus untuk export
                EnterTimeFormatted = s.EnterTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ExitTimeFormatted = s.ExitTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Still Active",
                DurationFormatted = s.DurationInMinutes.HasValue ? 
                    $"{s.DurationInMinutes.Value} minutes" : "N/A",
                Status = s.ExitTime.HasValue ? "Completed" : "Active"
            }).ToList();
        }

        private VisitorSessionSummaryRM MapToSession(SessionRaw rec, DateTime enterWib)
        {
            return new VisitorSessionSummaryRM
            {
                PersonId = rec.PersonId,
                PersonName = rec.PersonName,
                PersonType = rec.PersonType,
                VisitorId = rec.VisitorId,
                VisitorName = rec.VisitorName,
                IdentityId = rec.IdentityId,
                MemberId = rec.MemberId,
                MemberName = rec.MemberName,
                CardId = rec.CardId,
                CardName = rec.CardName,
                CardNumber = rec.CardNumber,
                AreaId = rec.AreaId,
                AreaName = rec.AreaName,
                BuildingId = rec.BuildingId,
                BuildingName = rec.BuildingName,
                FloorId = rec.FloorId,
                FloorName = rec.FloorName,
                FloorplanId = rec.FloorplanId,
                FloorplanImage = rec.FloorplanImage,
                FloorplanName = rec.FloorplanName,
                EnterTime = enterWib,
            };
        }
                
            private bool TableExists(string tableName)
            {
                var connString = _context.Database.GetConnectionString();

                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connString);
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = 'dbo'
                    AND TABLE_NAME = @name
                ";

                cmd.Parameters.AddWithValue("@name", tableName);

                var result = (int)cmd.ExecuteScalar();
                return result > 0;
            }



        }
    }