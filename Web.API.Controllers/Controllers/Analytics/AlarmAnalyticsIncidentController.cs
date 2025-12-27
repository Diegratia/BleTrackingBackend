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
        public async Task<IActionResult> GetArea([FromBody] AlarmAnalyticsRequestRM request)
        {
            var response = await _service.GetAreaSummaryAsync(request);
            return Ok(response);
        }

        // ===================================================================
        // 2️⃣ Daily Summary (Incident-level)
        // ===================================================================
        [HttpPost("daily")]
        public async Task<IActionResult> GetDaily([FromBody] AlarmAnalyticsRequestRM request)
        {
            var response = await _service.GetDailySummaryAsync(request);
            return Ok(response);
        }

        // ===================================================================
        // 3️⃣ Status Summary (Incident-level)
        // ===================================================================
        [HttpPost("status")]
        public async Task<IActionResult> GetStatus([FromBody] AlarmAnalyticsRequestRM request)
        {
            var response = await _service.GetStatusSummaryAsync(request);
            return Ok(response);
        }

        // ===================================================================
        // 4️⃣ Visitor Summary (Incident-level)
        // ===================================================================
        [HttpPost("visitor")]
        public async Task<IActionResult> GetVisitor([FromBody] AlarmAnalyticsRequestRM request)
        {
            var response = await _service.GetVisitorSummaryAsync(request);
            return Ok(response);
        }

        // ===================================================================
        // 5️⃣ Building Summary (Incident-level)
        // ===================================================================
        [HttpPost("building")]
        public async Task<IActionResult> GetBuilding([FromBody] AlarmAnalyticsRequestRM request)
        {
            var response = await _service.GetBuildingSummaryAsync(request);
            return Ok(response);
        }
    }
}
