using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using System.Threading.Tasks;
using Repositories.Repository.RepoModel;
using BusinessLogic.Services.Interface.Analytics;
using System.Net.Mime;

namespace Web.API.Controllers.Controllers.Analytics
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("RequireAllAndUserCreated")]
    public class TrackingAnalyticsController : ControllerBase
    {
        private readonly ITrackingAnalyticsService _service;
        private readonly ITrackingAnalyticsV2Service _serviceV2;

        public TrackingAnalyticsController(ITrackingAnalyticsService service, ITrackingAnalyticsV2Service serviceV2)
        {
            _service = service;
            _serviceV2 = serviceV2;
        }

        // ===================================================================
        // 1️⃣ Area Summary (Incident-level)
        // ===================================================================
        [HttpPost("area")]
        public async Task<IActionResult> GetArea([FromBody] TrackingAnalyticsRequestRM request)
        {
            var response = await _service.GetAreaSummaryAsync(request);
            return Ok(response);
        }

        // ===================================================================
        // 2️⃣ Daily Summary (Incident-level)
        // ===================================================================
        [HttpPost("daily")]
        public async Task<IActionResult> GetDaily([FromBody] TrackingAnalyticsRequestRM request)
        {
            var response = await _service.GetDailySummaryAsync(request);
            return Ok(response);
        }

        // ===================================================================
        // 3️⃣ Status Reader (Incident-level)
        // ===================================================================
        [HttpPost("reader")]
        public async Task<IActionResult> GetReader([FromBody] TrackingAnalyticsRequestRM request)
        {
            var response = await _service.GetReaderSummaryAsync(request);
            return Ok(response);
        }

        // ===================================================================
        // 4️⃣ Visitor Summary (Incident-level)
        // ===================================================================
        [HttpPost("visitor")]
        public async Task<IActionResult> GetVisitor([FromBody] TrackingAnalyticsRequestRM request)
        {
            var response = await _service.GetVisitorSummaryAsync(request);
            return Ok(response);
        }

        // ===================================================================
        // 5️⃣ Building Summary (Incident-level)
        // ===================================================================
        [HttpPost("building")]
        public async Task<IActionResult> GetBuilding([FromBody] TrackingAnalyticsRequestRM request)
        {
            var response = await _service.GetBuildingSummaryAsync(request);
            return Ok(response);
        }

        // 🔹 GET movement by Card ID
        [HttpGet("movement/{cardId}")]
        public async Task<IActionResult> GetMovement(Guid cardId)
        {
            var result = await _service.GetTrackingMovementByCardIdAsync(cardId);
            return Ok(result);
        }

        // 🔹 POST Heatmap
        [HttpPost("heatmap")]
        public async Task<IActionResult> GetHeatmap([FromBody] TrackingAnalyticsRequestRM request)
        {
            var result = await _service.GetHeatmapDataAsync(request);
            return Ok(result);
        }

        [HttpPost("latest-position")]
        public async Task<IActionResult> GetCard([FromBody] TrackingAnalyticsRequestRM request)
        {
            var result = await _service.GetCardSummaryAsync(request);
            return Ok(result);
        }

        [HttpPost("visitor-session")]
        public async Task<IActionResult> GetVisitorSessionSummaryAsync([FromBody] TrackingAnalyticsRequestRM request)
        {
            var result = await _serviceV2.GetVisitorSessionSummaryAsync(request);
            return Ok(result);
        }
        [HttpPost("area-accessed")]
        public async Task<IActionResult> GetAreaAccessed([FromBody] TrackingAnalyticsRequestRM request)
        {
            var result = await _service.GetAreaAccessedSummaryAsync(request);
            return Ok(result);
        }
        [HttpPost("area-accessed-v2")]
        public async Task<IActionResult> GetAreaAccessedV2([FromBody] TrackingAnalyticsRequestRM request)
        {
            var result = await _service.GetAreaAccessedSummaryAsyncV2(request);
            return Ok(result);
        }
        [HttpPost("area-accessed-v3")]
        public async Task<IActionResult> GetAreaAccessedSummaryAsyncV3([FromBody] TrackingAnalyticsRequestRM request)
        {
            var result = await _service.GetAreaAccessedSummaryAsyncV3(request);
            return Ok(result);
        }
        
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportVisitorSessionSummaryPdf([FromQuery] TrackingAnalyticsRequestRM request)
        {
            try
            {
                var pdfBytes = await _serviceV2.ExportVisitorSessionSummaryToPdfAsync(request);
                
                // Generate filename berdasarkan filter
                string fileName = GenerateExportFileName(request, "pdf");
                
                return File(pdfBytes, MediaTypeNames.Application.Pdf, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating PDF: {ex.Message}");
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportVisitorSessionSummaryExcel([FromQuery] TrackingAnalyticsRequestRM request)
        {
            try
            {
                var excelBytes = await _serviceV2.ExportVisitorSessionSummaryToExcelAsync(request);

                // Generate filename berdasarkan filter
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

        private string GenerateExportFileName(TrackingAnalyticsRequestRM request, string extension)
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
