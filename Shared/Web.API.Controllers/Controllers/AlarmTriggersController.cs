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

        // TODO: Engine belum support - comment dulu
            [HttpPut("{id}/acknowledge")]
            public async Task<IActionResult> Acknowledge(Guid id)
            {
                await _service.AcknowledgeAsync(id);
                return Ok(ApiResponse.Success("Alarm acknowledged successfully"));
            }

        [HttpPut("{id}/dispatched")]
        public async Task<IActionResult> Dispatched(Guid id, [FromBody] AlarmDispatchDto dto)
        {
            if (dto == null || dto.SecurityId == Guid.Empty)
            {
                return BadRequest(ApiResponse.BadRequest("SecurityId is required"));
            }

            await _service.DispatchAsync(id, dto.SecurityId);
            return Ok(ApiResponse.Success("Security dispatched to location"));
        }

        [HttpPut("{id}/arrived")]
        public async Task<IActionResult> Arrived(Guid id)
        {
            await _service.ArrivedAsync(id);
            return Ok(ApiResponse.Success("Security arrived at location"));
        }
    }
}