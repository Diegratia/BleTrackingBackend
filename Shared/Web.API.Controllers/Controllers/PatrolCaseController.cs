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
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web.API.Controllers.Controllers
{
    [MinLevel(LevelPriority.Primary)]
    [Route("api/patrol-case")]
    [ApiController]

    public class PatrolCaseController : ControllerBase
    {
        private readonly IPatrolCaseService _PatrolCaseService;

        public PatrolCaseController(IPatrolCaseService PatrolCaseService)
        {
            _PatrolCaseService = PatrolCaseService;
        }

        // GET: api/PatrolCase
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var patrolAreas = await _PatrolCaseService.GetAllAsync();
            return Ok(ApiResponse.Success("Patrol Case retrieved successfully", patrolAreas));
        }

        // GET: api/PatrolCase/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var security = await _PatrolCaseService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Patrol Case retrieved successfully", security));
        }

        // POST: api/PatrolCase
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PatrolCaseCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
            var create = await _PatrolCaseService.CreateAsync(createDto);
            return StatusCode(201, ApiResponse.Created("Patrol Case created successfully (auto-submitted)", create));
        }

        // DELETE: api/PatrolCase/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _PatrolCaseService.DeleteAsync(id);
            return StatusCode(200, ApiResponse.Success("Patrol Case deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new PatrolCaseFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<PatrolCaseFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PatrolCaseFilter();
            }

            var result = await _PatrolCaseService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Data retrieved", result));
        }

        // PUT: api/PatrolCase/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PatrolCaseUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var patrolRoute = await _PatrolCaseService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse.Success("Patrol Case updated successfully", patrolRoute));
        }

        // POST: api/patrol-case/{id}/approve
        [HttpPost("{id}/approve")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> Approve(Guid id, [FromBody] PatrolCaseApprovalDto dto)
        {
            var result = await _PatrolCaseService.ApproveAsync(id, dto);
            return Ok(ApiResponse.Success("Patrol Case approved", result));
        }

        // POST: api/patrol-case/{id}/reject
        [HttpPost("{id}/reject")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> Reject(Guid id, [FromBody] PatrolCaseApprovalDto dto)
        {
            var result = await _PatrolCaseService.RejectAsync(id, dto);
            return Ok(ApiResponse.Success("Patrol Case rejected", result));
        }

        // POST: api/patrol-case/{id}/close
        [HttpPost("{id}/close")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> Close(Guid id, [FromBody] PatrolCaseCloseDto dto)
        {
            var result = await _PatrolCaseService.CloseAsync(id, dto);
            return Ok(ApiResponse.Success("Patrol Case closed", result));
        }

        // DELETE: api/patrol-case/attachments
        [HttpDelete("/attachments")]
        public async Task<IActionResult> DeleteAttachment([FromQuery] Guid caseId,[FromQuery] Guid attachmentId)
        {
            await _PatrolCaseService.DeleteAttachmentAsync(caseId, attachmentId);
            return Ok(ApiResponse.Success("Attachment deleted successfully"));
        }

    }
}
