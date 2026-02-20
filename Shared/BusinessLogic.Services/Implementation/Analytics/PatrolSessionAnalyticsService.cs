using AutoMapper;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Interface.Analytics;
using BusinessLogic.Services.Implementation;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class PatrolSessionAnalyticsService : BaseService, IPatrolSessionAnalyticsService
    {
        private readonly PatrolSessionRepository _repository;
        private readonly IAuditEmitter _audit;
        private readonly ILogger<PatrolSessionAnalyticsService> _logger;

        public PatrolSessionAnalyticsService(
            PatrolSessionRepository repository,
            IAuditEmitter audit,
            IHttpContextAccessor http,
            ILogger<PatrolSessionAnalyticsService> logger) : base(http)
        {
            _repository = repository;
            _audit = audit;
            _logger = logger;
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        public async Task<object> GetReportAsync(DataTablesProjectedRequest request, PatrolSessionAnalyticsFilter filter)
        {
            // Map DataTables request to filter
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "StartedAt";
            filter.SortDir = request.SortDir ?? "desc";
            filter.Search = request.SearchValue;

            // Handle date filters from request
            if (request.DateFilters != null)
            {
                if (request.DateFilters.TryGetValue("StartedAt", out var dateFilter))
                {
                    filter.DateFrom = dateFilter.DateFrom;
                    filter.DateTo = dateFilter.DateTo;
                }
            }

            // 1. Get raw data from repository
            var (sessions, total, filtered) = await _repository.GetAnalyticsDataAsync(filter);

            // 2. Apply timezone conversion if needed
            var tz = TimezoneHelper.Resolve(filter.Timezone);
            var sessionReads = new List<PatrolSessionAnalyticsRead>();

            foreach (var session in sessions)
            {
                var sessionRead = BuildSessionRead(session, tz, filter);
                sessionReads.Add(sessionRead);
            }

            // 3. Apply sorting (already applied in repository, but sort transformed data if needed)
            if (!string.IsNullOrWhiteSpace(filter.SortColumn))
            {
                // Default sorting by StartedAt descending
                sessionReads = filter.SortDir?.ToLower() == "asc"
                    ? sessionReads.OrderBy(s => s.StartedAt).ToList()
                    : sessionReads.OrderByDescending(s => s.StartedAt).ToList();
            }

            // 4. Apply pagination on transformed data
            var start = (filter.Page - 1) * filter.PageSize;
            var pagedData = sessionReads
                .Skip(start)
                .Take(filter.PageSize)
                .ToList();

            // 5. Build DataTables response
            var response = new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data = pagedData
            };

            return response;
        }

        public async Task<PatrolSessionAnalyticsRead?> GetSessionTimelineAsync(Guid sessionId)
        {
            var session = await _repository.GetSessionForTimelineAsync(sessionId);
            if (session == null)
                return null;

            var tz = TimeZoneInfo.Utc;
            return BuildSessionRead(session, tz, new PatrolSessionAnalyticsFilter
            {
                IncludeTimeline = true,
                IncludeIncidents = true
            });
        }

        public async Task<byte[]> ExportToPdfAsync(PatrolSessionAnalyticsFilter filter)
        {
            // Get ALL data (no pagination) - set PageSize to a large number
            var originalPageSize = filter.PageSize;
            filter.PageSize = int.MaxValue;

            var (sessions, _, _) = await _repository.GetAnalyticsDataAsync(filter);

            filter.PageSize = originalPageSize;

            var tz = TimezoneHelper.Resolve(filter.Timezone);
            var sessionReads = sessions.Select(s => BuildSessionRead(s, tz, filter)).ToList();

            return GeneratePdfReport(sessionReads, filter);
        }

        private PatrolSessionAnalyticsRead BuildSessionRead(
            PatrolSession session,
            TimeZoneInfo tz,
            PatrolSessionAnalyticsFilter filter)
        {
            var read = new PatrolSessionAnalyticsRead
            {
                SessionId = session.Id,
                StartedAt = TimezoneHelper.ConvertFromUtc(session.StartedAt, tz),
                EndedAt = session.EndedAt.HasValue
                    ? TimezoneHelper.ConvertFromUtc(session.EndedAt.Value, tz)
                    : null,
                SecurityId = session.SecurityId,
                SecurityName = session.Security?.Name ?? session.SecurityNameSnap ?? "Unknown",
                SecurityEmployeeNumber = session.Security?.IdentityId ?? session.SecurityIdentityIdSnap,
                RouteId = session.PatrolRouteId,
                RouteName = session.PatrolRoute?.Name ?? session.PatrolRouteNameSnap ?? "Unknown",
                Metrics = BuildMetrics(session)
            };

            // Calculate duration
            if (session.EndedAt.HasValue)
            {
                var duration = session.EndedAt.Value - session.StartedAt;
                read.DurationFormatted = FormatDuration(duration);
            }

            // Build timeline if requested
            if (filter.IncludeTimeline)
            {
                read.Timeline = BuildTimeline(session, tz);
            }

            // Build incidents if requested
            if (filter.IncludeIncidents)
            {
                read.Incidents = session.PatrolCases?
                    .Where(c => c.Status != 0)
                    .Select(c => new PatrolIncidentSummary
                    {
                        CaseId = c.Id,
                        ReportedAt = TimezoneHelper.ConvertFromUtc(c.CreatedAt, tz),
                        Title = c.Title,
                        CaseType = c.CaseType.ToString(),
                        ThreatLevel = c.ThreatLevel?.ToString() ?? "Unknown",
                        CaseStatus = c.CaseStatus.ToString(),
                        AreaName = null  // TODO: Get area from PatrolCaseArea junction if needed
                    })
                    .ToList() ?? new List<PatrolIncidentSummary>();
            }

            return read;
        }

        private List<PatrolTimelineEvent> BuildTimeline(PatrolSession session, TimeZoneInfo tz)
        {
            var timeline = new List<PatrolTimelineEvent>();

            // 1. Started event
            timeline.Add(new PatrolTimelineEvent
            {
                Stage = "started",
                StageName = "Patrol Started",
                Timestamp = TimezoneHelper.ConvertFromUtc(session.StartedAt, tz)
            });

            // 2. Checkpoint events (ordered by OrderIndex)
            var checkpoints = session.PatrolCheckpointLogs
                .Where(l => l.Status != 0)
                .OrderBy(l => l.OrderIndex)
                .ToList();

            DateTime? previousTimestamp = session.StartedAt;

            foreach (var log in checkpoints)
            {
                if (!log.ArrivedAt.HasValue) continue;

                var duration = previousTimestamp.HasValue
                    ? (int?)(log.ArrivedAt.Value - previousTimestamp.Value).TotalSeconds
                    : null;

                int? durationSeconds = null;
                string? durationFormatted = null;

                if (log.LeftAt.HasValue)
                {
                    durationSeconds = (int)(log.LeftAt.Value - log.ArrivedAt.Value).TotalSeconds;
                    durationFormatted = FormatDuration(log.LeftAt.Value - log.ArrivedAt.Value);
                }

                timeline.Add(new PatrolTimelineEvent
                {
                    Stage = $"checkpoint_{log.OrderIndex}",
                    StageName = $"Checkpoint: {log.AreaNameSnap}",
                    Timestamp = TimezoneHelper.ConvertFromUtc(log.ArrivedAt.Value, tz),
                    DurationSeconds = durationSeconds ?? duration,
                    DurationFormatted = durationFormatted,
                    IsDelayed = false,  // TODO: Calculate delay based on estimated time
                    DelaySeconds = null
                });

                previousTimestamp = log.ArrivedAt.Value;
            }

            // 3. Completed event
            if (session.EndedAt.HasValue)
            {
                var duration = previousTimestamp.HasValue
                    ? (int?)(session.EndedAt.Value - previousTimestamp.Value).TotalSeconds
                    : null;

                timeline.Add(new PatrolTimelineEvent
                {
                    Stage = "completed",
                    StageName = "Patrol Completed",
                    Timestamp = TimezoneHelper.ConvertFromUtc(session.EndedAt.Value, tz),
                    DurationSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(TimeSpan.FromSeconds(duration.Value)) : null
                });
            }

            return timeline;
        }

        private PatrolSessionMetrics BuildMetrics(PatrolSession session)
        {
            var checkpoints = session.PatrolCheckpointLogs.Where(l => l.Status != 0).ToList();
            var totalCheckpoints = checkpoints.Count;
            var completedCheckpoints = checkpoints.Count(l => l.LeftAt != null);
            var completionPercentage = totalCheckpoints > 0
                ? (completedCheckpoints * 100 / totalCheckpoints)
                : 0;

            return new PatrolSessionMetrics
            {
                TotalCheckpoints = totalCheckpoints,
                CompletedCheckpoints = completedCheckpoints,
                CompletionPercentage = completionPercentage,
                TotalIncidents = session.PatrolCases?.Count(c => c.Status != 0) ?? 0,
                TotalDuration = session.EndedAt.HasValue
                    ? FormatDuration(session.EndedAt.Value - session.StartedAt)
                    : null,
                IsCompletedOnTime = session.EndedAt.HasValue
            };
        }

        private string FormatDuration(TimeSpan span)
        {
            if (span.TotalHours >= 1)
                return $"{(int)span.TotalHours}h {span.Minutes}m";
            if (span.TotalMinutes >= 1)
                return $"{span.Minutes}m";
            return $"{span.Seconds}s";
        }

        private byte[] GeneratePdfReport(List<PatrolSessionAnalyticsRead> sessions, PatrolSessionAnalyticsFilter filter)
        {
            var title = string.IsNullOrEmpty(filter.ReportTitle)
                ? "Patrol Session Report"
                : filter.ReportTitle;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item().Text(title).SemiBold().FontSize(16).AlignCenter();
                        column.Item().Text($"Total Sessions: {sessions.Count} | Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC")
                            .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                    });

                    // Content table
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(1f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Security").SemiBold();
                            header.Cell().Element(CellStyle).Text("Route").SemiBold();
                            header.Cell().Element(CellStyle).Text("Started").SemiBold();
                            header.Cell().Element(CellStyle).Text("Duration").SemiBold();
                            header.Cell().Element(CellStyle).Text("Checkpoints").SemiBold();
                            header.Cell().Element(CellStyle).Text("Complete").SemiBold();
                            header.Cell().Element(CellStyle).Text("Incidents").SemiBold();
                        });

                        int index = 1;
                        foreach (var session in sessions)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(session.SecurityName);
                            table.Cell().Element(CellStyle).Text(session.RouteName);
                            table.Cell().Element(CellStyle).Text(session.StartedAt.ToString("yyyy-MM-dd HH:mm"));
                            table.Cell().Element(CellStyle).Text(session.DurationFormatted ?? "Active");
                            table.Cell().Element(CellStyle).Text($"{session.Metrics.CompletedCheckpoints}/{session.Metrics.TotalCheckpoints}");
                            table.Cell().Element(CellStyle).Text($"{session.Metrics.CompletionPercentage}%");
                            table.Cell().Element(CellStyle).Text($"{session.Metrics.TotalIncidents}");
                        }
                    });

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ").SemiBold();
                            x.CurrentPageNumber();
                            x.Span(" of ").SemiBold();
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(4)
                .PaddingHorizontal(6);
        }
    }
}
