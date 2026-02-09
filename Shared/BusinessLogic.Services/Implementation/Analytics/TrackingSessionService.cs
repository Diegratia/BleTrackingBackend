using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using QuestPDF.Drawing;
using Repositories.Repository.Analytics;
using Repositories.Repository.RepoModel;
using System.Threading.Tasks;
using System.Text.Json;
using Helpers.Consumer;
using Shared.Contracts;
using Shared.Contracts.Analytics;
using DataView;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class TrackingSessionService : BaseService, ITrackingSessionService
    {
        private readonly ITrackingReportPresetService _presetService;
        private readonly TrackingSessionRepository _repository;
        private readonly IAuditEmitter _audit;
        private readonly ILogger<TrackingSessionService> _logger;
        private readonly IDistributedCache _cache;

        public TrackingSessionService(
            ITrackingReportPresetService presetService,
            TrackingSessionRepository repository,
            IAuditEmitter audit,
            IHttpContextAccessor http,
            ILogger<TrackingSessionService> logger,
            IDistributedCache cache) : base(http)
        {
            _presetService = presetService;
            _repository = repository;
            _audit = audit;
            _logger = logger;
            _cache = cache;
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        public async Task<GroupedSessionsResponse> GetVisitorSessionSummaryAsync(TrackingAnalyticsFilter request)
        {
            var (data, total, filtered) = await _repository.GetVisitorSessionSummaryAsync(request);

            var tz = TimezoneHelper.Resolve(request.Timezone);

            if (tz.Id != TimeZoneInfo.Utc.Id)
            {
                foreach (var item in data)
                {
                    item.EnterTime =
                        TimezoneHelper.ConvertFromUtc(item.EnterTime, tz);

                    if (item.ExitTime.HasValue)
                        item.ExitTime =
                            TimezoneHelper.ConvertFromUtc(item.ExitTime.Value, tz);
                }
            }

            // === GROUP BY PERSON FIRST (before pagination) ===
            var allGroupedPersons = GroupSessionsByPerson(data);

            // Count unique persons for recordsFiltered
            var recordsFiltered = allGroupedPersons.Count;

            // === APPLY PAGINATION ON PERSONS ===
            var start = request.Start >= 0 ? request.Start : 0;
            var length = request.Length > 0 ? request.Length : 10;
            var pagedPersons = allGroupedPersons
                .Skip(start)
                .Take(length)
                .ToList();

            // === BUILD RESPONSE WITH DATATABLES FORMAT ===
            var response = new GroupedSessionsResponse
            {
                Draw = request.Draw,
                RecordsTotal = total,
                RecordsFiltered = recordsFiltered,
                Persons = pagedPersons
            };

            // Optional: Include summary (calculate from all data, not paged)
            if (request.IncludeSummary)
            {
                response.Summary = BuildSummary(data);
            }

            // Optional: Include visual paths
            if (request.IncludeVisualPaths)
            {
                var visualPaths = await BuildVisualPathsAsync(request, tz);
                response.VisualPaths = visualPaths;
            }

            return response;
        }

        /// <summary>
        /// Group sessions by person with summary statistics
        /// </summary>
        private List<PersonSessionsRead> GroupSessionsByPerson(List<VisitorSessionRead> sessions)
        {
            var grouped = sessions
                .GroupBy(s => new { s.PersonId, s.PersonName, s.PersonType, s.IdentityId, s.CardId, s.CardNumber, s.VisitorId, s.VisitorName, s.MemberId, s.MemberName })
                .Select(g => new PersonSessionsRead
                {
                    PersonId = g.Key.PersonId,
                    PersonName = g.Key.PersonName,
                    PersonType = g.Key.PersonType,
                    IdentityId = g.Key.IdentityId,
                    CardId = g.Key.CardId,
                    CardNumber = g.Key.CardNumber,
                    VisitorId = g.Key.VisitorId,
                    VisitorName = g.Key.VisitorName,
                    MemberId = g.Key.MemberId,
                    MemberName = g.Key.MemberName,

                    TotalSessions = g.Count(),
                    TotalDurationMinutes = g.Where(s => s.DurationInMinutes.HasValue).Sum(s => s.DurationInMinutes.Value),
                    TotalDurationFormatted = FormatDuration(g.Where(s => s.DurationInMinutes.HasValue).Sum(s => s.DurationInMinutes.Value)),
                    TotalIncidents = g.Count(s => s.HasIncident),
                    RestrictedAreasVisited = g.Count(s => s.AreaName?.ToLower().Contains("server") == true ||
                                                               s.AreaName?.ToLower().Contains("vault") == true ||
                                                               s.AreaName?.ToLower().Contains("restricted") == true),

                    AreasVisited = g.Select(s => s.AreaName)
                        .Where(a => !string.IsNullOrEmpty(a))
                        .Distinct()
                        .ToList(),

                    FirstAreaEntered = g.OrderBy(s => s.EnterTime).FirstOrDefault()?.AreaName,
                    LastAreaExited = g.OrderByDescending(s => s.ExitTime ?? s.EnterTime).FirstOrDefault()?.AreaName,
                    CurrentArea = g.FirstOrDefault(s => s.ExitTime == null)?.AreaName,

                    // Map to PersonSessionItemRead (exclude person info)
                    Sessions = g.OrderBy(s => s.EnterTime).Select(s => new PersonSessionItemRead
                    {
                        AreaId = s.AreaId,
                        AreaName = s.AreaName,
                        BuildingId = s.BuildingId,
                        BuildingName = s.BuildingName,
                        FloorId = s.FloorId,
                        FloorName = s.FloorName,
                        FloorplanId = s.FloorplanId,
                        FloorplanName = s.FloorplanName,
                        FloorplanImage = s.FloorplanImage,
                        EnterTime = s.EnterTime,
                        ExitTime = s.ExitTime,
                        DurationMinutes = s.DurationInMinutes,
                        DurationFormatted = s.DurationFormatted,
                        SessionStatus = s.SessionStatus,
                        HasIncident = s.HasIncident,
                        Incident = s.Incident
                    }).ToList()
                })
                .ToList();

            return grouped;
        }

        /// <summary>
        /// Format duration in human-readable string
        /// </summary>
        private string FormatDuration(int totalMinutes)
        {
            if (totalMinutes < 60)
                return $"{totalMinutes} min";

            var hours = totalMinutes / 60;
            var mins = totalMinutes % 60;

            if (mins == 0)
                return $"{hours} hour{(hours > 1 ? "s" : "")}";

            return $"{hours} hour{(hours > 1 ? "s" : "")} {mins} min";
        }

        private static VisitorSessionSummaryRead BuildSummary(List<VisitorSessionRead> sessions)
        {
            if (sessions.Count == 0)
            {
                return new VisitorSessionSummaryRead();
            }

            var uniqueVisitors = sessions.Where(s => s.PersonType == "Visitor")
                .Select(s => s.PersonId)
                .Distinct()
                .Count();

            var uniqueMembers = sessions.Where(s => s.PersonType == "Member")
                .Select(s => s.PersonId)
                .Distinct()
                .Count();

            var areasVisited = sessions
                .Where(s => !string.IsNullOrEmpty(s.AreaName))
                .Select(s => s.AreaName!)
                .Distinct()
                .ToList();

            var totalDuration = sessions
                .Where(s => s.DurationInMinutes.HasValue)
                .Sum(s => s.DurationInMinutes.Value);

            return new VisitorSessionSummaryRead
            {
                TotalDurationMinutes = totalDuration,
                FirstDetection = sessions.Min(s => s.EnterTime),
                LastDetection = sessions.Max(s => s.ExitTime ?? s.EnterTime),
                AreasVisited = areasVisited,
                TotalDetections = sessions.Sum(s => 1), // Each session is a detection group
                TotalSessions = sessions.Count,
                UniqueVisitors = uniqueVisitors,
                UniqueMembers = uniqueMembers
            };
        }

        private async Task<VisualPathsRead> BuildVisualPathsAsync(TrackingAnalyticsFilter request, TimeZoneInfo tz)
        {
            // Generate cache key based on filter parameters
            var cacheKey = GenerateVisualPathsCacheKey(request);

            // Try get from Redis cache
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("VisualPaths Redis cache hit: {CacheKey}", cacheKey);
                var deserialized = JsonSerializer.Deserialize<VisualPathsRead>(cachedData);
                return deserialized ?? new VisualPathsRead();
            }

            // Cache miss - fetch from repository
            _logger.LogInformation("VisualPaths Redis cache miss: {CacheKey}", cacheKey);
            var rawPaths = await _repository.GetVisualPathsDataAsync(request);

            var visualPaths = new VisualPathsRead();

            // Group by floorplan
            var groupedByFloorplan = rawPaths
                .Where(p => p.FloorplanId != Guid.Empty &&
                           p.CoordinateX.HasValue &&
                           p.CoordinateY.HasValue)
                .GroupBy(p => new
                {
                    p.FloorplanId,
                    p.FloorplanName,
                    p.FloorplanImage
                });

            foreach (var group in groupedByFloorplan)
            {
                var floorplanKey = group.Key.FloorplanId.ToString();

                visualPaths.Floorplans[floorplanKey] = new FloorplanPathRead
                {
                    FloorplanId = group.Key.FloorplanId,
                    FloorplanName = group.Key.FloorplanName ?? "Unknown",
                    FloorplanImage = group.Key.FloorplanImage,
                    Points = group.Select(p => new FloorplanPointRead
                    {
                        X = p.CoordinateX!.Value,
                        Y = p.CoordinateY!.Value,
                        Time = tz.Id != TimeZoneInfo.Utc.Id
                            ? TimezoneHelper.ConvertFromUtc(p.TransTimeUtc, tz)
                            : p.TransTimeUtc,
                        Area = p.AreaName,
                        PersonName = p.PersonName,
                        PersonId = p.PersonId
                    }).OrderBy(p => p.Time).ToList()
                };
            }

            // Serialize and cache to Redis for 10 minutes
            var serialized = JsonSerializer.Serialize(visualPaths);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);
            _logger.LogInformation("VisualPaths cached to Redis: {CacheKey} for 10 minutes", cacheKey);

            return visualPaths;
        }

        /// <summary>
        /// Generate cache key for visual paths based on filter parameters
        /// </summary>
        private string GenerateVisualPathsCacheKey(TrackingAnalyticsFilter request)
        {
            // Create key from relevant filter parameters
            var parts = new List<string>
            {
                "visualpaths",
                request.From?.ToString("yyyyMMddHHmm") ?? "none",
                request.To?.ToString("yyyyMMddHHmm") ?? "none",
                request.BuildingId?.ToString() ?? "none",
                request.FloorId?.ToString() ?? "none",
                request.AreaId?.ToString() ?? "none",
                request.VisitorId?.ToString() ?? "none",
                request.MemberId?.ToString() ?? "none",
                request.PersonType ?? "all",
                request.IdentityId ?? "none",
                request.MaxPointsPerFloorplan?.ToString() ?? "none"
            };

            return string.Join(":", parts);
        }

        // public async Task<GroupedSessionsResponse> GetVisitorSessionSummaryByPresetAsync(Guid presetId, TrackingAnalyticsFilter overrideRequest)
        // {
        //     var request = await _presetService.ApplyPresetAsync(presetId);
        //     request.Timezone = overrideRequest.Timezone;
        //     request.PersonType = overrideRequest.PersonType;
        //     if (overrideRequest.From.HasValue)
        //         request.From = overrideRequest.From;

        //     if (overrideRequest.To.HasValue)
        //         request.To = overrideRequest.To;

        //     return await GetVisitorSessionSummaryAsync(request);
        // }

        public async Task<byte[]> ExportVisitorSessionSummaryToPdfAsync(TrackingAnalyticsFilter request)
        {
            // Repository returns all sessions (no pagination)
            var (sessions, _, _) = await _repository.GetVisitorSessionSummaryAsync(request);
            return GeneratePdfReport(sessions, request);
        }

        public async Task<byte[]> ExportVisitorSessionSummaryToExcelAsync(TrackingAnalyticsFilter request)
        {
            // Repository returns all sessions (no pagination)
            var (sessions, _, _) = await _repository.GetVisitorSessionSummaryAsync(request);
            return GenerateExcelReport(sessions, request);
        }

        private byte[] GeneratePdfReport(List<VisitorSessionRead> sessions, TrackingAnalyticsFilter request)
        {
            string reportTitle = GenerateReportTitle(request);
            string filterInfo = GenerateFilterInfo(request);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text(reportTitle)
                                .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                            if (!string.IsNullOrEmpty(filterInfo))
                            {
                                column.Item()
                                    .PaddingTop(5)
                                    .Text(filterInfo)
                                    .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                            }

                            column.Item()
                                .PaddingTop(10)
                                .Text($"Total Sessions: {sessions.Count}")
                                .FontSize(11).FontColor(Colors.Blue.Medium).AlignCenter();
                        });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").SemiBold();
                            header.Cell().Element(CellStyle).Text("Visitor Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Type").SemiBold();
                            header.Cell().Element(CellStyle).Text("Building").SemiBold();
                            header.Cell().Element(CellStyle).Text("Floor").SemiBold();
                            header.Cell().Element(CellStyle).Text("Area").SemiBold();
                            header.Cell().Element(CellStyle).Text("Floorplan").SemiBold();
                            header.Cell().Element(CellStyle).Text("Enter Time").SemiBold();
                            header.Cell().Element(CellStyle).Text("Exit Time").SemiBold();
                            header.Cell().Element(CellStyle).Text("Duration").SemiBold();
                            header.Cell().Element(CellStyle).Text("Status").SemiBold();
                        });

                        int index = 1;
                        foreach (var session in sessions)
                        {
                            table.Cell().Element(CellStyle).Text(index++.ToString());
                            table.Cell().Element(CellStyle).Text(session.VisitorName ?? "N/A");
                            table.Cell().Element(CellStyle).Text(session.PersonType ?? "Visitor");
                            table.Cell().Element(CellStyle).Text(session.BuildingName ?? "N/A");
                            table.Cell().Element(CellStyle).Text(session.FloorName ?? "N/A");
                            table.Cell().Element(CellStyle).Text(session.AreaName ?? "N/A");
                            table.Cell().Element(CellStyle).Text(session.FloorplanName ?? "N/A");
                            table.Cell().Element(CellStyle).Text(session.EnterTime.ToString("yyyy-MM-dd HH:mm"));
                            table.Cell().Element(CellStyle).Text(
                                session.ExitTime?.ToString("yyyy-MM-dd HH:mm") ?? "Still Active");
                            table.Cell().Element(CellStyle).Text(
                                session.DurationInMinutes.HasValue ?
                                $"{session.DurationInMinutes} min" : "N/A");
                            table.Cell().Element(CellStyle).Text(
                                session.ExitTime.HasValue ? "Completed" : "Active");
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Generated at: ").SemiBold();
                            txt.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
                            txt.Span(" | Page ").SemiBold();
                            txt.CurrentPageNumber();
                            txt.Span(" of ").SemiBold();
                            txt.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        private byte[] GenerateExcelReport(List<VisitorSessionRead> sessions, TrackingAnalyticsFilter request)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Visitor Sessions");

            string reportTitle = GenerateReportTitle(request);
            string filterInfo = GenerateFilterInfo(request);

            worksheet.Cell(1, 1).Value = reportTitle;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 11).Merge();

            if (!string.IsNullOrEmpty(filterInfo))
            {
                worksheet.Cell(2, 1).Value = filterInfo;
                worksheet.Range(2, 1, 2, 11).Merge();
            }

            worksheet.Cell(3, 1).Value = $"Total Sessions: {sessions.Count}";
            worksheet.Cell(3, 1).Style.Font.Bold = true;
            worksheet.Cell(3, 1).Style.Font.FontColor = XLColor.Blue;
            worksheet.Range(3, 1, 3, 11).Merge();

            int headerRow = 5;
            worksheet.Cell(headerRow, 1).Value = "#";
            worksheet.Cell(headerRow, 2).Value = "Visitor Name";
            worksheet.Cell(headerRow, 3).Value = "Type";
            worksheet.Cell(headerRow, 4).Value = "Building";
            worksheet.Cell(headerRow, 5).Value = "Floor";
            worksheet.Cell(headerRow, 6).Value = "Area";
            worksheet.Cell(headerRow, 7).Value = "Floorplan";
            worksheet.Cell(headerRow, 8).Value = "Enter Time";
            worksheet.Cell(headerRow, 9).Value = "Exit Time";
            worksheet.Cell(headerRow, 10).Value = "Duration (min)";
            worksheet.Cell(headerRow, 11).Value = "Status";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 11);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            int row = headerRow + 1;
            int no = 1;

            foreach (var session in sessions)
            {
                worksheet.Cell(row, 1).Value = no++;
                worksheet.Cell(row, 2).Value = session.VisitorName ?? "N/A";
                worksheet.Cell(row, 3).Value = session.PersonType ?? "Visitor";
                worksheet.Cell(row, 4).Value = session.BuildingName ?? "N/A";
                worksheet.Cell(row, 5).Value = session.FloorName ?? "N/A";
                worksheet.Cell(row, 6).Value = session.AreaName ?? "N/A";
                worksheet.Cell(row, 7).Value = session.FloorplanName ?? "N/A";
                worksheet.Cell(row, 8).Value = session.EnterTime;
                worksheet.Cell(row, 8).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";

                if (session.ExitTime.HasValue)
                {
                    worksheet.Cell(row, 9).Value = session.ExitTime.Value;
                    worksheet.Cell(row, 9).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
                }
                else
                {
                    worksheet.Cell(row, 9).Value = "Still Active";
                }

                worksheet.Cell(row, 10).Value = session.DurationInMinutes;
                worksheet.Cell(row, 11).Value = session.ExitTime.HasValue ? "Completed" : "Active";

                var statusCell = worksheet.Cell(row, 11);
                if (session.ExitTime.HasValue)
                {
                    statusCell.Style.Font.FontColor = XLColor.Green;
                }
                else
                {
                    statusCell.Style.Font.FontColor = XLColor.Orange;
                }

                row++;
            }

            worksheet.Columns().AdjustToContents();

            var dataRange = worksheet.Range(headerRow, 1, row - 1, 11);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Cell(row + 2, 1).Value = "Generated at:";
            worksheet.Cell(row + 2, 2).Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";
            worksheet.Cell(row + 2, 2).Style.Font.Italic = true;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private string GenerateReportTitle(TrackingAnalyticsFilter request)
        {
            string title = "Visitor Session Summary Report";

            if (!string.IsNullOrEmpty(request.ReportTitle))
                return request.ReportTitle;

            if (!string.IsNullOrEmpty(request.TimeRange))
            {
                title += $" - {request.TimeRange.ToUpper()}";
            }

            return title;
        }

        private string GenerateFilterInfo(TrackingAnalyticsFilter request)
        {
            var filters = new List<string>();

            if (request.From.HasValue && request.To.HasValue)
            {
                filters.Add($"Period: {request.From.Value:yyyy-MM-dd} to {request.To.Value:yyyy-MM-dd}");
            }
            else if (!string.IsNullOrEmpty(request.TimeRange))
            {
                filters.Add($"Time Range: {request.TimeRange}");
            }

            if (request.BuildingId.HasValue)
            {
                filters.Add($"Building Filtered");
            }

            if (request.FloorId.HasValue)
            {
                filters.Add($"Floor Filtered");
            }

            if (request.AreaId.HasValue)
            {
                filters.Add($"Area Filtered");
            }

            if (request.VisitorId.HasValue)
            {
                filters.Add($"Visitor Filtered");
            }

            return filters.Any() ? string.Join(" | ", filters) : "All Data";
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(4)
                .PaddingHorizontal(6);
        }

        public async Task<PeakHoursByAreaRead> GetPeakHoursByAreaAsync(TrackingAnalyticsFilter request)
        {
            var rawData = await _repository.GetPeakHoursByAreaAsync(request);

            if (!rawData.Any())
            {
                return new PeakHoursByAreaRead
                {
                    Labels = GenerateHourLabels(),
                    Series = new List<ChartSeriesDto>()
                };
            }

            // Group by area and create series
            var groupedByArea = rawData
                .Where(x => !string.IsNullOrEmpty(x.AreaName))
                .GroupBy(x => x.AreaName!);

            var series = new List<ChartSeriesDto>();

            foreach (var areaGroup in groupedByArea)
            {
                var areaName = areaGroup.Key;
                var hourlyData = new int[24]; // 24 hours

                // Fill in the data
                foreach (var item in areaGroup)
                {
                    if (item.Hour >= 0 && item.Hour < 24)
                    {
                        hourlyData[item.Hour] = item.Count;
                    }
                }

                series.Add(new ChartSeriesDto
                {
                    Name = areaName,
                    Data = hourlyData.ToList()
                });
            }

            // Sort series by total count (descending) - most active areas first
            series = series
                .OrderByDescending(s => s.Data.Sum())
                .ToList();

            // Optional: Limit to top 10 areas to avoid overcrowding the chart
            const int maxAreas = 10;
            if (series.Count > maxAreas)
            {
                var otherData = new int[24];
                for (int i = 0; i < 24; i++)
                {
                    otherData[i] = series.Skip(maxAreas).Sum(s => s.Data[i]);
                }

                series = series.Take(maxAreas).ToList();

                // Add "Others" series if there's data
                if (otherData.Sum() > 0)
                {
                    series.Add(new ChartSeriesDto
                    {
                        Name = "Others",
                        Data = otherData.ToList()
                    });
                }
            }

            return new PeakHoursByAreaRead
            {
                Labels = GenerateHourLabels(),
                Series = series
            };
        }

        private static List<string> GenerateHourLabels()
        {
            var labels = new List<string>();
            for (int i = 0; i < 24; i++)
            {
                labels.Add($"{i:D2}:00");
            }
            return labels;
        }
    }
}
