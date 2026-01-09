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
    [Route("api/patrol-area")]
    [ApiController]

    public class PatrolAreaController : ControllerBase
    {
        private readonly IPatrolAreaService _PatrolAreaService;

        public PatrolAreaController(IPatrolAreaService PatrolAreaService)
        {
            _PatrolAreaService = PatrolAreaService;
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        // GET: api/PatrolArea
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var patrolareas = await _PatrolAreaService.GetAllAsync();
            return Ok(ApiResponse.Success("Securities retrieved successfully", patrolareas));
        }
        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllLookUpAsync()
        {
            var patrolareas = await _PatrolAreaService.GetAllLookUpAsync();
            return Ok(ApiResponse.Success("Securities retrieved successfully", patrolareas));
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        // GET: api/PatrolArea/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var security = await _PatrolAreaService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("PatrolArea retrieved successfully", security));
        }


        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        // POST: api/PatrolArea
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PatrolAreaCreateDto PatrolAreaDto)
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
            return StatusCode(201, ApiResponse.Created("PatrolArea created successfully", createdSecurity));

        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        [HttpDelete("{id}")]
        // DELETE: api/PatrolArea/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            await _PatrolAreaService.DeleteAsync(id);
            return StatusCode(204, ApiResponse.NoContent("PatrolArea deleted successfully"));
        }


        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole")]
        [HttpPost("{filter}")]
        public async Task<IActionResult> Filter([FromBody] DataTablesRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.BadRequest("Invalid filter parameters"));

            var result = await _PatrolAreaService.FilterAsync(request);
            return Ok(ApiResponse.Paginated("Securities filtered successfully", result));
        }

        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        // PUT: api/PatrolArea/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] PatrolAreaUpdateDto updateDto)
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
            return Ok(ApiResponse.Success("PatrolArea updated successfully", patrolArea));
        }





        // [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        // // POST: api/PatrolArea
        // [HttpPost("{id}/blacklist")]
        // public async Task<IActionResult> BlacklistSecurity(Guid id, [FromBody] BlacklistReasonDto dto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
        //         return BadRequest(new
        //         {
        //             success = false,
        //             msg = "Validation failed: " + string.Join(", ", errors),
        //             collection = new { data = (object)null },
        //             code = 400
        //         });
        //     }
        //      try
        //     {
        //         await _PatrolAreaService.SecurityBlacklistAsync(id, dto);
        //         return Ok(new
        //         {
        //             success = true,
        //             msg = "PatrolArea Blacklisted successfully",
        //             collection = new { data = (object)null },
        //             code = 204
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             success = false,
        //             msg = $"Internal server error: {ex.Message}",
        //             collection = new { data = (object)null },
        //             code = 500
        //         });
        //     }
        // }

        // [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        // // POST: api/PatrolArea
        // [HttpPost("{id}/unblacklist")]
        // public async Task<IActionResult> UnBlacklistSecurity(Guid id)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
        //         return BadRequest(new
        //         {
        //             success = false,
        //             msg = "Validation failed: " + string.Join(", ", errors),
        //             collection = new { data = (object)null },
        //             code = 400
        //         });
        //     }
        //      try
        //     {
        //         await _PatrolAreaService.UnBlacklistSecurityAsync(id);
        //         return Ok(new
        //         {
        //             success = true,
        //             msg = "PatrolArea Unblacklist successfully",
        //             collection = new { data = (object)null },
        //             code = 204
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             success = false,
        //             msg = $"Internal server error: {ex.Message}",
        //             collection = new { data = (object)null },
        //             code = 500
        //         });
        //     }
        // }
    }
}