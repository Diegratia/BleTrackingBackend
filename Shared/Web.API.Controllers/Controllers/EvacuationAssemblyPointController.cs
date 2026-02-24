using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Web.API.Controllers.Controllers
{
    [Route("api/evacuation-assembly-point")]
    [ApiController]
    [MinLevel(LevelPriority.PrimaryAdmin)]
    public class EvacuationAssemblyPointController : ControllerBase
    {
        private readonly IEvacuationAssemblyPointService _service;

        public EvacuationAssemblyPointController(IEvacuationAssemblyPointService service)
        {
            _service = service;
        }

        // GET: api/EvacuationAssemblyPoint
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var assemblyPoints = await _service.GetAllAsync();
            return Ok(ApiResponse.Success("Evacuation Assembly Points retrieved successfully", assemblyPoints));
        }

        // GET: api/EvacuationAssemblyPoint/by-floorplan/{floorplanId}
        [HttpGet("by-floorplan/{floorplanId}")]
        public async Task<IActionResult> GetByFloorplanId(Guid floorplanId)
        {
            var assemblyPoints = await _service.GetByFloorplanIdAsync(floorplanId);
            return Ok(ApiResponse.Success("Evacuation Assembly Points retrieved successfully", assemblyPoints));
        }

        // GET: api/EvacuationAssemblyPoint/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var assemblyPoint = await _service.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Evacuation Assembly Point retrieved successfully", assemblyPoint));
        }

        // POST: api/EvacuationAssemblyPoint
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EvacuationAssemblyPointCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
            var assemblyPoint = await _service.CreateAsync(dto);
            return StatusCode(201, ApiResponse.Created("Evacuation Assembly Point created successfully", assemblyPoint));
        }

        // PUT: api/EvacuationAssemblyPoint/{id}
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] EvacuationAssemblyPointUpdateDto dto)
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
            return StatusCode(204, ApiResponse.NoContent("Evacuation Assembly Point updated successfully"));
        }

        // DELETE: api/EvacuationAssemblyPoint/{id}
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return StatusCode(204, ApiResponse.NoContent("Evacuation Assembly Point deleted successfully"));
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

            var filter = new EvacuationAssemblyPointFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<EvacuationAssemblyPointFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new EvacuationAssemblyPointFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Evacuation Assembly Point filtered successfully", result));
        }
    }
}
