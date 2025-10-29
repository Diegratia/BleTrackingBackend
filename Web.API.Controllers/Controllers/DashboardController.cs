using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
         private readonly IDashboardService _service;
         public DashboardController(IDashboardService service) { _service = service; }

        [HttpGet("count-summary")]
        public async Task<IActionResult> GetSummary()
        {
            var data = await _service.GetSummaryAsync();
            return Ok(data);
        }
    }
}