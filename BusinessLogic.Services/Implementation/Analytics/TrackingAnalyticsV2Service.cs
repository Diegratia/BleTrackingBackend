// File: BusinessLogic/Services/Implementation/Analytics/TrackingAnalyticsV2Service.cs
using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using QuestPDF.Drawing;
using Repositories.Repository.Analytics;
using Repositories.Repository.RepoModel;
using System.Threading.Tasks;
using Entities.Models;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class TrackingAnalyticsV2Service : ITrackingAnalyticsV2Service
    {
        private readonly ITrackingReportPresetService _presetService; 
        private readonly TrackingAnalyticsV2Repository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TrackingAnalyticsV2Service> _logger;

        public TrackingAnalyticsV2Service(
            ITrackingReportPresetService presetService, 
            TrackingAnalyticsV2Repository repository,
            IMapper mapper,
            ILogger<TrackingAnalyticsV2Service> logger)
        {
            _presetService = presetService;
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<ResponseCollection<VisitorSessionSummaryRM>> GetVisitorSessionSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetVisitorSessionSummaryAsync(request);
                return ResponseCollection<VisitorSessionSummaryRM>.Ok(data, "Visitor session summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetVisitorSessionSummaryAsync");
                return ResponseCollection<VisitorSessionSummaryRM>.Error($"Internal error: {ex.Message}");
            }
        }
            public async Task<ResponseCollection<VisitorSessionSummaryRM>> GetVisitorSessionSummaryByPresetAsync(Guid presetId)
        {
            try
            {
                // 1. Get analytics request from preset
                var analyticsRequest = await _presetService.ApplyPresetAsync(presetId);
                
                // 2. Use existing method to get data
                var data = await _repository.GetVisitorSessionSummaryAsync(analyticsRequest);
                
                return ResponseCollection<VisitorSessionSummaryRM>.Ok(data, "Visitor session summary from preset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetVisitorSessionSummaryByPresetAsync");
                return ResponseCollection<VisitorSessionSummaryRM>.Error($"Internal error: {ex.Message}");
            }
        }
        
        
        public async Task<byte[]> ExportVisitorSessionSummaryToPdfAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var sessions = await _repository.GetVisitorSessionSummaryAsync(request);

                // Generate judul report berdasarkan filter
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
                            // Define column widths
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35); // No
                                columns.RelativeColumn(2.5f); // Visitor Name
                                columns.RelativeColumn(1.5f); // Person Type
                                columns.RelativeColumn(2); // Building
                                columns.RelativeColumn(1.5f); // Floor
                                columns.RelativeColumn(2); // Area
                                columns.RelativeColumn(2); // Floorplan
                                columns.RelativeColumn(2.5f); // Enter Time
                                columns.RelativeColumn(2.5f); // Exit Time
                                columns.RelativeColumn(1.5f); // Duration
                                columns.RelativeColumn(1.5f); // Status
                            });

                            // Header
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

                            // Rows
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to PDF");
                throw;
            }
        }

        public async Task<byte[]> ExportVisitorSessionSummaryToExcelAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var sessions = await _repository.GetVisitorSessionSummaryAsync(request);
                
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Visitor Sessions");

                // Report Header
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

                // Column Headers (start at row 5)
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

                // Style header
                var headerRange = worksheet.Range(headerRow, 1, headerRow, 11);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Data rows
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
                    
                    // Style status column
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

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();
                
                // Add borders to data
                var dataRange = worksheet.Range(headerRow, 1, row - 1, 11);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Add timestamp
                worksheet.Cell(row + 2, 1).Value = "Generated at:";
                worksheet.Cell(row + 2, 2).Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";
                worksheet.Cell(row + 2, 2).Style.Font.Italic = true;

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to Excel");
                throw;
            }
        }

        private string GenerateReportTitle(TrackingAnalyticsRequestRM request)
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

        private string GenerateFilterInfo(TrackingAnalyticsRequestRM request)
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
            
            // Tambahkan filter lain sesuai kebutuhan
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
    }

}
