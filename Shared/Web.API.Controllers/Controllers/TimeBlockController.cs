using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeBlockController : ControllerBase
    {
        private readonly ITimeBlockService _service;

        public TimeBlockController(ITimeBlockService service)
        {
            _service = service;
        }
        // POST: api/MstBrand
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TimeBlockCreateDto dto)
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
                var timeBlock = await _service.CreateAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "TimeBlock created successfully",
                    collection = new { data = timeBlock },
                    code = 201
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