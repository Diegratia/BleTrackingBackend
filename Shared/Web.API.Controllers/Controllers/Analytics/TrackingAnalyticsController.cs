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
            return Ok(response);
        }

        // 2️⃣ Daily Summary
        [HttpPost("daily")]
        public async Task<IActionResult> GetDaily([FromBody] TrackingAnalyticsFilter request)
        {
            var response = await _summaryService.GetDailySummaryAsync(request);
            return Ok(response);
        }

        // 3️⃣ Reader Summary
        [HttpPost("reader")]
        public async Task<IActionResult> GetReader([FromBody] TrackingAnalyticsFilter request)
        {
            var response = await _summaryService.GetReaderSummaryAsync(request);
            return Ok(response);
        }

        // 4️⃣ Visitor Summary
        [HttpPost("visitor")]
        public async Task<IActionResult> GetVisitor([FromBody] TrackingAnalyticsFilter request)
        {
            var response = await _summaryService.GetVisitorSummaryAsync(request);
            return Ok(response);
        }

        // 5️⃣ Building Summary
        [HttpPost("building")]
        public async Task<IActionResult> GetBuilding([FromBody] TrackingAnalyticsFilter request)
        {
            var response = await _summaryService.GetBuildingSummaryAsync(request);
            return Ok(response);
        }

        // 6️⃣ Movement by Card ID
        [HttpGet("movement/{cardId}")]
        public async Task<IActionResult> GetMovement(Guid cardId)
        {
            var result = await _summaryService.GetTrackingMovementByCardIdAsync(cardId);
            return Ok(result);
        }

        // 7️⃣ Heatmap Data
        [HttpPost("heatmap")]
        public async Task<IActionResult> GetHeatmap([FromBody] TrackingAnalyticsFilter request)
        {
            var result = await _summaryService.GetHeatmapDataAsync(request);
            return Ok(result);
        }

        // 8️⃣ Latest Position (Card Summary)
        [HttpPost("latest-position")]
        public async Task<IActionResult> GetCard([FromBody] TrackingAnalyticsFilter request)
        {
            var result = await _summaryService.GetCardSummaryAsync(request);
            return Ok(result);
        }

        // 9️⃣ Area Accessed Summary (with Chart)
        [HttpPost("area-accessed")]
        public async Task<IActionResult> GetAreaAccessedSummaryAsyncV3([FromBody] TrackingAnalyticsFilter request)
        {
            var result = await _summaryService.GetAreaAccessedSummaryAsyncV3(request);
            return Ok(result);
        }

        // ===================================================================
        // SESSION ENDPOINTS
        // ===================================================================

        // Visitor Session Summary
        [HttpPost("visitor-session")]
        public async Task<IActionResult> GetVisitorSessionSummaryAsync(
            [FromBody] TrackingAnalyticsFilter request,
            [FromQuery] bool includeVisualPaths = false)
        {
            var result = await _sessionService.GetVisitorSessionSummaryAsync(request, includeVisualPaths);
            return Ok(result);
        }

        // Peak Hours by Area
        [HttpPost("peak-hours-by-area")]
        public async Task<IActionResult> GetPeakHoursByArea([FromBody] TrackingAnalyticsFilter request)
        {
            var result = await _sessionService.GetPeakHoursByAreaAsync(request);
            return Ok(result);
        }

        // Export to PDF
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportVisitorSessionSummaryPdf([FromQuery] TrackingAnalyticsFilter request)
        {
            try
            {
                var pdfBytes = await _sessionService.ExportVisitorSessionSummaryToPdfAsync(request);
                string fileName = GenerateExportFileName(request, "pdf");
                return File(pdfBytes, MediaTypeNames.Application.Pdf, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating PDF: {ex.Message}");
            }
        }

        // Export to Excel
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportVisitorSessionSummaryExcel([FromQuery] TrackingAnalyticsFilter request)
        {
            try
            {
                var excelBytes = await _sessionService.ExportVisitorSessionSummaryToExcelAsync(request);
                string fileName = GenerateExportFileName(request, "xlsx");
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating Excel: {ex.Message}");
            }
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
