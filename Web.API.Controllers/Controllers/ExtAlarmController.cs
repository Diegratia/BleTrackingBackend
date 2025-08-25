using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtAlarmController : ControllerBase
    {
        [HttpPost("hit")]
        public IActionResult Hit([FromBody] JsonElement payload)
        {
            // Log the payload to console (or a logging service)
            Console.WriteLine($"post: {payload}");

            return Ok(new
            {
                status = "ok",
                payload
            });
        }
    }
}



