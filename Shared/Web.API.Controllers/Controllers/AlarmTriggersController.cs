using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Data.ViewModels.ResponseHelper;

namespace Web.API.Controllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
    public class AlarmTriggersController : ControllerBase
    {
        private readonly IAlarmTriggersService _service;

        public AlarmTriggersController(IAlarmTriggersService service)
        {
            _service = service;
        }

        [HttpGet("open")]
        [AllowAnonymous]

        public async Task<IActionResult> OpenGetAll()
        {
            var alarms = await _service.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Alarm Triggers retrieved successfully", alarms));
        }

         [HttpGet]
        public async Task<IActionResult> GetAll()
        {
        
                var alarms = await _service.GetAllAsync();
                return Ok(ApiResponse.Success("Alarm Events retrieved successfully", alarms));
        }
        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllLookUp()
        {

            var alarms = await _service.GetAllLookUpAsync();
                return Ok(ApiResponse.Success("Alarm Events retrieved successfully", alarms));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AlarmTriggersUpdateDto dto)
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
            return Ok(ApiResponse.Success("Alarm updated successfully"));
        }

        [HttpPut("tag/{beaconId}")]
        public async Task<IActionResult> UpdateAlarmStatus(string beaconId, [FromBody] AlarmTriggersUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                await _service.UpdateAlarmStatusAsync(beaconId, dto);
                return Ok(new
                {
                    success = true,
                    msg = "Trigger updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Card not found",
                    collection = new { data = (object)null },
                    code = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpPost("{filter}")]
        public async Task<IActionResult> Filter([FromBody] DataTablesRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.BadRequest("Invalid filter parameters"));
            var result = await _service.FilterAsync(request);
            return Ok(ApiResponse.Paginated("Alarm Triggers filtered successfully", result));
        }
    }
}