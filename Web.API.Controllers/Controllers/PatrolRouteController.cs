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
    [Route("api/patrol-route")]
    [ApiController]

    public class PatrolRouteController : ControllerBase
    {
        private readonly IPatrolRouteService _PatrolRouteService;

        public PatrolRouteController(IPatrolRouteService PatrolRouteService)
        {
            _PatrolRouteService = PatrolRouteService;
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        // GET: api/PatrolRoute
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var patrolroutes = await _PatrolRouteService.GetAllAsync();
            return Ok(ApiResponse.Success("Patrol Route retrieved successfully", patrolroutes));
        }
        // [HttpGet("lookup")]
        // public async Task<IActionResult> GetAllLookUpAsync()
        // {
        //     var patrolroutes = await _PatrolRouteService.GetAllLookUpAsync();
        //     return Ok(ApiResponse.Success("Patrol Route retrieved successfully", patrolroutes));
        // }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        // GET: api/PatrolRoute/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var security = await _PatrolRouteService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Patrol Route retrieved successfully", security));
        }


        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        // POST: api/PatrolRoute
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PatrolRouteCreateDto PatrolRouteDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
            var createdSecurity = await _PatrolRouteService.CreateAsync(PatrolRouteDto);
            return StatusCode(201, ApiResponse.Created("Patrol Route created successfully", createdSecurity));
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        [HttpDelete("{id}")]
        // DELETE: api/PatrolRoute/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            await _PatrolRouteService.DeleteAsync(id);
            return StatusCode(204, ApiResponse.NoContent("Patrol Route deleted successfully"));
        }


        // [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        // [HttpPost("{filter}")]
        // public async Task<IActionResult> Filter([FromBody] DataTablesRequest request)
        // {
        //     if (!ModelState.IsValid)
        //         return BadRequest(ApiResponse.BadRequest("Invalid filter parameters"));

        //     var result = await _PatrolRouteService.FilterAsync(request);
        //     return Ok(ApiResponse.Paginated("Patrol Route filtered successfully", result));
        // }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        // PUT: api/PatrolRoute/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PatrolRouteUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var patrolRoute = await _PatrolRouteService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse.Success("Patrol Route updated successfully", patrolRoute));
        }
    }
}