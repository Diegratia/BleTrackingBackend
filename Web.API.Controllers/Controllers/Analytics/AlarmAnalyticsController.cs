using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels.AlarmAnalytics;
using System.Threading.Tasks;

namespace Web.API.Controllers.Controllers.Analytics
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("RequireAllAndUserCreated")]
    public class AlarmAnalyticsController : ControllerBase
    {
        private readonly IAlarmAnalyticsService _service;

        public AlarmAnalyticsController(IAlarmAnalyticsService service)
        {
            _service = service;
        }

        [HttpPost("daily")]
        public async Task<IActionResult> GetDaily([FromBody] AlarmAnalyticsRequest request)
        {
            var response = await _service.GetDailySummaryAsync(request);
            return Ok(response);
        }

        [HttpPost("area")]
        public async Task<IActionResult> GetArea([FromBody] AlarmAnalyticsRequest request)
        {
            var response = await _service.GetAreaSummaryAsync(request);
            return Ok(response);
        }

        [HttpPost("operator")]
        public async Task<IActionResult> GetOperator([FromBody] AlarmAnalyticsRequest request)
        {
            var response = await _service.GetOperatorSummaryAsync(request);
            return Ok(response);
        }

        [HttpPost("status")]
        public async Task<IActionResult> GetStatus([FromBody] AlarmAnalyticsRequest request)
        {
            var response = await _service.GetStatusSummaryAsync(request);
            return Ok(response);
        }

        [HttpPost("building")]
        public async Task<IActionResult> GetBuilding([FromBody] AlarmAnalyticsRequest request)
        {
            var response = await _service.GetBuildingSummaryAsync(request);
            return Ok(response);
        }

        [HttpPost("visitor")]
        public async Task<IActionResult> GetVisitor([FromBody] AlarmAnalyticsRequest request)
        {
            var response = await _service.GetVisitorSummaryAsync(request);
            return Ok(response);
        }

        [HttpPost("floor")]
        public async Task<IActionResult> GetFloor([FromBody] AlarmAnalyticsRequest request)
        {
            var response = await _service.GetFloorSummaryAsync(request);
            return Ok(response);
        }

        [HttpPost("weekly-trend")]
        public async Task<IActionResult> GetWeeklyTrend([FromBody] AlarmAnalyticsRequest request)
        {
            var response = await _service.GetWeeklyTrendAsync(request);
            return Ok(response);
        }

        [HttpPost("duration")]
        public async Task<IActionResult> GetDuration([FromBody] AlarmAnalyticsRequest request)
        {
            var response = await _service.GetDurationSummaryAsync(request);
            return Ok(response);
        }

        [HttpPost("trend-by-action")]
        public async Task<IActionResult> GetTrendByAction([FromBody] AlarmAnalyticsRequest request)
        {
            var response = await _service.GetTrendByActionAsync(request);
            return Ok(response);
        }
    }
}
