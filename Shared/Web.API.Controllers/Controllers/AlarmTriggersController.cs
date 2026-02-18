using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Data.ViewModels.ResponseHelper;
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;
using System.Text.Json.Serialization;

namespace Web.API.Controllers.Controllers
{
    [MinLevel(LevelPriority.Primary)]
    [ApiController]
    [Route("api/[controller]")]
    public class AlarmTriggersController : ControllerBase
    {
        private readonly IAlarmTriggersService _service;

        public AlarmTriggersController(IAlarmTriggersService service)
        {
            _service = service;
        }

        private string CurrentUsername => User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            var alarms = await _service.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Alarm Triggers retrieved successfully", alarms));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var alarms = await _service.GetAllAsync();
            return Ok(ApiResponse.Success("Alarm Events retrieved successfully", alarms));
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllLookUp()
        {
            var alarms = await _service.GetAllLookUpAsync();
            return Ok(ApiResponse.Success("Alarm Events retrieved successfully", alarms));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AlarmTriggersUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse.Success("Alarm updated successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new AlarmTriggersFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<AlarmTriggersFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AlarmTriggersFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Alarm Triggers filtered successfully", result));
        }

        [HttpGet("{id}/timeline")]
        public async Task<IActionResult> GetIncidentTimeline(Guid id)
        {
            var result = await _service.GetIncidentTimelineAsync(id);
            return Ok(result);
        }

        // =====================================================
        // NEW WORKFLOW ENDPOINTS
        // =====================================================

        /// <summary>
        /// Operator acknowledges the alarm
        /// Flow: Idle → Acknowledged
        /// </summary>
        [HttpPut("{id}/acknowledge")]
        public async Task<IActionResult> Acknowledge(Guid id)
        {
            await _service.AcknowledgeAsync(id);
            return Ok(ApiResponse.Success("Alarm acknowledged successfully"));
        }

        /// <summary>
        /// Operator dispatches alarm to specific security
        /// Flow: Acknowledged → Dispatched
        /// </summary>
        [HttpPut("{id}/dispatch")]
        public async Task<IActionResult> Dispatch(Guid id, [FromBody] AlarmDispatchDto dto)
        {
            if (dto == null || dto.AssignedSecurityId == Guid.Empty)
            {
                return BadRequest(ApiResponse.BadRequest("SecurityId is required"));
            }

            await _service.DispatchAsync(id, dto.AssignedSecurityId);
            return Ok(ApiResponse.Success("Security dispatched to location"));
        }

        /// <summary>
        /// Operator puts alarm in waiting queue (no security available)
        /// Flow: Acknowledged → Waiting
        /// </summary>
        [HttpPut("{id}/waiting")]
        public async Task<IActionResult> Waiting(Guid id)
        {
            await _service.WaitingAsync(id);
            return Ok(ApiResponse.Success("Alarm put in waiting queue"));
        }

        /// <summary>
        /// Security accepts the dispatch
        /// Flow: Dispatched → Accepted
        /// </summary>
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> Accept(Guid id)
        {
            await _service.AcceptAsync(id);
            return Ok(ApiResponse.Success("Dispatch accepted successfully"));
        }

        /// <summary>
        /// Security arrives at location
        /// Flow: Accepted → Arrived
        /// </summary>
        [HttpPut("{id}/arrived")]
        public async Task<IActionResult> Arrived(Guid id)
        {
            await _service.ArrivedAsync(id);
            return Ok(ApiResponse.Success("Security arrived at location"));
        }

        /// <summary>
        /// Security completes investigation with result
        /// Flow: Arrived → DoneInvestigated
        /// </summary>
        [HttpPut("{id}/done-investigated")]
        public async Task<IActionResult> DoneInvestigated(Guid id, [FromBody] AlarmInvestigationResultDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Result))
            {
                return BadRequest(ApiResponse.BadRequest("Investigation result is required"));
            }

            await _service.DoneInvestigatedAsync(id, dto.Result);
            return Ok(ApiResponse.Success("Investigation completed successfully"));
        }

        /// <summary>
        /// Operator marks alarm as resolved
        /// Flow: DoneInvestigated → Done (final state)
        /// </summary>
        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> Resolve(Guid id)
        {
            await _service.ResolveAsync(id);
            return Ok(ApiResponse.Success("Alarm resolved successfully"));
        }
    }
}