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
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web.API.Controllers.Controllers
{
    // [MinLevel(LevelPriority.PrimaryAdmin)]
    [Route("api/patrol-case")]
    [ApiController]

    public class PatrolCaseController : ControllerBase
    {
        private readonly IPatrolCaseService _PatrolCaseService;

        public PatrolCaseController(IPatrolCaseService PatrolCaseService)
        {
            _PatrolCaseService = PatrolCaseService;
        }

        // // GET: api/PatrolCase
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var patrolAreas = await _PatrolCaseService.GetAllAsync();
            return Ok(ApiResponse.Success("Patrol Case retrieved successfully", patrolAreas));
        }
        // // GET: api/PatrolCase
        // [HttpGet("lookup")]
        // public async Task<IActionResult> GetAllLookUpAsync()
        // {
        //     var patrolroutes = await _PatrolRouteService.GetAllLookUpAsync();
        //     return Ok(ApiResponse.Success("Patrol Route retrieved successfully", patrolroutes));
        // }

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
            return StatusCode(201, ApiResponse.Created("Patrol Case created successfully", create));
        }

        // // DELETE: api/PatrolCase/{id}
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
                // Deserialisasi ini akan memetakan string "Incident" ke Enum CaseType.Incident secara otomatis
                filter = JsonSerializer.Deserialize<PatrolCaseFilter>(request.Filters.GetRawText(), 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PatrolCaseFilter();
            }

            var result = await _PatrolCaseService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Data retrieved", result));
        }

        // PUT: api/PatrolCase/{id}
        // [HttpPut("{id}")]
        // public async Task<IActionResult> Update(Guid id, [FromBody] PatrolRouteUpdateDto updateDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         var errors = ModelState.ToDictionary(
        //             kvp => kvp.Key,
        //             kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
        //         );
        //         return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
        //     }

        //     var patrolRoute = await _PatrolRouteService.UpdateAsync(id, updateDto);
        //     return Ok(ApiResponse.Success("Patrol Route updated successfully", patrolRoute));
        // }
    }
}