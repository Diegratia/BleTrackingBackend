using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository;
using Shared.Contracts.Analytics;
using Helpers.Consumer;
using Microsoft.Extensions.Logging;

namespace Repositories.Repository.Analytics
{
    /// <summary>
    /// Repository for User Journey Analytics
    /// Provides methods for common paths analysis, security checks, and next area prediction
    /// </summary>
    public class UserJourneyRepository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserJourneyRepository> _logger;
        private static readonly TimeZoneInfo WibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public UserJourneyRepository(
            BleTrackingDbContext context,
            IHttpContextAccessor accessor,
            ILogger<UserJourneyRepository> logger)
            : base(context, accessor)
        {
            _context = context;
            _httpContextAccessor = accessor;
            _logger = logger;
        }

        /// <summary>
        /// Get common paths analysis - most popular journey sequences
        /// </summary>
        public async Task<CommonPathsResponse> GetCommonPathsAsync(UserJourneyFilter filter)
        {
            var fromUtc = filter.From ?? DateTime.UtcNow.AddDays(-7);
            var toUtc = filter.To ?? DateTime.UtcNow;

            _logger.LogInformation("Getting common paths from {From} to {To}", fromUtc, toUtc);

            // Get all visitor sessions in date range
            var sessions = await GetSessionsInRangeAsync(fromUtc, toUtc, filter);

            if (!sessions.Any())
            {
                return new CommonPathsResponse
                {
                    Data = new List<CommonPathRead>(),
                    TotalJourneys = 0,
                    DateRange = $"{fromUtc:yyyy-MM-dd} to {toUtc:yyyy-MM-dd}"
                };
            }

            // Group by person and create journey paths
            var journeys = sessions
                .GroupBy(s => new { PersonId = s.VisitorId ?? s.MemberId, s.PersonType })
                .Select(g => new
                {
                    PersonId = g.Key.PersonId,
                    PersonType = g.Key.PersonType,
                    Sessions = g.OrderBy(s => s.EnterTime).ToList()
                })
                .Where(j => j.Sessions.Count >= filter.MinJourneyLength)
                .Select(j => new
                {
                    PathSequence = string.Join("→", j.Sessions.Select(s => s.AreaName?.Trim())),
                    AreaSequence = j.Sessions.Select(s => s.AreaName?.Trim()).Where(n => !string.IsNullOrEmpty(n)).ToList(),
                    TotalDurationMinutes = j.Sessions.Sum(s => s.DurationInMinutes ?? 0),
                    AreaCount = j.Sessions.Count
                })
                .ToList();

            var totalJourneys = journeys.Count;

            // Group by path sequence and aggregate
            var commonPaths = journeys
                .GroupBy(j => j.PathSequence)
                .Select(g => new CommonPathRead
                {
                    PathId = g.Key.ToLower().Replace(" ", "-").Replace("→", "-"),
                    AreaSequence = g.First().AreaSequence,
                    JourneyCount = g.Count(),
                    Percentage = totalJourneys > 0 ? (g.Count() * 100.0 / totalJourneys) : 0,
                    AvgDurationMinutes = g.Average(j => j.TotalDurationMinutes),
                    IsAnomaly = g.Count() < 5,
                    RiskLevel = CalculateRiskLevel(g.First().AreaSequence, g.Count())
                })
                .OrderByDescending(p => p.JourneyCount)
                .Take(filter.MaxResults)
                .ToList();

            return new CommonPathsResponse
            {
                Data = commonPaths,
                TotalJourneys = totalJourneys,
                DateRange = $"{fromUtc:yyyy-MM-dd} to {toUtc:yyyy-MM-dd}"
            };
        }

