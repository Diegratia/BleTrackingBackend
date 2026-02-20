using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using DataView;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Contracts.Analytics;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web.API.Controllers.Controllers
{
    [Route("api/patrol-analytics")]
    [ApiController]
    [MinLevel(LevelPriority.Primary)]
    public class PatrolSessionAnalyticsController : ControllerBase
    {
        private readonly IPatrolSessionAnalyticsService _service;

        public PatrolSessionAnalyticsController(IPatrolSessionAnalyticsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets patrol session report with filters (POST - DataTables Pattern)
        /// POST /api/PatrolSessionAnalytics/report
        /// </summary>
        [HttpPost("report")]
        public async Task<IActionResult> GetReport([FromBody] DataTablesProjectedRequest request)
        {
            // Deserialize filters from request.Filters
            var filter = new PatrolSessionAnalyticsFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<PatrolSessionAnalyticsFilter>(
                    request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new PatrolSessionAnalyticsFilter();
            }

            var result = await _service.GetReportAsync(request, filter);
            return Ok(ApiResponse.Paginated("Patrol report retrieved successfully", result));
        }

        /// <summary>
        /// Gets single patrol session timeline
        /// GET /api/PatrolSessionAnalytics/timeline/{sessionId}
        /// </summary>
        [HttpGet("timeline/{sessionId}")]
        public async Task<IActionResult> GetTimeline(Guid sessionId)
        {
            var result = await _service.GetSessionTimelineAsync(sessionId);
            if (result == null)
                return NotFound(ApiResponse.NotFound("Session not found"));

            return Ok(ApiResponse.Success("Timeline retrieved successfully", result));
        }

        /// <summary>
        /// Exports patrol report to PDF (Flat body - no nested filters)
        /// POST /api/PatrolSessionAnalytics/export/pdf
        /// </summary>
        [HttpPost("export/pdf")]
        public async Task<IActionResult> ExportToPdf([FromBody] PatrolSessionAnalyticsFilter filter)
        {
            var pdfBytes = await _service.ExportToPdfAsync(filter);

            return File(pdfBytes,
                "application/pdf",
                $"PatrolReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}
