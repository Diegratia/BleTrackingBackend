using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FloorplanMaskedAreaController : ControllerBase
    {
        private readonly IFloorplanMaskedAreaService _FloorplanMaskedAreaService;

        public FloorplanMaskedAreaController(IFloorplanMaskedAreaService FloorplanMaskedAreaService)
        {
            _FloorplanMaskedAreaService = FloorplanMaskedAreaService;
        }

        // GET: api/FloorplanMaskedArea
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var areas = await _FloorplanMaskedAreaService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Areas retrieved successfully",
                    collection = new { data = areas },
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

        // GET: api/FloorplanMaskedArea/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var area = await _FloorplanMaskedAreaService.GetByIdAsync(id);
                if (area == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Area not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Area retrieved successfully",
                    collection = new { data = area },
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

        // POST: api/FloorplanMaskedArea
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FloorplanMaskedAreaCreateDto FloorplanMaskedAreaDto)
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
                var createdArea = await _FloorplanMaskedAreaService.CreateAsync(FloorplanMaskedAreaDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Area created successfully",
                    collection = new { data = createdArea },
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

        // PUT: api/FloorplanMaskedArea/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FloorplanMaskedAreaUpdateDto FloorplanMaskedAreaDto)
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
                await _FloorplanMaskedAreaService.UpdateAsync(id, FloorplanMaskedAreaDto);
                return Ok(new
                {
                    success = true,
                    msg = "Area updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Area not found",
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

        // DELETE: api/FloorplanMaskedArea/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _FloorplanMaskedAreaService.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Area deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Area not found",
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
    }
}