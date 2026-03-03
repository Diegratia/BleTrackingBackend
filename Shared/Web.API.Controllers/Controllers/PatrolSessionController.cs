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
using System.Text.Json;

namespace Web.API.Controllers.Controllers
{
    [MinLevel(LevelPriority.Primary)]
    [Route("api/patrol-session")]
    [ApiController]
    public class PatrolSessionController : ControllerBase
    {
        private readonly IPatrolSessionService _PatrolSessionService;

        public PatrolSessionController(IPatrolSessionService PatrolSessionService)
        {
            _PatrolSessionService = PatrolSessionService;
        }

        // GET: api/PatrolSession
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var session = await _PatrolSessionService.GetAllAsync();
            return Ok(ApiResponse.Success("Patrol Session retrieved successfully", session));
        }
        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllLookUpAsync()
        {
            var session = await _PatrolSessionService.GetAllLookUpAsync();
            return Ok(ApiResponse.Success("Patrol Session retrieved successfully", session));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var session = await _PatrolSessionService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Patrol Session retrieved successfully", session!));
        }
        [HttpPost("{id}/stop")]
        public async Task<IActionResult> StopSession(Guid id)
        {
            var session = await _PatrolSessionService.StopAsync(id);
            return Ok(ApiResponse.Success("Patrol Session stopped successfully", session));
        }

        // POST: api/PatrolArea
        [HttpPost("start")]
        public async Task<IActionResult> Create([FromBody] PatrolSessionStartDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
            var session = await _PatrolSessionService.CreateAsync(dto);
            return StatusCode(201, ApiResponse.Created("Patrol Session created successfully", session));
        }


        [HttpPost("{filter}")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new PatrolSessionFilter();

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.BadRequest("Invalid filter parameters"));
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<PatrolSessionFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PatrolSessionFilter();
            }

            var result = await _PatrolSessionService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Patrol Session filtered successfully", result));
        }

    }
}