        /// <summary>
        /// Perform security journey check for a specific visitor/member
        /// </summary>
        public async Task<SecurityJourneyCheckRead> GetSecurityCheckAsync(
            Guid? visitorId,
            Guid? memberId,
            DateTime from,
            DateTime to)
        {
            var filter = new UserJourneyFilter
            {
                VisitorId = visitorId,
                MemberId = memberId,
                From = from,
                To = to
            };

            var sessions = await GetSessionsInRangeAsync(from, to, filter);

            if (!sessions.Any())
            {
                return new SecurityJourneyCheckRead
                {
                    VisitorId = visitorId,
                    MemberId = memberId,
                    RiskLevel = "Low",
                    RequiresEscort = false,
                    Violations = new List<SecurityViolation>()
                };
            }

            // Order sessions by enter time
            var orderedSessions = sessions.OrderBy(s => s.EnterTime).ToList();

            // Get security zone mappings
            var areaIds = orderedSessions.Select(s => s.AreaId).Where(id => id.HasValue).Distinct().ToList();
            var zoneMappings = await _context.SecurityZoneMappings
                .Where(z => areaIds.Contains(z.AreaId) && z.Status != 0)
                .ToListAsync();

            var zoneDict = zoneMappings.ToDictionary(z => z.AreaId);

            // Build path taken
            var pathTaken = orderedSessions
                .Select(s => s.AreaName?.Trim())
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList()!;

            // Check for violations
            var violations = new List<SecurityViolation>();
            bool requiresEscort = false;
            string overallRiskLevel = "Low";

            for (int i = 0; i < orderedSessions.Count; i++)
            {
                var currentSession = orderedSessions[i];
                var currentAreaId = currentSession.AreaId;

                if (!currentAreaId.HasValue || !zoneDict.ContainsKey(currentAreaId.Value))
                {
                    continue; // No zone mapping, skip
                }

                var currentZone = zoneDict[currentAreaId.Value];

                // Check if zone requires escort
                if (currentZone.RequiresEscort)
                {
                    requiresEscort = true;
                }

                // Check transition from previous zone
                if (i > 0)
                {
                    var prevSession = orderedSessions[i - 1];
                    var prevAreaId = prevSession.AreaId;

                    if (prevAreaId.HasValue && zoneDict.ContainsKey(prevAreaId.Value))
                    {
                        var prevZone = zoneDict[prevAreaId.Value];

                        // Check if transition is allowed
                        if (!IsTransitionAllowed(prevZone.SecurityZone, currentZone.SecurityZone, currentZone))
                        {
                            violations.Add(new SecurityViolation
                            {
                                Type = "Unauthorized Zone Access",
                                AreaName = currentSession.AreaName,
                                Description = $"Transition from {prevZone.SecurityZone.GetDisplayName()} to {currentZone.SecurityZone.GetDisplayName()} is not allowed",
                                ZoneType = currentZone.SecurityZone.GetDisplayName()
                            });

                            overallRiskLevel = "High";
                        }
                        else if (currentZone.RequiresEscort)
                        {
                            violations.Add(new SecurityViolation
                            {
                                Type = "Escort Required",
                                AreaName = currentSession.AreaName,
                                Description = $"{currentSession.AreaName} requires escort for access",
                                ZoneType = currentZone.SecurityZone.GetDisplayName()
                            });

                            overallRiskLevel = overallRiskLevel == "High" ? "High" : "Medium";
                        }
                    }
                }
                else if (currentZone.SecurityZone == SecurityZone.Restricted || currentZone.SecurityZone == SecurityZone.Critical)
                {
                    // First area is restricted/critical - violation
                    violations.Add(new SecurityViolation
                    {
                        Type = "Unauthorized Direct Access",
                        AreaName = currentSession.AreaName,
                        Description = $"Direct access to {currentZone.SecurityZone.GetDisplayName()} zone without proper entry",
                        ZoneType = currentZone.SecurityZone.GetDisplayName()
                    });

                    overallRiskLevel = "Critical";
                }
            }

            var firstSession = orderedSessions.First();
            var totalDuration = orderedSessions.Sum(s => s.DurationInMinutes ?? 0);

            return new SecurityJourneyCheckRead
            {
                VisitorId = firstSession.VisitorId,
                VisitorName = firstSession.VisitorName,
                MemberId = firstSession.MemberId,
                MemberName = firstSession.MemberName,
                PathTaken = pathTaken,
                RiskLevel = overallRiskLevel,
                RequiresEscort = requiresEscort || violations.Any(v => v.Type == "Escort Required"),
                Violations = violations,
                TotalDurationMinutes = totalDuration
            };
        }

        #region Private Helper Methods

        /// <summary>
        /// Get visitor sessions within date range
        /// Reuses TrackingSessionRepository logic
        /// </summary>
        private async Task<List<VisitorSessionRead>> GetSessionsInRangeAsync(
            DateTime fromUtc,
            DateTime toUtc,
            UserJourneyFilter filter)
        {
            var tableNames = GetTableNamesInRange(fromUtc, toUtc);
            if (!tableNames.Any())
                return new List<VisitorSessionRead>();

            var unionParts = new List<string>();
            var allParams = new List<object>();
            int paramIndex = 0;

            foreach (var table in tableNames)
            {
                var (sql, parameters) = BuildSessionSql(
                    table, fromUtc, toUtc, filter, paramIndex);

                unionParts.Add(sql);
                allParams.AddRange(parameters);
                paramIndex += parameters.Count;
            }

            var fullSql = string.Join("\nUNION ALL\n", unionParts);

            var raw = await _context.Database
                .SqlQueryRaw<SessionRaw>(fullSql, allParams.ToArray())
                .ToListAsync();

            if (!raw.Any())
                return new List<VisitorSessionRead>();

            // Group by person + area to create sessions
            var sessions = new List<VisitorSessionRead>();
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
                var last = records.Last();

                var session = MapToSession(first);

                if (records.Count == 1)
                {
                    session.ExitTime = null;
                    session.DurationInMinutes = null;
                    sessions.Add(session);
                    continue;
                }

                session.ExitTime = last.TransTimeUtc;
                session.DurationInMinutes = (int)(session.ExitTime.Value - session.EnterTime).TotalMinutes;

                if (session.DurationInMinutes >= 1)
                    sessions.Add(session);
            }

