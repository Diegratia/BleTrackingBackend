using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Data.ViewModels.ResponseHelper;
using BusinessLogic.Services.Extension.RootExtension;
using Helpers.Consumer;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    [MinLevel(LevelPriority.Primary)]
    [Route("api/patrol-area")]
    [ApiController]

    public class PatrolAreaController : ControllerBase
    {
        private readonly IPatrolAreaService _PatrolAreaService;

        public PatrolAreaController(IPatrolAreaService PatrolAreaService)
        {
            _PatrolAreaService = PatrolAreaService;
        }

        // GET: api/PatrolArea
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var patrolareas = await _PatrolAreaService.GetAllAsync();
            return Ok(ApiResponse.Success("Patrol Area retrieved successfully", patrolareas));
        }
        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllLookUpAsync()
        {
            var patrolareas = await _PatrolAreaService.GetAllLookUpAsync();
            return Ok(ApiResponse.Success("Patrol Area retrieved successfully", patrolareas));
        }

        // GET: api/PatrolArea/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var security = await _PatrolAreaService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Patrol Area retrieved successfully", security));
        }


        // POST: api/PatrolArea
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PatrolAreaCreateDto PatrolAreaDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
            var createdSecurity = await _PatrolAreaService.CreateAsync(PatrolAreaDto);
            return StatusCode(201, ApiResponse.Created("Patrol Area created successfully", createdSecurity));
        }

        [HttpDelete("{id}")]
        // DELETE: api/PatrolArea/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            await _PatrolAreaService.DeleteAsync(id);
            return StatusCode(204, ApiResponse.NoContent("Patrol Area deleted successfully"));
        }


        [HttpPost("{filter}")]
        public async Task<IActionResult> Filter([FromBody] DataTablesRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.BadRequest("Invalid filter parameters"));

            var result = await _PatrolAreaService.FilterAsync(request);
            return Ok(ApiResponse.Paginated("Patrol Area filtered successfully", result));
        }


        // PUT: api/PatrolArea/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PatrolAreaUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var patrolArea = await _PatrolAreaService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse.Success("Patrol Area updated successfully", patrolArea));
        }
    }
}