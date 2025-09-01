using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

         [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var alarms = await _service.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Alarm Triggers retrieved successfully",
                    collection = new { data = alarms },
                    code = 200
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
    }
}