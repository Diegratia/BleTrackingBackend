using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    [MinLevel(LevelPriority.PrimaryAdmin)]
    [Route("api/[controller]")]
    [ApiController]
    public class OverpopulatingController : ControllerBase
    {
        private readonly IOverpopulatingService _service;

        public OverpopulatingController(IOverpopulatingService service)
        {
            _service = service;
        }

        // GET: api/Overpopulating
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var overpopulatings = await _service.GetAllAsync();
            return Ok(ApiResponse.Success("Overpopulatings retrieved successfully", overpopulatings));
        }

        // GET: api/Overpopulating/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var overpopulating = await _service.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Overpopulating retrieved successfully", overpopulating));
        }

        // POST: api/Overpopulating
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OverpopulatingCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var overpopulating = await _service.CreateAsync(dto);
            return StatusCode(201, ApiResponse.Created("Overpopulating created successfully", overpopulating));
        }

        // PUT: api/Overpopulating/{id}
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OverpopulatingUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse.Success("Overpopulating updated successfully"));
        }

        // DELETE: api/Overpopulating/{id}
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse.Success("Overpopulating deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            }

            var filter = new OverpopulatingFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<OverpopulatingFilter>(
                    request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new OverpopulatingFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Data retrieved", result));
        }
    }
}

