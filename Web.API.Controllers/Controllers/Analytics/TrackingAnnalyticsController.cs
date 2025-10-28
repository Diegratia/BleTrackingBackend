using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using System.Threading.Tasks;
using Repositories.Repository.RepoModel;
using BusinessLogic.Services.Interface.Analytics;

namespace Web.API.Controllers.Controllers.Analytics
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("RequireAllAndUserCreated")]
    public class TrackingAnalyticsController : ControllerBase
    {
        private readonly ITrackingAnalyticsService _service;

        public TrackingAnalyticsController(ITrackingAnalyticsService service)
        {
            _service = service;
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
        
        [HttpPost("card-summary")]
        public async Task<IActionResult> GetCard([FromBody] TrackingAnalyticsRequestRM request)
        {
            var result = await _service.GetCardSummaryAsync(request);
            return Ok(result);
        }
    }
}
