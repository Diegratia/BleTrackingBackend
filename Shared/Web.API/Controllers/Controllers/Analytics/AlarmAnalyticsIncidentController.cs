using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using System.Threading.Tasks;
using Repositories.Repository.RepoModel;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels.ResponseHelper;
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers.Analytics
{
    [ApiController]
    [Route("api/[controller]")]
    [MinLevel(LevelPriority.Primary)]

    public class AlarmAnalyticsIncidentController : ControllerBase
    {
        private readonly IAlarmAnalyticsIncidentService _service;

        public AlarmAnalyticsIncidentController(IAlarmAnalyticsIncidentService service)
        {
            _service = service;
        }

        // ===================================================================
        // 1️⃣ Area Summary (Incident-level)
        // ===================================================================
        [HttpPost("area")]
        public async Task<IActionResult> GetArea([FromBody] AlarmAnalyticsFilter request)
        {
            var response = await _service.GetAreaSummaryChartAsync(request);
            return Ok(ApiResponse.Success("Area Summary retrieved successfully", response));
        }

        // ===================================================================
        // 2️⃣ Daily Summary (Incident-level)
        // ===================================================================
        [HttpPost("daily")]
        public async Task<IActionResult> GetDaily([FromBody] AlarmAnalyticsFilter request)
        {
            var response = await _service.GetDailySummaryAsync(request);
            return Ok(ApiResponse.Success("Daily Summary retrieved successfully", response));
        }

        // ===================================================================
        // 3️⃣ Status Summary (Incident-level)
        // ===================================================================
        [HttpPost("status")]
        public async Task<IActionResult> GetStatus([FromBody] AlarmAnalyticsFilter request)
        {
            var response = await _service.GetStatusSummaryAsync(request);
            return Ok(ApiResponse.Success("Status Summary retrieved successfully", response));
        }

        // ===================================================================
        // 4️⃣ Visitor Summary (Incident-level)
        // ===================================================================
        [HttpPost("visitor")]
        public async Task<IActionResult> GetVisitor([FromBody] AlarmAnalyticsFilter request)
        {
            var response = await _service.GetVisitorSummaryAsync(request);
            return Ok(ApiResponse.Success("Visitor Summary retrieved successfully", response));
        }

        // ===================================================================
        // 5️⃣ Building Summary (Incident-level)
        // ===================================================================
        [HttpPost("building")]
        public async Task<IActionResult> GetBuilding([FromBody] AlarmAnalyticsFilter request)
        {
            var response = await _service.GetBuildingSummaryAsync(request);
            return Ok(ApiResponse.Success("Building Summary retrieved successfully", response));
        }

        // ===================================================================
        // 6️⃣ Hourly Status Summary
        // ===================================================================
        [HttpPost("hourly")]
        public async Task<IActionResult> GetHourlyStatusSummaryAsync([FromBody] AlarmAnalyticsFilter request)
        {
            var response = await _service.GetHourlyStatusSummaryAsync(request);
            return Ok(ApiResponse.Success("Hourly Status Summary retrieved successfully", response));
        }
    }
}
