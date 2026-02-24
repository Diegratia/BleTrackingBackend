using System;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    [Route("api/evacuation-alert")]
    [ApiController]
    [MinLevel(LevelPriority.PrimaryAdmin)]
    public class EvacuationAlertController : ControllerBase
    {
        private readonly IEvacuationAlertService _service;

        public EvacuationAlertController(IEvacuationAlertService service)
        {
            _service = service;
        }

        // GET: api/EvacuationAlert
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var alerts = await _service.GetAllAsync();
            return Ok(ApiResponse.Success("Evacuation Alerts retrieved successfully", alerts));
        }

        // GET: api/EvacuationAlert/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var alert = await _service.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Evacuation Alert retrieved successfully", alert));
        }

        // GET: api/EvacuationAlert/{id}/summary
        [HttpGet("{id}/summary")]
        public async Task<IActionResult> GetSummary(Guid id)
        {
            var summary = await _service.GetSummaryAsync(id);
            return Ok(ApiResponse.Success("Evacuation summary retrieved successfully", summary));
        }

        // GET: api/EvacuationAlert/{id}/person-status
        [HttpGet("{id}/person-status")]
        public async Task<IActionResult> GetPersonStatus(Guid id)
        {
            var personStatus = await _service.GetPersonStatusAsync(id);
            return Ok(ApiResponse.Success("Evacuation person status retrieved successfully", personStatus));
        }

        // POST: api/EvacuationAlert
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EvacuationAlertCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
            var alert = await _service.CreateAsync(dto);
            return StatusCode(201, ApiResponse.Created("Evacuation Alert created successfully", alert));
        }

        // PUT: api/EvacuationAlert/{id}
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] EvacuationAlertUpdateDto dto)
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
            return StatusCode(204, ApiResponse.NoContent("Evacuation Alert updated successfully"));
        }

        // DELETE: api/EvacuationAlert/{id}
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return StatusCode(204, ApiResponse.NoContent("Evacuation Alert deleted successfully"));
        }

        // POST: api/EvacuationAlert/{id}/start
        [HttpPost("{id}/start")]
        public async Task<IActionResult> Start(Guid id)
        {
            var alert = await _service.StartAsync(id);
            return Ok(ApiResponse.Success("Evacuation started successfully", alert));
        }

        // POST: api/EvacuationAlert/{id}/pause
        [HttpPost("{id}/pause")]
        public async Task<IActionResult> Pause(Guid id)
        {
            var alert = await _service.PauseAsync(id);
            return Ok(ApiResponse.Success("Evacuation paused successfully", alert));
        }

        // POST: api/EvacuationAlert/{id}/complete
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(Guid id, [FromBody] EvacuationCompleteDto? dto)
        {
            var alert = await _service.CompleteAsync(id, dto?.CompletionNotes);
            return Ok(ApiResponse.Success("Evacuation completed successfully", alert));
        }

        // POST: api/EvacuationAlert/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var alert = await _service.CancelAsync(id);
            return Ok(ApiResponse.Success("Evacuation cancelled successfully", alert));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var filter = new EvacuationAlertFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<EvacuationAlertFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new EvacuationAlertFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Evacuation Alert filtered successfully", result));
        }
    }

    public class EvacuationCompleteDto
    {
        public string? CompletionNotes { get; set; }
    }
}
