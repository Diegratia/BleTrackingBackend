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

namespace Web.API.Controllers.Controllers
{
    [Route("api/patrol-assignment")]
    [ApiController]
    public class PatrolAssignmentController : ControllerBase
    {
        private readonly IPatrolAssignmentService _PatrolAssignmentService;

        public PatrolAssignmentController(IPatrolAssignmentService PatrolAssignmentService)
        {
            _PatrolAssignmentService = PatrolAssignmentService;
        }

        // GET: api/PatrolAssignment
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var assignment = await _PatrolAssignmentService.GetAllAsync();
            return Ok(ApiResponse.Success("Patrol Assignment retrieved successfully", assignment));
        }
        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllLookUpAsync()
        {
            var assignment = await _PatrolAssignmentService.GetAllLookUpAsync();
            return Ok(ApiResponse.Success("Patrol Assignment retrieved successfully", assignment));
        }

        // GET: api/PatrolArea/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var assignment = await _PatrolAssignmentService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Patrol Assignment retrieved successfully", assignment));
        }

        // POST: api/PatrolArea
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PatrolAssignmentCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
            var createdAssignment = await _PatrolAssignmentService.CreateAsync(dto);
            return StatusCode(201, ApiResponse.Created("Patrol Assignment created successfully", createdAssignment));
        }

        [HttpDelete("{id}")]
        // DELETE: api/PatrolArea/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            await _PatrolAssignmentService.DeleteAsync(id);
            return StatusCode(204, ApiResponse.NoContent("Patrol Assignment deleted successfully"));
        }


        [HttpPost("{filter}")]
        public async Task<IActionResult> Filter([FromBody] DataTablesRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.BadRequest("Invalid filter parameters"));

            var result = await _PatrolAssignmentService.FilterAsync(request);
            return Ok(ApiResponse.Paginated("Patrol Assignment filtered successfully", result));
        }

        // PUT: api/PatrolArea/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PatrolAssignmentUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var patrolAssignment = await _PatrolAssignmentService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse.Success("Patrol Assignment updated successfully", patrolAssignment));
        }
    }
}