            return sessions.OrderByDescending(x => x.EnterTime).ToList();
        }

        /// <summary>
        /// Get tracking transaction table names in date range
        /// </summary>
        private List<string> GetTableNamesInRange(DateTime fromUtc, DateTime toUtc)
        {
            var fromWib = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, WibZone).Date;
            var toWib = TimeZoneInfo.ConvertTimeFromUtc(toUtc, WibZone).Date;

            var tables = new List<string>();
            for (var d = fromWib; d <= toWib; d = d.AddDays(1))
            {
                var table = $"tracking_transaction_{d:yyyyMMdd}";
                if (TableExists(table))
                    tables.Add(table);
            }
            return tables;
        }

        /// <summary>
        /// Check if table exists in database
        /// </summary>
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

        /// <summary>
        /// Build SQL for session queries
        /// </summary>
        private (string sql, List<object> parameters) BuildSessionSql(
            string tableName, DateTime fromUtc, DateTime toUtc,
            UserJourneyFilter filter, int startParamIndex)
        {
            var sql = $@"
                SELECT
                    v.id AS VisitorId,
                    v.name AS VisitorName,
                    m.id AS MemberId,
                    m.name AS MemberName,
                    COALESCE(v.id, m.id) AS PersonId,
                    CASE WHEN v.id IS NOT NULL THEN 'Visitor' WHEN m.id IS NOT NULL THEN 'Member' ELSE 'Unknown' END AS PersonType,
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

            if (filter.BuildingId.HasValue)
            {
                sql += $" AND b.id = @p{paramIndex++}";
                parameters.Add(filter.BuildingId.Value);
            }
            if (filter.FloorId.HasValue)
            {
                sql += $" AND fl.id = @p{paramIndex++}";
                parameters.Add(filter.FloorId.Value);
            }
            if (filter.AreaId.HasValue)
            {
                sql += $" AND ma.id = @p{paramIndex++}";
                parameters.Add(filter.AreaId.Value);
            }
            if (filter.VisitorId.HasValue)
            {
                sql += $" AND v.id = @p{paramIndex++}";
                parameters.Add(filter.VisitorId.Value);
            }
            if (filter.MemberId.HasValue)
            {
                sql += $" AND m.id = @p{paramIndex++}";
                parameters.Add(filter.MemberId.Value);
            }

            return (sql, parameters);
        }

        /// <summary>
        /// Map SessionRaw to VisitorSessionRead
        /// </summary>
        private VisitorSessionRead MapToSession(SessionRaw rec)
        {
            return new VisitorSessionRead
            {
                PersonId = rec.PersonId,
                PersonName = rec.VisitorName ?? rec.MemberName,
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

        /// <summary>
        /// Check if transition between zones is allowed
        /// </summary>
        private bool IsTransitionAllowed(SecurityZone fromZone, SecurityZone toZone, SecurityZoneMapping toZoneMapping)
        {
            // If specific restrictions are defined, check them
            if (!string.IsNullOrEmpty(toZoneMapping.AllowedFromZones))
            {
                var allowedZones = toZoneMapping.AllowedFromZones.Split(',')
                    .Where(s => int.TryParse(s.Trim(), out _))
                    .Select(int.Parse)
                    .ToHashSet();

                return allowedZones.Contains((int)fromZone);
            }

            // Default behavior: use extension method
            return SecurityZoneExtensions.IsTransitionAllowed(fromZone, toZone);
        }

        /// <summary>
        /// Calculate risk level based on area sequence and occurrence count
        /// </summary>
        private string CalculateRiskLevel(List<string> areaSequence, int count)
        {
            if (count < 5)
                return "Medium";

            // Check if sequence contains restricted/critical areas
            // This is a simplified check - in production, you'd query the actual zone mappings
            var hasRestrictedAreas = areaSequence.Any(area =>
                area.ToLower().Contains("server") ||
                area.ToLower().Contains("vault") ||
                area.ToLower().Contains("data center") ||
                area.ToLower().Contains("control room"));

            return hasRestrictedAreas ? "Medium" : "Low";
        }

        #endregion

        #region Internal DTOs

        private class SessionRaw
        {
            public Guid? PersonId { get; set; }
            public string? PersonType { get; set; }
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
        }

        #endregion
    }
}
