using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using System.Threading.Tasks;
using Repositories.Repository.RepoModel;
using BusinessLogic.Services.Interface.Analytics;
using System.Net.Mime;
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;
using Shared.Contracts.Analytics;
using Data.ViewModels.ResponseHelper;

namespace Web.API.Controllers.Controllers.Analytics
{
    [ApiController]
    [Route("api/[controller]")]
    [MinLevel(LevelPriority.Primary)]

    public class TrackingAnalyticsController : ControllerBase
    {
        private readonly ITrackingSummaryService _summaryService;
        private readonly ITrackingSessionService _sessionService;

        public TrackingAnalyticsController(
            ITrackingSummaryService summaryService,
            ITrackingSessionService sessionService)
        {
            _summaryService = summaryService;
            _sessionService = sessionService;
        }

        // ===================================================================
        // SUMMARY ENDPOINTS
        // ===================================================================

        // 1️⃣ Area Summary
        [HttpPost("area")]
        public async Task<IActionResult> GetArea([FromBody] TrackingAnalyticsFilter request)
        {
            var response = await _summaryService.GetAreaSummaryAsync(request);
            return Ok(ApiResponse.Success("Area summary retrieved successfully", response));
        }

        // 2️⃣ Daily Summary
        [HttpPost("daily")]
        public async Task<IActionResult> GetDaily([FromBody] TrackingAnalyticsFilter request)
        {
            var response = await _summaryService.GetDailySummaryAsync(request);
            return Ok(ApiResponse.Success("Daily summary retrieved successfully", response));
        }

        // 3️⃣ Reader Summary
        [HttpPost("reader")]
        public async Task<IActionResult> GetReader([FromBody] TrackingAnalyticsFilter request)
        {
            var response = await _summaryService.GetReaderSummaryAsync(request);
            return Ok(ApiResponse.Success("Reader summary retrieved successfully", response));
        }

        // 4️⃣ Visitor Summary
        [HttpPost("visitor")]
        public async Task<IActionResult> GetVisitor([FromBody] TrackingAnalyticsFilter request)
        {
            var response = await _summaryService.GetVisitorSummaryAsync(request);
            return Ok(ApiResponse.Success("Visitor summary retrieved successfully", response));
        }

        // 5️⃣ Building Summary
        [HttpPost("building")]
        public async Task<IActionResult> GetBuilding([FromBody] TrackingAnalyticsFilter request)
        {
            var response = await _summaryService.GetBuildingSummaryAsync(request);
            return Ok(ApiResponse.Success("Building summary retrieved successfully", response));
        }

        // 6️⃣ Movement by Card ID
        [HttpGet("movement/{cardId}")]
        public async Task<IActionResult> GetMovement(Guid cardId)
        {
            var result = await _summaryService.GetTrackingMovementByCardIdAsync(cardId);
            return Ok(ApiResponse.Success("Tracking movement retrieved successfully", result));
        }

        // 7️⃣ Heatmap Data
        [HttpPost("heatmap")]
        public async Task<IActionResult> GetHeatmap([FromBody] TrackingAnalyticsFilter request)
        {
            var result = await _summaryService.GetHeatmapDataAsync(request);
            return Ok(ApiResponse.Success("Heatmap data retrieved successfully", result));
        }

        // 8️⃣ Latest Position (Card Summary)
        [HttpPost("latest-position")]
        public async Task<IActionResult> GetCard([FromBody] TrackingAnalyticsFilter request)
        {
            var result = await _summaryService.GetCardSummaryAsync(request);
            return Ok(ApiResponse.Success("Card summary retrieved successfully", result));
        }

        // Area Accessed Summary (with Chart)
        [HttpPost("area-accessed")]
        public async Task<IActionResult> GetAreaAccessedSummaryAsyncV3([FromBody] TrackingAnalyticsFilter request)
        {
            var result = await _summaryService.GetAreaAccessedSummaryAsyncV3(request);
            return Ok(ApiResponse.Success("Area accessed summary retrieved successfully", result));
        }

        // ===================================================================
        // SESSION ENDPOINTS
        // ===================================================================

        // Visitor Session Summary
        [HttpPost("visitor-session")]
        public async Task<IActionResult> GetVisitorSessionSummaryAsync(
            [FromBody] TrackingAnalyticsFilter request,
            [FromQuery] bool includeVisualPaths = false,
            [FromQuery] bool includeSummary = false,
            [FromQuery] bool includeIncident = true,
            [FromQuery] string type = null,
            [FromQuery] bool? hasIncident = null,
            [FromQuery] int? maxPointsPerFloorplan = null)
        {
            // Set parameters from query string (for backward compatibility)
            if (includeVisualPaths)
                request.IncludeVisualPaths = true;

            if (includeSummary)
                request.IncludeSummary = true;

            if (maxPointsPerFloorplan.HasValue)
                request.MaxPointsPerFloorplan = maxPointsPerFloorplan.Value;

            if (request.IncludeIncident) // Only override if not explicitly set
                request.IncludeIncident = includeIncident;

            if (!string.IsNullOrEmpty(type))
                request.Type = type;

            if (hasIncident.HasValue)
                request.HasIncident = hasIncident;

            var result = await _sessionService.GetVisitorSessionSummaryAsync(request);
            return Ok(ApiResponse.Success("Visitor session summary retrieved successfully", result));
        }

        [HttpPost("peak-hours-by-area")]
        public async Task<IActionResult> GetPeakHoursByArea(
            [FromBody] TrackingAnalyticsFilter request,
            [FromQuery] PeakHoursGroupByMode groupByMode = PeakHoursGroupByMode.Area)
        {
            var result = await _sessionService.GetPeakHoursByAreaAsync(request, groupByMode);
            return Ok(ApiResponse.Success("Peak hours by area retrieved successfully", result));
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportVisitorSessionSummaryPdf([FromQuery] TrackingAnalyticsFilter request)
        {
            var pdfBytes = await _sessionService.ExportVisitorSessionSummaryToPdfAsync(request);
            string fileName = GenerateExportFileName(request, "pdf");
            return File(pdfBytes, MediaTypeNames.Application.Pdf, fileName);
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportVisitorSessionSummaryExcel([FromQuery] TrackingAnalyticsFilter request)
        {
            var excelBytes = await _sessionService.ExportVisitorSessionSummaryToExcelAsync(request);
            string fileName = GenerateExportFileName(request, "xlsx");
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private string GenerateExportFileName(TrackingAnalyticsFilter request, string extension)
        {
            var parts = new List<string> { "VisitorSessions" };

            if (!string.IsNullOrEmpty(request.TimeRange))
            {
                parts.Add(request.TimeRange);
            }

            if (request.From.HasValue)
            {
                parts.Add(request.From.Value.ToString("yyyyMMdd"));
            }

            if (request.To.HasValue)
            {
                parts.Add(request.To.Value.ToString("yyyyMMdd"));
            }

            return $"{string.Join("_", parts)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{extension}";
        }
    }
}
