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
    using Shared.Contracts;
    using Shared.Contracts.Analytics;

namespace Repositories.Repository.Analytics
    {
        public class TrackingSessionRepository : BaseRepository
        {
            private readonly BleTrackingDbContext _context;
            private readonly ILogger<TrackingSessionRepository> _logger;
            private static readonly TimeZoneInfo WibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            public TrackingSessionRepository(BleTrackingDbContext context,
            IHttpContextAccessor accessor,
            ILogger<TrackingSessionRepository> logger)
                : base(context, accessor)
            {
                _context = context;
                _logger = logger;
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

            // Di dalam class TrackingSessionRepository

            // === DTO SEMENTARA (internal) ===
            public class VisualPathRaw
            {
                public Guid FloorplanId { get; set; }
                public string? FloorplanName { get; set; }
                public string? FloorplanImage { get; set; }
                public float? CoordinateX { get; set; }
                public float? CoordinateY { get; set; }
                public DateTime TransTimeUtc { get; set; }
                public string? AreaName { get; set; }
                public Guid? PersonId { get; set; }
                public string? PersonName { get; set; }
            }

            // === DTO SEMENTARA (internal) ===

            /// <summary>
            /// Optimized DTO for alarm trigger data
            /// Only loads the fields needed for incident matching
            /// </summary>
            internal class AlarmTriggerDto
            {
                public Guid Id { get; set; }
                public DateTime? TriggerTime { get; set; }
                public DateTime? AcknowledgedAt { get; set; }
                public DateTime? DispatchedAt { get; set; }
                public DateTime? ArrivedAt { get; set; }
                public DateTime? InvestigatedDoneAt { get; set; }
                public DateTime? DoneTimestamp { get; set; }
                public DateTime? CancelTimestamp { get; set; }
                public string? AcknowledgedBy { get; set; }
                public string? DispatchedBy { get; set; }
                public string? AcceptedBy { get; set; }
                public string? ArrivedBy { get; set; }
                public string? InvestigatedDoneBy { get; set; }
                public string? DoneBy { get; set; }
                public string? CancelBy { get; set; }
                public string? WaitingBy { get; set; }
                public DateTime? WaitingTimestamp { get; set; }
                public string? AlarmColor { get; set; }
                public Shared.Contracts.AlarmRecordStatus? Alarm { get; set; }
                public Shared.Contracts.ActionStatus? Action { get; set; }
                public bool? IsActive { get; set; }
                public Guid? SecurityId { get; set; }
                public string? SecurityName { get; set; }
                public string? InvestigatedResult { get; set; }

                /// <summary>
                /// FloorplanMaskedArea IDs from the floorplan
                /// Pre-fetched to avoid N+1 query
                /// </summary>
                public HashSet<Guid> FloorplanMaskedAreaIds { get; set; } = new();
            }

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
                TrackingAnalyticsFilter request, int startParamIndex)
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

                // NEW: Type filter (takes precedence over PersonType)
                var type = (request.Type ?? "visitor").Trim().ToLowerInvariant();
                if (type == "visitor")
                {
                    sql += $" AND v.id IS NOT NULL AND m.id IS NULL";
                }
                else if (type == "member")
                {
                    sql += $" AND m.id IS NOT NULL AND v.id IS NULL";
                }
                else if (type == "security")
                {
                    // Filter by MstSecurity table
                    sql += $" AND EXISTS (SELECT 1 FROM mst_security s WHERE s.id = c.security_id)";
                }
                // Legacy: PersonType filter (for backward compatibility)
                else if (!string.IsNullOrWhiteSpace(request.PersonType))
                {
                    var personType = request.PersonType.Trim().ToLowerInvariant();
                    if (personType == "visitor")
                    {
                        sql += $" AND v.id IS NOT NULL";
                    }
                    else if (personType == "member")
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

            // === BUILD SQL UNTUK VISUAL PATHS ===
            private (string sql, List<object> parameters) BuildVisualPathsSql(
                string tableName, DateTime fromUtc, DateTime toUtc,
                TrackingAnalyticsFilter request, int startParamIndex)
            {
                var sql = $@"
            SELECT
                fp.id AS FloorplanId,
                fp.name AS FloorplanName,
                fp.floorplan_image AS FloorplanImage,
                t.coordinate_x AS CoordinateX,
                t.coordinate_y AS CoordinateY,
                t.trans_time AS TransTimeUtc,
                ma.name AS AreaName,
                COALESCE(v.id, m.id) AS PersonId,
                COALESCE(v.name, m.name) AS PersonName
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
            AND t.coordinate_x IS NOT NULL
            AND t.coordinate_y IS NOT NULL
            AND fp.id IS NOT NULL
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
           public async Task<(List<VisitorSessionRead> Data, int Total, int Filtered)> GetVisitorSessionSummaryAsync(
            TrackingAnalyticsFilter request)
        {
            var range = GetTimeRange(request.TimeRange);
            var fromUtc = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-1);
            var toUtc   = range?.to   ?? request.To   ?? DateTime.UtcNow;

            var tableNames = GetTableNamesInRange(fromUtc, toUtc);
            if (!tableNames.Any())
                return (new List<VisitorSessionRead>(), 0, 0);

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

            var filtered = raw.Count;

            if (!raw.Any())
                return (new List<VisitorSessionRead>(), 0, 0);

            var sessions = new List<VisitorSessionRead>();

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

                var session = MapToSession(first);

                // === HANYA 1 HIT → SESSION MASIH AKTIF ===
                if (records.Count == 1)
                {
                    session.ExitTime  = null;
                    session.DurationInMinutes = null;
                    session.DurationFormatted = null;
                    session.SessionStatus = "active";
                }
                else
                {
                    // === SESSION SELESAI ===
                    session.ExitTime = last.TransTimeUtc;
                    session.DurationInMinutes =
                        (int)(session.ExitTime.Value - session.EnterTime).TotalMinutes;
                    session.DurationFormatted = FormatDuration(session.DurationInMinutes.Value);
                    session.SessionStatus = "completed";

                    // jangan tampilkan session 0 menit
                    if (session.DurationInMinutes < 1)
                        continue;
                }

                sessions.Add(session);
            }

            // === LOAD INCIDENT DATA (independent of HasIncident filter) ===
            if (request.IncludeIncident || request.HasIncident.HasValue)
            {
                var alarmTriggers = await GetAlarmTriggersInRangeAsync(fromUtc, toUtc, request);

                foreach (var session in sessions)
                {
                    var hasIncident = CheckSessionHasIncident(session, alarmTriggers);
                    session.HasIncident = hasIncident;

                    // Include full incident data if requested
                    if (hasIncident && request.IncludeIncident)
                    {
                        session.Incident = GetIncidentForSession(session, alarmTriggers);
                    }
                }
            }

            // === FILTER: HasIncident (separate from loading) ===
            if (request.HasIncident.HasValue)
            {
                if (request.HasIncident.Value)
                {
                    sessions = sessions.Where(s => s.HasIncident).ToList();
                }
                else
                {
                    sessions = sessions.Where(s => !s.HasIncident).ToList();
                }
            }

            // Apply sorting
            sessions = request.SortDir?.ToLower() == "asc"
                ? sessions.OrderBy(x => x.EnterTime).ToList()
                : sessions.OrderByDescending(x => x.EnterTime).ToList();
            return (sessions, raw.Count, raw.Count);
        }

        private async Task<List<AlarmTriggerDto>> GetAlarmTriggersInRangeAsync(
            DateTime fromUtc,
            DateTime toUtc,
            TrackingAnalyticsFilter request)
        {
            var query = _context.AlarmTriggers
                .Where(at => at.TriggerTime >= fromUtc && at.TriggerTime <= toUtc);

            var type = (request.Type ?? "visitor").ToLowerInvariant();
            if (type == "visitor" && request.VisitorId.HasValue)
            {
                query = query.Where(at => at.VisitorId == request.VisitorId);
            }
            else if (type == "member" && request.MemberId.HasValue)
            {
                query = query.Where(at => at.MemberId == request.MemberId);
            }

            var alarmData = await query
                .Select(at => new
                {
                    at.Id,
                    at.TriggerTime,
                    at.AcknowledgedAt,
                    at.DispatchedAt,
                    at.ArrivedAt,
                    at.InvestigatedDoneAt,
                    at.DoneTimestamp,
                    at.CancelTimestamp,
                    at.AcknowledgedBy,
                    at.DispatchedBy,
                    at.ArrivedBy,
                    at.InvestigatedDoneBy,
                    at.DoneBy,
                    at.CancelBy,
                    at.WaitingBy,
                    at.WaitingTimestamp,
                    at.AlarmColor,
                    at.Alarm,
                    at.Action,
                    at.IsActive,
                    at.SecurityId,
                    SecurityName = at.Security != null ? at.Security.Name : null,
                    at.InvestigatedResult,
                    FloorplanId = at.Floorplan != null ? at.Floorplan.Id : (Guid?)null
                })
                .ToListAsync();

            // Collect all FloorplanIds that are not null
            var floorplanIds = alarmData
                .Where(x => x.FloorplanId.HasValue)
                .Select(x => x.FloorplanId.Value)
                .Distinct()
                .ToList();

            // Fetch FloorplanMaskedAreaIds in a single query (avoid N+1)
            var floorplanMaskedAreaIds = new Dictionary<Guid, HashSet<Guid>>();
            if (floorplanIds.Any())
            {
                var areas = await _context.FloorplanMaskedAreas
                    .Where(fma => fma.Status != 0 && floorplanIds.Contains(fma.FloorplanId))
                    .Select(fma => new { fma.FloorplanId, fma.Id })
                    .ToListAsync();

                floorplanMaskedAreaIds = areas
                    .GroupBy(x => x.FloorplanId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.Id).ToHashSet()
                    );
            }

            // Build DTOs with pre-fetched FloorplanMaskedAreaIds
            return alarmData.Select(ad => new AlarmTriggerDto
            {
                Id = ad.Id,
                TriggerTime = ad.TriggerTime,
                AcknowledgedAt = ad.AcknowledgedAt,
                DispatchedAt = ad.DispatchedAt,
                ArrivedAt = ad.ArrivedAt,
                InvestigatedDoneAt = ad.InvestigatedDoneAt,
                DoneTimestamp = ad.DoneTimestamp,
                CancelTimestamp = ad.CancelTimestamp,
                AcknowledgedBy = ad.AcknowledgedBy,
                DispatchedBy = ad.DispatchedBy,
                ArrivedBy = ad.ArrivedBy,
                InvestigatedDoneBy = ad.InvestigatedDoneBy,
                DoneBy = ad.DoneBy,
                CancelBy = ad.CancelBy,
                WaitingBy = ad.WaitingBy,
                WaitingTimestamp = ad.WaitingTimestamp,
                AlarmColor = ad.AlarmColor,
                Alarm = ad.Alarm,
                Action = ad.Action,
                IsActive = ad.IsActive,
                SecurityId = ad.SecurityId,
                SecurityName = ad.SecurityName,
                InvestigatedResult = ad.InvestigatedResult.HasValue ? ad.InvestigatedResult.ToString() : null,
                FloorplanMaskedAreaIds = ad.FloorplanId.HasValue && floorplanMaskedAreaIds.ContainsKey(ad.FloorplanId.Value)
                    ? floorplanMaskedAreaIds[ad.FloorplanId.Value]
                    : new HashSet<Guid>()
            }).ToList();
        }

        /// <summary>
        /// Check if session has an incident
        /// </summary>
        private bool CheckSessionHasIncident(
            VisitorSessionRead session,
            List<AlarmTriggerDto> alarmTriggers)
        {
            if (!session.AreaId.HasValue)
                return false;

            // Check if any alarm matches this session
            return alarmTriggers
                .Where(at => at.FloorplanMaskedAreaIds.Contains(session.AreaId.Value) &&
                           at.TriggerTime.HasValue)
                .Any(at =>
                    Math.Abs((at.TriggerTime.Value - session.EnterTime).TotalSeconds) < 300); // Within 5 minutes
        }

        /// <summary>
        /// Get incident details for a session
        /// </summary>
        private IncidentMarkerRead GetIncidentForSession(
            VisitorSessionRead session,
            List<AlarmTriggerDto> alarmTriggers)
        {
            if (!session.AreaId.HasValue)
                return null;

            // Find matching alarm
            var alarm = alarmTriggers
                .Where(at => at.FloorplanMaskedAreaIds.Contains(session.AreaId.Value) &&
                           at.TriggerTime.HasValue)
                .FirstOrDefault(at =>
                    Math.Abs((at.TriggerTime.Value - session.EnterTime).TotalSeconds) < 300);

            if (alarm == null)
                return null;

            // Calculate metrics
            var responseTimeSeconds = alarm.AcknowledgedAt.HasValue
                ? (int)(alarm.AcknowledgedAt.Value - alarm.TriggerTime.Value).TotalSeconds
                : 0;

            var resolutionTimeSeconds = alarm.DoneTimestamp.HasValue
                ? (int)(alarm.DoneTimestamp.Value - alarm.TriggerTime.Value).TotalSeconds
                : 0;

            return new IncidentMarkerRead
            {
                AlarmTriggerId = alarm.Id,
                AlarmColor = alarm.AlarmColor,
                AlarmStatus = alarm.Alarm?.ToString(),
                ActionStatus = alarm.Action?.ToString(),
                IsActive = alarm.IsActive ?? false,

                TriggerTime = alarm.TriggerTime ?? DateTime.UtcNow,
                AcknowledgedAt = alarm.AcknowledgedAt,
                DispatchedAt = alarm.DispatchedAt,
                ArrivedAt = alarm.ArrivedAt,
                InvestigatedDoneAt = alarm.InvestigatedDoneAt,
                DoneAt = alarm.DoneTimestamp,

                AcknowledgedBy = alarm.AcknowledgedBy,
                DispatchedBy = alarm.DispatchedBy,
                AcceptedBy = alarm.AcceptedBy,
                ArrivedBy = alarm.ArrivedBy,
                InvestigatedDoneBy = alarm.InvestigatedDoneBy,
                DoneBy = alarm.DoneBy,

                SecurityId = alarm.SecurityId,
                SecurityName = alarm.SecurityName,

                InvestigationResult = alarm.InvestigatedResult,

                ResponseTimeSeconds = responseTimeSeconds,
                ResponseTimeFormatted = FormatDuration(responseTimeSeconds),
                ResolutionTimeSeconds = resolutionTimeSeconds,
                ResolutionTimeFormatted = FormatDuration(resolutionTimeSeconds),

                TimelineSummary = BuildTimelineSummary(alarm)
            };
        }

        /// <summary>
        /// Build timeline summary for incident
        /// </summary>
        private string BuildTimelineSummary(AlarmTriggerDto alarm)
        {
            var timeline = new List<string>();

            if (alarm.AcknowledgedAt.HasValue)
                timeline.Add($"ACK by {alarm.AcknowledgedBy}");

            if (alarm.DispatchedAt.HasValue)
                timeline.Add($"DISPATCHED by {alarm.DispatchedBy}");

            if (alarm.ArrivedAt.HasValue)
                timeline.Add($"ARRIVED by {alarm.ArrivedBy}");

            if (alarm.WaitingTimestamp.HasValue)
                timeline.Add($"WAITING by {alarm.WaitingBy}");

            if (alarm.InvestigatedDoneAt.HasValue)
                timeline.Add($"INVESTIGATED DONE by {alarm.InvestigatedDoneBy}");

            if (alarm.DoneTimestamp.HasValue)
                timeline.Add($"DONE by {alarm.DoneBy}");

            if (alarm.CancelTimestamp.HasValue)
                timeline.Add($"CANCELLED by {alarm.CancelBy}");

            return timeline.Count > 0 ? string.Join(" → ", timeline) : "No actions taken";
        }

        /// <summary>
        /// Format duration in human-readable string
        /// </summary>
        private string FormatDuration(int minutes)
        {
            if (minutes < 60)
                return $"{minutes} min";

            var hours = minutes / 60;
            var mins = minutes % 60;

            if (mins == 0)
                return $"{hours} hour{(hours > 1 ? "s" : "")}";

            return $"{hours} hour{(hours > 1 ? "s" : "")} {mins} min";
        }

        // === GET VISUAL PATHS DATA FOR FLOORPLAN VISUALIZATION ===
        public async Task<List<VisualPathRaw>> GetVisualPathsDataAsync(
            TrackingAnalyticsFilter request)
        {
            var range = GetTimeRange(request.TimeRange);
            var fromUtc = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-1);
            var toUtc = range?.to ?? request.To ?? DateTime.UtcNow;

            var tableNames = GetTableNamesInRange(fromUtc, toUtc);
            if (!tableNames.Any())
                return new List<VisualPathRaw>();

            var unionParts = new List<string>();
            var allParams = new List<object>();
            int paramIndex = 0;

            foreach (var table in tableNames)
            {
                var (sql, parameters) =
                    BuildVisualPathsSql(table, fromUtc, toUtc, request, paramIndex);

                unionParts.Add(sql);
                allParams.AddRange(parameters);
                paramIndex += parameters.Count;
            }

            var fullSql = string.Join("\nUNION ALL\n", unionParts);

            var visualPaths = await _context.Database
                .SqlQueryRaw<VisualPathRaw>(fullSql, allParams.ToArray())
                .ToListAsync();

            // Apply sampling if MaxPointsPerFloorplan is set
            if (request.MaxPointsPerFloorplan.HasValue && request.MaxPointsPerFloorplan.Value > 0)
            {
                visualPaths = ApplyMaxPointsSampling(visualPaths, request.MaxPointsPerFloorplan.Value);
            }

            return visualPaths;
        }

        /// <summary>
        /// Apply sampling to limit points per floorplan
        /// Distributes sampled points evenly across time range
        /// </summary>
        private List<VisualPathRaw> ApplyMaxPointsSampling(
            List<VisualPathRaw> visualPaths,
            int maxPointsPerFloorplan)
        {
            // Group by floorplan
            var groupedByFloorplan = visualPaths
                .GroupBy(p => new { p.FloorplanId, p.FloorplanName, p.FloorplanImage })
                .ToList();

            var result = new List<VisualPathRaw>();

            foreach (var group in groupedByFloorplan)
            {
                var points = group.ToList();

                // If within limit, keep all
                if (points.Count <= maxPointsPerFloorplan)
                {
                    result.AddRange(points);
                    continue;
                }

                // Apply sampling: take N points evenly distributed
                var sampleRate = (double)points.Count / maxPointsPerFloorplan;
                var sampledPoints = new List<VisualPathRaw>();

                for (int i = 0; i < maxPointsPerFloorplan; i++)
                {
                    // Calculate index to get evenly distributed points
                    var targetIndex = (int)Math.Floor(i * sampleRate);
                    sampledPoints.Add(points[targetIndex]);
                }

                result.AddRange(sampledPoints);
            }

            return result;
        }

        // === GET PEAK HOURS BY AREA ===
        public async Task<List<PeakHoursRawRead>> GetPeakHoursByAreaAsync(
            TrackingAnalyticsFilter request,
            PeakHoursGroupByMode groupByMode = PeakHoursGroupByMode.Area)
        {
            var range = GetTimeRange(request.TimeRange);
            var fromUtc = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-1);
            var toUtc = range?.to ?? request.To ?? DateTime.UtcNow;

            var tableNames = GetTableNamesInRange(fromUtc, toUtc);
            if (!tableNames.Any())
                return new List<PeakHoursRawRead>();

            var unionParts = new List<string>();
            var allParams = new List<object>();
            int paramIndex = 0;

            foreach (var table in tableNames)
            {
                var (sql, parameters) = BuildPeakHoursSql(table, fromUtc, toUtc, request, groupByMode, paramIndex);
                unionParts.Add(sql);
                allParams.AddRange(parameters);
                paramIndex += parameters.Count;
            }

            var fullSql = string.Join("\nUNION ALL\n", unionParts);

            var result = await _context.Database
                .SqlQueryRaw<PeakHoursRawRead>(fullSql, allParams.ToArray())
                .ToListAsync();

            return result;
        }

        // === BUILD SQL FOR PEAK HOURS BY AREA ===
        private (string sql, List<object> parameters) BuildPeakHoursSql(
            string tableName, DateTime fromUtc, DateTime toUtc,
            TrackingAnalyticsFilter request, PeakHoursGroupByMode groupByMode, int startParamIndex)
        {
            // Determine GROUP BY column based on mode
            string groupByColumn = groupByMode switch
            {
                PeakHoursGroupByMode.Building => "b.name",
                PeakHoursGroupByMode.Floor => "fl.name",
                PeakHoursGroupByMode.Floorplan => "fp.name",
                _ => "ma.name"  // Default: Area
            };

            // SQL to count visitors per hour per area/building/floor/floorplan
            // Uses DATEADD(HOUR, DATEDIFF(HOUR, 0, t.trans_time), 0) to group by hour
            // Adjusts for WIB timezone (UTC+7) by adding 7 hours before extracting hour
            var sql = $@"
            SELECT
                {groupByColumn} AS Name,
                (DATEPART(HOUR, DATEADD(HOUR, 7, t.trans_time))) AS Hour,
                COUNT(DISTINCT COALESCE(v.id, m.id)) AS Count
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
            AND ma.id IS NOT NULL
            AND (v.id IS NOT NULL OR m.id IS NOT NULL)
        ";

            var parameters = new List<object> { fromUtc, toUtc };
            int paramIndex = startParamIndex + 2;

            // Apply building filter dari token untuk operator
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any() && !request.BuildingId.HasValue)
            {
                // Hanya apply jika tidak ada manual BuildingId di request
                // SQL Server requires IN clause with individual parameters
                var buildingParams = accessibleBuildingIds.Select((id, i) =>
                {
                    var paramName = $"@p{paramIndex++}";
                    parameters.Add(id);
                    return paramName;
                }).ToArray();
                sql += $" AND fl.building_id IN ({string.Join(", ", buildingParams)})";
            }

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

            sql += $"\nGROUP BY {groupByColumn}, DATEPART(HOUR, DATEADD(HOUR, 7, t.trans_time))";

            return (sql, parameters);
        }

        public async Task<List<VisitorSessionSummaryExportRM>> GetVisitorSessionSummaryExportAsync(TrackingAnalyticsFilter request)
        {
            // Repository returns all sessions (no pagination applied)
            var (sessions, _, _) = await GetVisitorSessionSummaryAsync(request);
            
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
                EnterTime  = s.EnterTime,
                ExitTime  = s.ExitTime,
                DurationInMinutes = s.DurationInMinutes,
                
                // Format khusus untuk export
                EnterTimeFormatted = s.EnterTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ExitTimeFormatted = s.ExitTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Still Active",
                DurationFormatted = s.DurationInMinutes.HasValue ? 
                    $"{s.DurationInMinutes.Value} minutes" : "N/A",
                Status = s.ExitTime.HasValue ? "Completed" : "Active"
            }).ToList();
        }

        private VisitorSessionRead MapToSession(SessionRaw rec)
        {
            return new VisitorSessionRead
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
                EnterTime = rec.TransTimeUtc
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