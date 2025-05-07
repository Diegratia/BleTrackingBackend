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
    public class MstDistrictController : ControllerBase
    {
        private readonly IMstDistrictService _mstDistrictService;

        public MstDistrictController(IMstDistrictService mstDistrictService)
        {
            _mstDistrictService = mstDistrictService;
        }

        // GET: api/MstDistrict
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var districts = await _mstDistrictService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Districts retrieved successfully",
                    collection = new { data = districts },
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

        // GET: api/MstDistrict/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var district = await _mstDistrictService.GetByIdAsync(id);
                if (district == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "District not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "District retrieved successfully",
                    collection = new { data = district },
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

        // POST: api/MstDistrict
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstDistrictCreateDto mstDistrictDto)
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
                var createdDistrict = await _mstDistrictService.CreateAsync(mstDistrictDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "District created successfully",
                    collection = new { data = createdDistrict },
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

        // PUT: api/MstDistrict/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstDistrictUpdateDto mstDistrictDto)
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
                await _mstDistrictService.UpdateAsync(id, mstDistrictDto);
                return Ok(new
                {
                    success = true,
                    msg = "District updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "District not found",
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

        // DELETE: api/MstDistrict/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _mstDistrictService.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "District deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "District not found",
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